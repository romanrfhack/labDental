"""DEV migration preflight and HTTP QA. Never emits credentials or customer data."""
import datetime
import http.cookiejar
import json
import os
import pathlib
import re
import secrets
import subprocess
import sys
import urllib.error
import urllib.request

mode = os.environ.get('LDT_CHECK_MODE', 'preflight')
config = {}
for line in pathlib.Path(os.environ['LDT_ENV_FILE']).read_text().splitlines():
    line = line.strip()
    if line and not line.startswith('#') and '=' in line:
        key, value = line.split('=', 1)
        config[key] = value.strip().strip('\"').strip("'")
connection = config.get('ConnectionStrings__DefaultConnection', '')
parts = dict((k.strip().lower(), v.strip()) for k, v in
             (part.split('=', 1) for part in connection.split(';') if '=' in part))
db = os.environ['LDT_DB_NAME']
if not re.fullmatch(r'[A-Za-z0-9_]+', db) or 'dev' not in db.lower():
    raise SystemExit('Refusing non-DEV database')
if parts.get('database', parts.get('initial catalog')) != db:
    raise SystemExit('Database configuration mismatch')
env = dict(os.environ, SQLCMDPASSWORD=parts['password'])

def sql(query):
    p = subprocess.run([os.environ['LDT_SQLCMD'], '-S', '127.0.0.1,14330',
        '-d', db, '-U', parts['user id'], '-C', '-b', '-h', '-1', '-W', '-Q',
        'SET NOCOUNT ON; ' + query], env=env, capture_output=True, text=True, timeout=180)
    if p.returncode:
        codes = re.findall(r'Msg (\d+)', p.stdout + p.stderr)
        raise RuntimeError('SQL check failed; error numbers=' + ','.join(codes))
    return p.stdout.strip()

expected = {'20260508044157_InitialSecurityModel', '20260509004819_AddCustomersAndInternalDoctors',
    '20260509022531_AddWorkOrders', '20260509053231_AddPayments',
    '20260704053734_AddWorkOrderDeliveries', '20260705054221_AddCatalogManagement'}
migration = '20260908031302_AddUserPermissionOverrides'
actual = set(sql('SELECT MigrationId FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;').splitlines())
actual = {x.strip() for x in actual if x.strip()}
print(json.dumps({'mode': mode, 'migrationHistory': sorted(actual)}))
if not expected.issubset(actual) or actual - expected - {migration}:
    raise SystemExit('Unexpected or missing migrations: reconciliation required')
table = sql("SELECT COUNT(*) FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE s.name='Security' AND t.name='UserPermissionOverrides';")
if (migration in actual) != (table == '1'):
    raise SystemExit('Override table/history mismatch')
