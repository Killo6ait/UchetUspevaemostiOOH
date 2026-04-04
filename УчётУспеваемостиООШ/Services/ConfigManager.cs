using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace УчётУспеваемостиООШ.Services
{
    /// <summary>
    /// Менеджер конфигурации приложения (Singleton)
    /// Обеспечивает единую точку доступа к настройкам
    /// </summary>
    public class ConfigManager
    {
        private static ConfigManager? _instance;
        private static readonly object _lock = new object();
        private ConfigModel _config;
        private readonly string _configPath;

        /// <summary>
        /// Приватный конструктор (Singleton)
        /// </summary>
        private ConfigManager()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            LoadConfig();
        }

        /// <summary>
        /// Получение экземпляра ConfigManager (Singleton)
        /// </summary>
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Загрузка конфигурации из файла
        /// Вызывается при запуске программы
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<ConfigModel>(json) ?? GetDefaultConfig();
                    ValidateConfig();
                }
                else
                {
                    _config = GetDefaultConfig();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки конфигурации: {ex.Message}\nИспользуются настройки по умолчанию.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                _config = GetDefaultConfig();
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения конфигурации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool ValidateConfig()
        {
            bool isValid = true;
            var validScales = new[] { 75, 100, 125, 150 };
            if (!validScales.Contains(_config.AppSettings.FontSizeScale))
            {
                _config.AppSettings.FontSizeScale = 100; 
                isValid = false;
            }
            if (_config.AppSettings.AutoBackup.Enabled && _config.AppSettings.AutoBackup.IntervalDays <= 0)
            {
                _config.AppSettings.AutoBackup.IntervalDays = 7;
                isValid = false;
            }
            if (_config.AppSettings.AutoBackup.MaxBackupCount <= 0)
            {
                _config.AppSettings.AutoBackup.MaxBackupCount = 10;
                isValid = false;
            }
            if (!isValid) SaveConfig();
            return isValid;
        }

        /// <summary>
        /// Получение конфигурации по умолчанию
        /// </summary>
        private ConfigModel GetDefaultConfig()
        {
            return new ConfigModel
            {
                Database = new DatabaseConfig
                {
                    Server = "DESKTOP-MQ9HMGB",
                    DatabaseName = "УчетУспеваемостиООШ",
                    IntegratedSecurity = true,
                    TrustServerCertificate = true
                },
                AppSettings = new AppSettingsConfig
                {
                    Theme = "Light",
                    FontSizeScale = 100,
                    FontFamily = "Segoe UI",
                    AutoBackup = new AutoBackupConfig
                    {
                        Enabled = true,
                        IntervalDays = 7,
                        BackupPath = "Backups",
                        MaxBackupCount = 10
                    }
                }
            };
        }

        // Свойства для доступа к настройкам
        public DatabaseConfig Database => _config.Database;
        public AppSettingsConfig AppSettings => _config.AppSettings;

        /// <summary>
        /// Получение строки подключения к базе данных
        /// </summary>
        public string GetConnectionString()
        {
            if (_config.Database.IntegratedSecurity)
            {
                return $"Server={_config.Database.Server};Database={_config.Database.DatabaseName};Trusted_Connection=True;TrustServerCertificate={_config.Database.TrustServerCertificate};";
            }
            else
            {
                return $"Server={_config.Database.Server};Database={_config.Database.DatabaseName};User Id={_config.Database.UserId};Password={_config.Database.Password};TrustServerCertificate={_config.Database.TrustServerCertificate};";
            }
        }
    }

    // Модели конфигурации (структура данных)
    public class ConfigModel
    {
        public DatabaseConfig Database { get; set; } = new DatabaseConfig();
        public AppSettingsConfig AppSettings { get; set; } = new AppSettingsConfig();
    }

    public class DatabaseConfig
    {
        public string Server { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public bool IntegratedSecurity { get; set; } = true;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool TrustServerCertificate { get; set; } = true;
    }

    public class AppSettingsConfig
    {
        public string Theme { get; set; } = "Light";
        public int FontSizeScale { get; set; } = 100;
        public string FontFamily { get; set; } = "Segoe UI";
        public AutoBackupConfig AutoBackup { get; set; } = new AutoBackupConfig();
        public LoggingConfig Logging { get; set; } = new LoggingConfig();
        public ExportConfig Export { get; set; } = new ExportConfig();
    }

    public class AutoBackupConfig
    {
        public bool Enabled { get; set; } = true;
        public int IntervalDays { get; set; } = 7;
        public string BackupPath { get; set; } = "Backups";
        public int MaxBackupCount { get; set; } = 10;
    }

    public class LoggingConfig
    {
        public bool Enabled { get; set; } = true;
        public string LogPath { get; set; } = "Logs";
        public string LogLevel { get; set; } = "Info";
    }

    public class ExportConfig
    {
        public string DefaultPath { get; set; } = "Exports";
        public string FileNamePattern { get; set; } = "Ведомость_{className}_{date}.xlsx";
    }
}