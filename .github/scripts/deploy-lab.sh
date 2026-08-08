#!/usr/bin/env bash
set -Eeuo pipefail

: "${LDT_RELEASE_ID:?Missing LDT_RELEASE_ID}"
: "${LDT_APP_ROOT:?Missing LDT_APP_ROOT}"
: "${LDT_API_SERVICE:?Missing LDT_API_SERVICE}"
: "${LDT_DB_NAME:?Missing LDT_DB_NAME}"
: "${LDT_ENV_FILE:?Missing LDT_ENV_FILE}"
: "${LDT_SITE_URL:?Missing LDT_SITE_URL}"
: "${LDT_SQLCMD:?Missing LDT_SQLCMD}"
: "${LDT_LOCAL_HEALTH_URL:?Missing LDT_LOCAL_HEALTH_URL}"
: "${LDT_PUBLIC_HEALTH_URL:?Missing LDT_PUBLIC_HEALTH_URL}"

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
  rm -rf "$TMP_DIR" || true
  rm -f "$REMOTE_TARBALL" || true
}

wait_for_health() {
  local name="$1"
  local url="$2"
  local max_attempts="$3"
  local sleep_seconds="$4"
  local attempt
  local http_status

  for ((attempt = 1; attempt <= max_attempts; attempt++)); do
    http_status=""

    if http_status="$(curl -fsS \
      --connect-timeout 2 \
      --max-time 5 \
      --output /dev/null \
      --write-out '%{http_code}' \
      "$url")" && [ "$http_status" = "200" ]; then
      echo "Health check '${name}' succeeded with HTTP 200 on attempt ${attempt}/${max_attempts}."
      return 0
    fi

    echo "Health check '${name}' failed on attempt ${attempt}/${max_attempts} (HTTP ${http_status:-unavailable})."

    if [ "$attempt" -lt "$max_attempts" ]; then
      sleep "$sleep_seconds"
    fi
  done

  echo "ERROR: health check '${name}' did not return HTTP 200 after ${max_attempts} attempts."
  return 1
}

print_deploy_diagnostics() {
  echo
  echo "=== DEPLOY FAILURE DIAGNOSTICS ==="
  systemctl status "$LDT_API_SERVICE" --no-pager -l || true
  journalctl -u "$LDT_API_SERVICE" -n 120 --no-pager || true

  printf 'backend/current -> '
  readlink -f "${LDT_APP_ROOT}/backend/current" || echo "unavailable"

  printf 'frontend/current -> '
  readlink -f "${LDT_APP_ROOT}/frontend/current" || echo "unavailable"
}

rollback_on_error() {
  local exit_code=$?
  local rollback_failed="false"

  if [ "$exit_code" -ne 0 ] && [ "$SWITCHED" = "true" ]; then
    print_deploy_diagnostics

    echo
    echo "ERROR: deploy failed after symlink switch. Attempting rollback..."

    if [ -z "$PREV_BACKEND" ] || [ ! -d "$PREV_BACKEND" ]; then
      echo "ERROR: rollback backend release is unavailable."
      rollback_failed="true"
    fi

    if [ -z "$PREV_FRONTEND" ] || [ ! -d "$PREV_FRONTEND" ]; then
      echo "ERROR: rollback frontend release is unavailable."
      rollback_failed="true"
    fi

    if [ "$rollback_failed" = "false" ]; then
      if ! ln -sfn "$PREV_BACKEND" "${LDT_APP_ROOT}/backend/current.rollback" \
        || ! mv -Tf "${LDT_APP_ROOT}/backend/current.rollback" "${LDT_APP_ROOT}/backend/current"; then
        echo "ERROR: failed to restore backend/current during rollback."
        rollback_failed="true"
      fi

      if ! ln -sfn "$PREV_FRONTEND" "${LDT_APP_ROOT}/frontend/current.rollback" \
        || ! mv -Tf "${LDT_APP_ROOT}/frontend/current.rollback" "${LDT_APP_ROOT}/frontend/current"; then
        echo "ERROR: failed to restore frontend/current during rollback."
        rollback_failed="true"
      fi
    fi

    if [ "$rollback_failed" = "false" ]; then
      if ! systemctl restart "$LDT_API_SERVICE"; then
        echo "ERROR: rollback restored symlinks but failed to restart ${LDT_API_SERVICE}."
        rollback_failed="true"
      elif ! wait_for_health "rollback local" "$LDT_LOCAL_HEALTH_URL" 30 3; then
        echo "ERROR: rollback release failed its local health check."
        rollback_failed="true"
      elif ! wait_for_health "rollback public" "$LDT_PUBLIC_HEALTH_URL" 30 3; then
        echo "ERROR: rollback release failed its public health check."
        rollback_failed="true"
      fi
    fi

    if [ "$rollback_failed" = "true" ]; then
      echo "ERROR: rollback failed; manual intervention is required."
      print_deploy_diagnostics
    else
      echo "Rollback completed and the previous release is healthy."
    fi
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

echo
echo "=== HEALTH CHECK ==="
wait_for_health "local" "$LDT_LOCAL_HEALTH_URL" 30 3
wait_for_health "public" "$LDT_PUBLIC_HEALTH_URL" 30 3

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
