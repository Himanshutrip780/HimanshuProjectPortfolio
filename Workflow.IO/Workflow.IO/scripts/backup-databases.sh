#!/bin/bash
# SQL Server backup script - run daily via cron

BACKUP_DIR="/var/opt/mssql/backup"
RETENTION_DAYS=30
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Create backup directory if it doesn't exist
mkdir -p $BACKUP_DIR

# Backup all databases
docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IOUserDb] TO DISK = '$BACKUP_DIR/Workflow.IOUserDb_$TIMESTAMP.bak'" 2>/dev/null

docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IOProjectDb] TO DISK = '$BACKUP_DIR/Workflow.IOProjectDb_$TIMESTAMP.bak'" 2>/dev/null

docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IOTaskDb] TO DISK = '$BACKUP_DIR/Workflow.IOTaskDb_$TIMESTAMP.bak'" 2>/dev/null

docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IOCommentDb] TO DISK = '$BACKUP_DIR/Workflow.IOCommentDb_$TIMESTAMP.bak'" 2>/dev/null

docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IONotificationDb] TO DISK = '$BACKUP_DIR/Workflow.IONotificationDb_$TIMESTAMP.bak'" 2>/dev/null

docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IOActivityDb] TO DISK = '$BACKUP_DIR/Workflow.IOActivityDb_$TIMESTAMP.bak'" 2>/dev/null

docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IOFileDb] TO DISK = '$BACKUP_DIR/Workflow.IOFileDb_$TIMESTAMP.bak'" 2>/dev/null

docker exec workflow.io-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $MSSQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [Workflow.IOAnalyticsDb] TO DISK = '$BACKUP_DIR/Workflow.IOAnalyticsDb_$TIMESTAMP.bak'" 2>/dev/null

# Remove old backups (older than RETENTION_DAYS)
find $BACKUP_DIR -type f -name "*.bak" -mtime +$RETENTION_DAYS -delete

echo "[$(date)] Backup completed. Backups older than $RETENTION_DAYS days have been removed." >> /var/log/workflow.io-backup.log
