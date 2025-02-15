-- Database name which is set from the backup-db.sh script.
DECLARE @DatabaseName varchar(100)
SET @DatabaseName = '$(DATABASE_NAME)'

-- Database name without spaces for saving the backup files.
DECLARE @DatabaseNameSafe varchar(100)
SET @DatabaseNameSafe = '$(DATABASE_NAME)'

DECLARE @BackupFile varchar(100)
SET @BackupFile = '/etc/bitwarden/mssql/backups/' + @DatabaseNameSafe + '_FULL_$(now).BAK'

DECLARE @BackupName varchar(100)
SET @BackupName = @DatabaseName + ' full backup for $(now)'

DECLARE @BackupCommand NVARCHAR(1000)
SET @BackupCommand = 'BACKUP DATABASE [' + @DatabaseName + '] TO DISK = ''' + @BackupFile + ''' WITH INIT, NAME= ''' + @BackupName + ''', NOSKIP, NOFORMAT'

EXEC(@BackupCommand)

-- Shrink the transaction log after the backup
DBCC SHRINKFILE ('$(DATABASE_NAME)_log', EMPTYFILE);
