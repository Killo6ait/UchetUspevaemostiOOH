using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using Microsoft.Win32;

namespace УчётУспеваемостиООШ.Services
{

    public class BackupManager
    {
        private readonly string _backupFolder;
        private readonly string _connectionString;
        private readonly ConfigManager _config;

        public BackupManager()
        {
            _config = ConfigManager.Instance;

            _backupFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                _config.AppSettings.AutoBackup.BackupPath ?? "Backups");


            _connectionString = _config.GetConnectionString();

            if (!Directory.Exists(_backupFolder))
            {
                Directory.CreateDirectory(_backupFolder);
            }
        }

        public string CreateBackup(Action<int>? progressCallback = null)
        {
            try
            {
                string fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string backupPath = Path.Combine(_backupFolder, fileName);
                string backupCommand = $@"
            BACKUP DATABASE [{_config.Database.DatabaseName}] 
            TO DISK = N'{backupPath}' 
            WITH FORMAT, STATS = 10";

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(backupCommand, connection))
                    {
                        command.ExecuteNonQuery();
                        progressCallback?.Invoke(100);
                    }
                }

                string checksum = ComputeChecksum(backupPath);
                File.WriteAllText(backupPath + ".md5", checksum);

                Logger.Info($"Backup created: {fileName}", "Backup");
                return fileName;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating backup: {ex.Message}", ex, "Backup");
                throw;
            }
        }

        public void RestoreBackup(string backupFilePath, Action<int>? progressCallback = null)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                    throw new FileNotFoundException($"Файл бэкапа не найден: {backupFilePath}");

                if (!VerifyBackupIntegrity(backupFilePath))
                    throw new Exception("Бэкап повреждён!");

                string databaseName = _config.Database.DatabaseName;
                string restoreCommand = $@"
            USE master; 
            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;  
            RESTORE DATABASE [{databaseName}] FROM DISK = N'{backupFilePath}' WITH REPLACE;
            ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(restoreCommand, connection))
                    {
                        command.ExecuteNonQuery();
                        progressCallback?.Invoke(100);
                    }
                }

                Logger.Info($"Backup restored: {Path.GetFileName(backupFilePath)}", "Backup");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error restoring backup: {ex.Message}", ex, "Backup");
                throw;
            }
        }

        public List<BackupInfo> ListBackups()
        {
            var backups = new List<BackupInfo>();

            if (!Directory.Exists(_backupFolder))
                return backups;

            var files = Directory.GetFiles(_backupFolder, "backup_*.bak");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                string fileName = Path.GetFileName(file);
                DateTime backupDate = DateTime.MinValue;
                string datePart = fileName.Replace("backup_", "").Replace(".bak", "");
                if (DateTime.TryParseExact(datePart, "yyyyMMdd_HHmmss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    backupDate = parsedDate;
                }
                bool isValid = VerifyBackupIntegrity(file);

                backups.Add(new BackupInfo
                {
                    FileName = fileName,
                    FullPath = file,
                    CreatedDate = backupDate,
                    SizeBytes = fileInfo.Length,
                    SizeFormatted = FormatFileSize(fileInfo.Length),
                    IsValid = isValid
                });
            }

            return backups.OrderByDescending(b => b.CreatedDate).ToList();
        }

        public int CleanupOldBackups(int days)
        {
            if (days <= 0)
                return 0;

            var cutoffDate = DateTime.Now.AddDays(-days);
            var backups = ListBackups();
            int deletedCount = 0;

            foreach (var backup in backups)
            {
                if (backup.CreatedDate < cutoffDate)
                {
                    try
                    {
                        File.Delete(backup.FullPath);
                        string checksumFile = backup.FullPath + ".md5";
                        if (File.Exists(checksumFile))
                            File.Delete(checksumFile);

                        deletedCount++;
                        Logger.Info($"Удалён старый бэкап: {backup.FileName}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Ошибка при удалении бэкапа {backup.FileName}: {ex.Message}", ex);
                    }
                }
            }

            return deletedCount;
        }

        private string ComputeChecksum(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private bool VerifyBackupIntegrity(string backupFilePath)
        {
            try
            {
                string checksumFile = backupFilePath + ".md5";

                if (!File.Exists(checksumFile))
                {
                    Logger.Warning($"Файл контрольной суммы не найден для {Path.GetFileName(backupFilePath)}");
                    return true;
                }

                string savedChecksum = File.ReadAllText(checksumFile).Trim();
                string currentChecksum = ComputeChecksum(backupFilePath);

                return string.Equals(savedChecksum, currentChecksum, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка при проверке целостности бэкапа: {ex.Message}", ex);
                return false;
            }
        }
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public string? ShowRestoreDialog()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Резервные копии (*.bak)|*.bak|Все файлы (*.*)|*.*",
                Title = "Выберите файл резервной копии для восстановления",
                InitialDirectory = _backupFolder
            };

            if (openDialog.ShowDialog() == true)
            {
                return openDialog.FileName;
            }

            return null;
        }

        public string? ShowSaveDialog(string defaultName)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Резервные копии (*.bak)|*.bak",
                Title = "Сохранить резервную копию",
                FileName = defaultName,
                InitialDirectory = _backupFolder
            };

            if (saveDialog.ShowDialog() == true)
            {
                return saveDialog.FileName;
            }

            return null;
        }
    }

    public class BackupInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public long SizeBytes { get; set; }
        public string SizeFormatted { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
    }
}