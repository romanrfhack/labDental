#!/usr/bin/env bash
set -Eeuo pipefail

: "${LDT_RELEASE_ID:?Missing LDT_RELEASE_ID}"
: "${LDT_APP_ROOT:?Missing LDT_APP_ROOT}"
: "${LDT_API_SERVICE:?Missing LDT_API_SERVICE}"
: "${LDT_DB_NAME:?Missing LDT_DB_NAME}"
: "${LDT_ENV_FILE:?Missing LDT_ENV_FILE}"
: "${LDT_SITE_URL:?Missing LDT_SITE_URL}"
: "${LDT_SQLCMD:?Missing LDT_SQLCMD}"

REMOTE_TARBALL="/tmp/labdental-${LDT_RELEASE_ID}.tar.gz"
TMP_DIR="$(mktemp -d)"

BACKEND_RELEASE="${LDT_APP_ROOT}/backend/releases/${LDT_RELEASE_ID}"
FRONTEND_RELEASE="${LDT_APP_ROOT}/frontend/releases/${LDT_RELEASE_ID}"
MIGRATION_DIR="${LDT_APP_ROOT}/migrations/releases"
MIGRATION_FILE="${MIGRATION_DIR}/${LDT_RELEASE_ID}.sql"

PREV_BACKEND="$(readlink -f "${LDT_APP_ROOT}/backend/current" 2>/dev/null || true)"
PREV_FRONTEND="$(readlink -f "${LDT_APP_ROOT}/frontend/current" 2>/dev/null || true)"
SWITCHED="false"

cleanup() {
  rm -rf "$TMP_DIR"
  rm -f "$REMOTE_TARBALL"
}

rollback_on_error() {
  local exit_code=$?
  if [ "$exit_code" -ne 0 ] && [ "$SWITCHED" = "true" ]; then
    echo "ERROR: deploy failed after symlink switch. Attempting rollback..."

    if [ -n "$PREV_BACKEND" ] && [ -d "$PREV_BACKEND" ]; then
      ln -sfn "$PREV_BACKEND" "${LDT_APP_ROOT}/backend/current.rollback"
      mv -Tf "${LDT_APP_ROOT}/backend/current.rollback" "${LDT_APP_ROOT}/backend/current"
    fi

    if [ -n "$PREV_FRONTEND" ] && [ -d "$PREV_FRONTEND" ]; then
      ln -sfn "$PREV_FRONTEND" "${LDT_APP_ROOT}/frontend/current.rollback"
      mv -Tf "${LDT_APP_ROOT}/frontend/current.rollback" "${LDT_APP_ROOT}/frontend/current"
    fi

    systemctl restart "$LDT_API_SERVICE" || true
  fi

  cleanup
  exit "$exit_code"
}

trap rollback_on_error EXIT

echo "=== REMOTE DEPLOY START ==="
echo "release=${LDT_RELEASE_ID}"
echo "app_root=${LDT_APP_ROOT}"
echo "service=${LDT_API_SERVICE}"
echo "db=${LDT_DB_NAME}"
echo "site=${LDT_SITE_URL}"

echo
echo "=== PREPARE DIRECTORIES ==="
mkdir -p \
  "${LDT_APP_ROOT}/backend/releases" \
  "${LDT_APP_ROOT}/frontend/releases" \
  "$MIGRATION_DIR" \
  "${LDT_APP_ROOT}/logs" \
  "${LDT_APP_ROOT}/shared"

test -f "$REMOTE_TARBALL"

echo
echo "=== EXTRACT PACKAGE ==="
tar -xzf "$REMOTE_TARBALL" -C "$TMP_DIR"

test -f "${TMP_DIR}/backend/LaboratorioTlahuac.Api.dll"
test -f "${TMP_DIR}/frontend/index.html"
test -f "${TMP_DIR}/migrations.sql"

echo
echo "=== INSTALL RELEASE FILES ==="
rm -rf "$BACKEND_RELEASE" "$FRONTEND_RELEASE"
mkdir -p "$BACKEND_RELEASE" "$FRONTEND_RELEASE"

cp -a "${TMP_DIR}/backend/." "$BACKEND_RELEASE/"
cp -a "${TMP_DIR}/frontend/." "$FRONTEND_RELEASE/"
install -m 600 "${TMP_DIR}/migrations.sql" "$MIGRATION_FILE"

chown -R www-data:www-data "$BACKEND_RELEASE" "$FRONTEND_RELEASE"

echo
echo "=== APPLY EF MIGRATIONS ==="
APP_CONN_LINE="$(grep '^ConnectionStrings__DefaultConnection=' "$LDT_ENV_FILE")"
APP_USER="$(echo "$APP_CONN_LINE" | sed -nE 's/.*User Id=([^;]+).*/\1/p')"
APP_PASS="$(echo "$APP_CONN_LINE" | sed -nE 's/.*Password=([^;]+).*/\1/p')"

test -n "$APP_USER"
test -n "$APP_PASS"

"$LDT_SQLCMD" \
  -S 127.0.0.1,14330 \
  -d "$LDT_DB_NAME" \
  -U "$APP_USER" \
  -P "$APP_PASS" \
  -C \
  -b \
  -i "$MIGRATION_FILE"

unset APP_PASS

echo
echo "=== SWITCH CURRENT SYMLINKS ==="
ln -sfn "$BACKEND_RELEASE" "${LDT_APP_ROOT}/backend/current.next"
mv -Tf "${LDT_APP_ROOT}/backend/current.next" "${LDT_APP_ROOT}/backend/current"

ln -sfn "$FRONTEND_RELEASE" "${LDT_APP_ROOT}/frontend/current.next"
mv -Tf "${LDT_APP_ROOT}/frontend/current.next" "${LDT_APP_ROOT}/frontend/current"

SWITCHED="true"

echo
echo "=== RESTART SERVICE ==="
systemctl restart "$LDT_API_SERVICE"
sleep 5

echo
echo "=== HEALTH CHECK ==="
curl -fsS "${LDT_SITE_URL}/health"
echo

echo
echo "=== SERVICE STATUS ==="
systemctl is-active "$LDT_API_SERVICE"

echo
echo "=== PRUNE OLD RELEASES ==="
find "${LDT_APP_ROOT}/backend/releases" -mindepth 1 -maxdepth 1 -type d -printf '%T@ %p\n' \
  | sort -rn | awk 'NR>5 {print $2}' | xargs -r rm -rf

find "${LDT_APP_ROOT}/frontend/releases" -mindepth 1 -maxdepth 1 -type d -printf '%T@ %p\n' \
  | sort -rn | awk 'NR>5 {print $2}' | xargs -r rm -rf

find "$MIGRATION_DIR" -mindepth 1 -maxdepth 1 -type f -name '*.sql' -printf '%T@ %p\n' \
  | sort -rn | awk 'NR>10 {print $2}' | xargs -r rm -f

echo
echo "=== REMOTE DEPLOY OK ==="
