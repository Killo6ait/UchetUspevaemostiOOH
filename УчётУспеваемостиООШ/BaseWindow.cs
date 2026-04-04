using System;
using System.Windows;
using System.Windows.Media;
using УчётУспеваемостиООШ.Models;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ
{
    public abstract class BaseWindow : Window
    {
        protected User _currentUser;
        protected UserSettingsManager _settingsManager;

        // Конструктор по умолчанию
        public BaseWindow()
        {
            _settingsManager = new UserSettingsManager();
            SettingsApplied += OnSettingsApplied;
        }

        // Конструктор с пользователем
        public BaseWindow(User user) : this()
        {
            _currentUser = user;
            ApplyCurrentSettings();
        }

        public static event EventHandler? SettingsApplied;

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            ApplyCurrentSettings();
        }

        protected virtual void ApplyCurrentSettings()
        {
            if (_currentUser != null)
            {
                var settings = _settingsManager.GetSettings(_currentUser);
                ApplyTheme(settings.Theme);
                ApplyScale(settings.FontSizeScale);
                ApplyFont(settings.FontFamily);
            }
        }

        protected virtual void ApplyTheme(string theme)
        {
            var res = Application.Current.Resources;

            if (theme == "Dark")
            {
                res["ControlBackground"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                res["TextColor"] = new SolidColorBrush(Colors.White);
                res["BorderColor"] = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                res["DataGridHeaderColor"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                res["HeaderTextColor"] = new SolidColorBrush(Colors.White);
                res["WindowBackground"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            }
            else
            {
                res["ControlBackground"] = new SolidColorBrush(Colors.White);
                res["TextColor"] = new SolidColorBrush(Color.FromRgb(52, 73, 94));
                res["BorderColor"] = new SolidColorBrush(Color.FromRgb(189, 195, 199));
                res["DataGridHeaderColor"] = new SolidColorBrush(Color.FromRgb(236, 240, 241));
                res["HeaderTextColor"] = new SolidColorBrush(Color.FromRgb(52, 73, 94));
                res["WindowBackground"] = new SolidColorBrush(Color.FromRgb(245, 246, 250));
            }

            this.Background = (Brush)res["WindowBackground"];
        }

        protected virtual void ApplyScale(int scalePercent)
        {
            double scale = scalePercent / 100.0;
            if (this.Content is FrameworkElement content)
            {
                content.LayoutTransform = new ScaleTransform(scale, scale);
            }
        }

        protected virtual void ApplyFont(string fontFamily)
        {
            this.FontFamily = new FontFamily(fontFamily);
        }

        protected virtual void OnSettingsApplied(object? sender, EventArgs e)
        {
            ApplyCurrentSettings();
        }

        public static void NotifySettingsChanged() => SettingsApplied?.Invoke(null, EventArgs.Empty);
    }
}