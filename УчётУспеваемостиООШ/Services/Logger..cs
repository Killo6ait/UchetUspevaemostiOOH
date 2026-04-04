using System;
using System.IO;
using System.Linq;

namespace УчётУспеваемостиООШ.Services
{
    /// <summary>
    /// Система логирования с поддержкой ротации файлов
    /// Формат: [YYYY-MM-DD HH:MM:SS] [LEVEL] [Category] Message
    /// </summary>
    public static class Logger
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;
        private static readonly object _lock = new object();
        private static readonly int MaxFileSize = 10 * 1024 * 1024; // 10 МБ
        private static readonly int MaxBackupFiles = 5;

        static Logger()
        {
            LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            LogFilePath = Path.Combine(LogDirectory, "app.log");
        }

        /// <summary>
        /// Информационное сообщение
        /// </summary>
        public static void Info(string message, string category = "App")
        {
            WriteLog("INFO", category, message);
        }

        /// <summary>
        /// Предупреждение
        /// </summary>
        public static void Warning(string message, string category = "App")
        {
            WriteLog("WARNING", category, message);
        }

        /// <summary>
        /// Ошибка с деталями исключения
        /// </summary>
        public static void Error(string message, Exception? ex = null, string category = "App")
        {
            string errorMsg = ex != null ? $"{message} | {ex.Message} | {ex.StackTrace}" : message;
            WriteLog("ERROR", category, errorMsg);
        }

        /// <summary>
        /// Аудит действий пользователя
        /// Формат: [AUDIT] [Audit] [User: admin] Added record to Students (ID=15)
        /// </summary>
        public static void Audit(string user, string action, string details, string category = "Audit")
        {
            WriteLog("AUDIT", category, $"[User: {user}] {action}: {details}");
        }

        /// <summary>
        /// Метрики производительности
        /// </summary>
        public static void Metrics(string metric, string value, string category = "Metrics")
        {
            WriteLog("INFO", category, $"[Metrics] {metric}: {value}");
        }

        /// <summary>
        /// Запись в лог с ротацией
        /// </summary>
        private static void WriteLog(string level, string category, string message)
        {
            try
            {
                lock (_lock)
                {
                    CheckAndRotateLog();
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] [{category}] {message}";
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }
            }
            catch { }
        }

        /// <summary>
        /// Ротация логов при превышении 10 МБ
        /// </summary>
        private static void CheckAndRotateLog()
        {
            if (!File.Exists(LogFilePath))
                return;

            var fileInfo = new FileInfo(LogFilePath);
            if (fileInfo.Length >= MaxFileSize)
            {
                var backupFiles = Directory.GetFiles(LogDirectory, "app.log.*")
                    .OrderByDescending(f => f)
                    .ToList();

                while (backupFiles.Count >= MaxBackupFiles)
                {
                    File.Delete(backupFiles.Last());
                    backupFiles.RemoveAt(backupFiles.Count - 1);
                }

                for (int i = MaxBackupFiles - 1; i > 0; i--)
                {
                    string oldFile = Path.Combine(LogDirectory, $"app.log.{i}");
                    string newFile = Path.Combine(LogDirectory, $"app.log.{i + 1}");
                    if (File.Exists(oldFile))
                        File.Move(oldFile, newFile);
                }

                File.Move(LogFilePath, Path.Combine(LogDirectory, "app.log.1"));
            }
        }
    }
}