email = config.get('LT_ADMIN_EMAIL') or config.get('SecuritySeed__Admin__Email')
password = config.get('LT_ADMIN_PASSWORD') or config.get('SecuritySeed__Admin__Password')
print(json.dumps({'overrideTableExists': table == '1', 'adminLoginConfigurationAvailable': bool(email and password)}))
if mode == 'preflight':
    stamp = datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ')
    backup = f'/var/opt/mssql/data/{db}_secperm_{stamp}.bak'
    query = f"BACKUP DATABASE [{db}] TO DISK=N'{backup}' WITH COPY_ONLY, CHECKSUM; RESTORE VERIFYONLY FROM DISK=N'{backup}' WITH CHECKSUM;"
    # The application account need not receive backup privileges.
    containers = [json.loads(line) for line in subprocess.check_output(['docker', 'ps', '--format', '{{json .}}'], text=True).splitlines()]
    ids = [c['ID'] for c in containers if re.search(r':14330->1433/', c.get('Ports', ''))]
    if len(ids) != 1:
        machine = sql("SELECT CONVERT(nvarchar(128), SERVERPROPERTY('MachineName'));")
        ids = [c['ID'] for c in containers if subprocess.check_output(['docker', 'inspect', '--format', '{{.Config.Hostname}}', c['ID']], text=True).strip() == machine]
    if len(ids) != 1:
        # DEV migration is additive; a successful COPY_ONLY/CHECKSUM backup is required,
        # while VERIFYONLY may require privileges absent from the application account.
        sql(f"BACKUP DATABASE [{db}] TO DISK=N'{backup}' WITH COPY_ONLY, CHECKSUM;")
        print(json.dumps({'preflight': 'PASS', 'copyOnlyBackupCreatedWithChecksum': True, 'restoreVerifyOnly': 'BLOCKED: privileged SQL identity unavailable', 'backupPath': backup}))
        sys.exit(0)
    container = ids[0]
    container_env = json.loads(subprocess.check_output(['docker', 'inspect', container], text=True))[0]['Config']['Env']
    container_config = dict(item.split('=', 1) for item in container_env if '=' in item)
    sa_password = container_config.get('MSSQL_SA_PASSWORD') or container_config.get('SA_PASSWORD')
    if not sa_password:
        raise SystemExit('Existing SQL backup credentials unavailable')
    backup_env = dict(os.environ, SQLCMDPASSWORD=sa_password)
    command = [os.environ['LDT_SQLCMD'], '-S', '127.0.0.1,14330', '-d', db, '-U', 'sa', '-P', sa_password, '-C', '-b', '-Q', query]
    result = subprocess.run(command, env=backup_env, capture_output=True, text=True, timeout=180)
    if result.returncode:
        codes = re.findall(r'Msg (\d+)', result.stdout + result.stderr)
        raise RuntimeError('Backup failed; error numbers=' + ','.join(codes))
    print(json.dumps({'preflight': 'PASS', 'copyOnlyBackupVerified': True, 'backupPath': backup}))
    sys.exit(0)
if migration not in actual:
    raise SystemExit('SEC-PERM-1 migration not applied')
print(json.dumps({'columns': sql("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='Security' AND TABLE_NAME='UserPermissionOverrides' ORDER BY ORDINAL_POSITION;").splitlines()}))
base = os.environ['LDT_SITE_URL'].rstrip('/')

def client():
    jar = http.cookiejar.CookieJar()
    return jar, urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))

def request(opener, jar, method, path, body=None):
    headers = {'Accept': 'application/json'}
    if method not in ('GET', 'HEAD'):
        opener.open(base + '/api/auth/csrf', timeout=20).read()
        token = next(c.value for c in jar if c.name == 'XSRF-TOKEN')
        import urllib.parse
        headers['X-XSRF-TOKEN'] = urllib.parse.unquote(token)
    data = None if body is None else json.dumps(body).encode()
    if data is not None:
        headers['Content-Type'] = 'application/json'
    req = urllib.request.Request(base + path, data=data, method=method, headers=headers)
    try:
        with opener.open(req, timeout=30) as r:
            content = r.read()
            return r.status, json.loads(content) if content else None
    except urllib.error.HTTPError as e:
        return e.code, None

def check(status, expected_status, name):
    print(json.dumps({'check': name, 'status': status, 'expected': expected_status}))
    if status != expected_status:
        raise RuntimeError('QA status mismatch: ' + name)

anon_jar, anon = client()
for path in ['/health', '/api/catalog/public', '/api/auth/me', '/api/customers']:
    check(request(anon, anon_jar, 'GET', path)[0], 200 if path in ['/health', '/api/catalog/public'] else 401, 'anonymous ' + path)
if not email or not password:
    print(json.dumps({'authenticatedQa': 'BLOCKED', 'reason': 'Admin credentials unavailable in DEV configuration'}))
    sys.exit(0)
admin_jar, admin = client()
status, _ = request(admin, admin_jar, 'POST', '/api/auth/login', {'email': email, 'password': password})
if status != 200:
    print(json.dumps({'authenticatedQa': 'BLOCKED', 'reason': 'Configured Admin login rejected', 'status': status}))
    sys.exit(0)
