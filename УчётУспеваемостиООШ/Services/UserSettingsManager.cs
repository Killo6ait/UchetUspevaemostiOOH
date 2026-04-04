using System;
using System.IO;
using Newtonsoft.Json;
using УчётУспеваемостиООШ.Models;

namespace УчётУспеваемостиООШ.Services
{

    public class UserSettingsManager
    {
        private readonly string _settingsFolder;

        public UserSettingsManager()
        {
            _settingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserSettings");
            if (!Directory.Exists(_settingsFolder))
                Directory.CreateDirectory(_settingsFolder);
        }

        private string GetSettingsPath(User user)
        {
            return Path.Combine(_settingsFolder, $"user_{user.UserID}.json");
        }

        public UserSettings GetSettings(User user)
        {
            if (user == null) return new UserSettings();
            
            string filePath = GetSettingsPath(user);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    return UserSettings.FromJson(json);
                }
                catch
                {
                    return new UserSettings();
                }
            }
            
            return new UserSettings();
        }

        public void SaveSettings(User user, UserSettings settings)
        {
            if (user == null) return;
            
            string filePath = GetSettingsPath(user);
            string json = settings.ToJson();
            File.WriteAllText(filePath, json);
            
            Logger.Info($"Settings saved for user {user.Username}", "Settings");
        }
    }
}