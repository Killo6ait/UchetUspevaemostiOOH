using Newtonsoft.Json;

namespace УчётУспеваемостиООШ.Models
{
    public class UserSettings
    {
        public string Theme { get; set; } = "Light";
        public int FontSizeScale { get; set; } = 100;
        public string FontFamily { get; set; } = "Segoe UI";
        public bool AutoBackupEnabled { get; set; } = true;
        public int BackupIntervalDays { get; set; } = 7;

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static UserSettings FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new UserSettings();
            try
            {
                return JsonConvert.DeserializeObject<UserSettings>(json) ?? new UserSettings();
            }
            catch
            {
                return new UserSettings();
            }
        }
    }
}