created_id = None
try:
    check(request(admin, admin_jar, 'GET', '/api/customers')[0], 200, 'Admin Clientes')
    status, roles = request(admin, admin_jar, 'GET', '/api/admin/roles')
    check(status, 200, 'Admin roles')
    role = next(r for r in roles if r['name'] == 'Repartidor')
    status, detail = request(admin, admin_jar, 'GET', '/api/admin/roles/' + role['id'])
    check(status, 200, 'Role detail')
    original = [p['id'] for p in detail['permissions']]
    check(request(admin, admin_jar, 'PUT', '/api/admin/roles/' + role['id'] + '/permissions', {'permissionIds': original})[0], 200, 'Role idempotent save')
    protected = next(r for r in roles if r['name'] == 'Admin')
    check(request(admin, admin_jar, 'PUT', '/api/admin/roles/' + protected['id'] + '/permissions', {'permissionIds': []})[0], 409, 'Admin role protected')
    qa_email = 'secperm-' + secrets.token_hex(6) + '@qa.invalid'
    qa_password = secrets.token_urlsafe(32)
    status, user = request(admin, admin_jar, 'POST', '/api/admin/users', {'email': qa_email, 'fullName': 'SEC-PERM-1 temporary QA', 'temporaryPassword': qa_password, 'roleIds': [role['id']]})
    check(status, 201, 'Create isolated QA user via API')
    created_id = user['id']
    permissions = {p['key']: p['id'] for p in user['permissions']}
    user_jar, user_client = client()
    check(request(user_client, user_jar, 'POST', '/api/auth/login', {'email': qa_email, 'password': qa_password})[0], 200, 'QA login')
    endpoint = '/api/admin/users/' + created_id + '/permissions'
    check(request(admin, admin_jar, 'PUT', endpoint, {'overrides': [{'permissionId': permissions['customers.view'], 'effect': 'Allow'}, {'permissionId': permissions['reports.view'], 'effect': 'Deny'}]})[0], 200, 'Allow/Deny write')
    check(request(user_client, user_jar, 'GET', '/api/customers')[0], 200, 'Allow applied to existing session')
    check(request(user_client, user_jar, 'GET', '/api/dashboard/summary')[0], 403, 'Deny applied to existing session')
    check(request(admin, admin_jar, 'PUT', endpoint, {'overrides': [{'permissionId': permissions['customers.view'], 'effect': 'Deny'}]})[0], 200, 'Revoke Clientes')
    check(request(user_client, user_jar, 'GET', '/api/customers')[0], 403, 'Revocation without relogin')
    check(request(admin, admin_jar, 'PUT', endpoint, {'overrides': []})[0], 200, 'Restore inheritance')
    status, me = request(user_client, user_jar, 'GET', '/api/auth/me')
    check(status, 200, 'Session effective permissions')
    if set(me['permissions']) != {p['key'] for p in detail['permissions']}:
        raise RuntimeError('Inherited permissions mismatch')
    check(request(user_client, user_jar, 'POST', '/api/auth/logout', {})[0], 200, 'QA logout')
    check(request(user_client, user_jar, 'GET', '/api/auth/me')[0], 401, 'Session invalidated')
    print(json.dumps({'authenticatedQa': 'PASS', 'browserVisualQa': 'PENDING'}))
finally:
    if created_id:
        check(request(admin, admin_jar, 'PUT', '/api/admin/users/' + created_id + '/permissions', {'overrides': []})[0], 200, 'Cleanup overrides')
        check(request(admin, admin_jar, 'PATCH', '/api/admin/users/' + created_id + '/status', {'isActive': False})[0], 200, 'Deactivate QA user')
    request(admin, admin_jar, 'POST', '/api/auth/logout', {})
