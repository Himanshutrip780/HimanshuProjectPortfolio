# Healthcheck and Backup Configuration for Workflow.IO

## 1. Healthchecks (Docker Compose)

Updated `docker-compose.yml` with healthchecks for all services:

**SQL Server**: Uses sqlcmd to verify database connectivity
- Interval: 10s, Timeout: 5s, Retries: 5, Start period: 40s

**RabbitMQ**: Uses rabbitmq-diagnostics to verify broker health
- Interval: 10s, Timeout: 5s, Retries: 5, Start period: 30s

**All APIs**: Use curl to check /health endpoint
- Interval: 15s, Timeout: 5s, Retries: 3, Start period: 30s

**Dependency Management**: Services now wait for dependencies to be healthy before starting, preventing cascade failures.

Check health status:
```bash
docker compose ps
```

## 2. GitHub Actions CI/CD Pipeline

Created `.github/workflows/deploy.yml` with:

**Build Stage**:
- Builds all Docker images
- Tests deployment locally
- Verifies service health

**Deploy Stage** (main branch only):
- Pulls latest code from git
- Updates images
- Restarts services on production

**Setup Instructions**:
1. Add these GitHub Secrets:
   - `DEPLOY_KEY`: SSH private key for production server
   - `DEPLOY_HOST`: Production server IP/hostname
   - `DEPLOY_USER`: SSH username on production

2. Trigger on every push to `main` branch

Test locally:
```bash
docker compose up -d
docker compose ps  # All services should show "healthy"
```

## 3. SQL Server Automated Backups

Created `scripts/backup-databases.sh` for daily backups:

**Features**:
- Backs up all 8 databases
- Stores backups in `./backups` volume
- Auto-deletes backups older than 30 days
- Logs backup status

**Setup on Linux/macOS**:
```bash
chmod +x scripts/backup-databases.sh

# Add to crontab for daily 2 AM backups
crontab -e
# Add: 0 2 * * * cd /path/to/Workflow.IO && scripts/backup-databases.sh
```

**Manual backup**:
```bash
./scripts/backup-databases.sh
```

**Restore a database**:
```bash
docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P $MSSQL_SA_PASSWORD \
  -Q "RESTORE DATABASE [Workflow.IOUserDb] FROM DISK = '/var/opt/mssql/backup/Workflow.IOUserDb_TIMESTAMP.bak'"
```

**Verify backups exist**:
```bash
docker exec workflow.io-sqlserver ls -la /var/opt/mssql/backup/
```

## 4. Monitor Service Health

**Check all service statuses**:
```bash
docker compose ps
```

**View healthcheck logs**:
```bash
docker compose logs --follow gatewayapi
```

**Restart unhealthy service**:
```bash
docker compose restart userapi
```
