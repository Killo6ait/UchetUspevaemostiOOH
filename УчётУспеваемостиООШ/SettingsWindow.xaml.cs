using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using УчётУспеваемостиООШ.Models;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ
{
    public partial class SettingsWindow : Window
    {
        private UserSettingsManager _settingsManager;
        private BackupManager _backupManager;
        private User _currentUser;
        private Models.UserSettings _currentSettings;
        private Window _owner;

        public SettingsWindow(Window owner, User currentUser)
        {
            InitializeComponent();
            Owner = owner;
            _currentUser = currentUser;
            _settingsManager = new UserSettingsManager();
            _backupManager = new BackupManager();
            _currentSettings = _settingsManager.GetSettings(_currentUser);

            LoadSettings();
            ApplyLightTheme();  // Окно настроек всегда светлое
        }

        private void LoadSettings()
        {
            // Просто устанавливаем выбранные значения в ComboBox
            cmbTheme.SelectedItem = _currentSettings.Theme == "Dark" ? cmbTheme.Items[1] : cmbTheme.Items[0];

            switch (_currentSettings.FontSizeScale)
            {
                case 75: cmbScale.SelectedItem = cmbScale.Items[0]; break;
                case 100: cmbScale.SelectedItem = cmbScale.Items[1]; break;
                case 125: cmbScale.SelectedItem = cmbScale.Items[2]; break;
                case 150: cmbScale.SelectedItem = cmbScale.Items[3]; break;
                default: cmbScale.SelectedItem = cmbScale.Items[1]; break;
            }

            foreach (ComboBoxItem item in cmbFont.Items)
            {
                if (item.Tag?.ToString() == _currentSettings.FontFamily)
                {
                    cmbFont.SelectedItem = item;
                    break;
                }
            }

            //if (_currentUser.Role == "Администратор")
            //{
            //    chkAutoBackup.IsChecked = _currentSettings.AutoBackupEnabled;
            //    txtBackupInterval.Text = _currentSettings.BackupIntervalDays.ToString();
            //    txtBackupInterval.IsEnabled = chkAutoBackup.IsChecked == true;
            //}
            //else
            //{
            //    chkAutoBackup.Visibility = Visibility.Collapsed;
            //    txtBackupInterval.Visibility = Visibility.Collapsed;
            //}
        }

        // ========== ОБРАБОТЧИКИ (без применения к окну настроек) ==========

        /// <summary>
        /// Обработчик изменения темы (только сохраняем выбранное значение, НЕ применяем)
        /// </summary>
        private void cmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ничего не делаем, только запоминаем для сохранения
            // НЕ меняем внешний вид окна настроек
        }

        /// <summary>
        /// Обработчик изменения масштаба (только предпросмотр, НЕ применяем к окну)
        /// </summary>
        private void cmbScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ничего не делаем – масштаб не должен меняться в окне настроек
            // if (cmbScale.SelectedItem is ComboBoxItem item && item.Tag != null)
            // {
            //     if (int.TryParse(item.Tag.ToString(), out int scale))
            //     {
            //         // НЕ вызываем ApplyScale
            //     }
            // }
        }

        /// <summary>
        /// Обработчик изменения шрифта (только предпросмотр, НЕ применяем к окну)
        /// </summary>
        private void cmbFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ничего не делаем – шрифт не должен меняться в окне настроек
            // if (cmbFont.SelectedItem is ComboBoxItem item && item.Tag != null)
            // {
            //     // НЕ вызываем ApplyFont
            // }
        }

        /// <summary>
        /// Обработчик изменения чекбокса авто-бэкапа
        /// </summary>
        private void chkAutoBackup_Changed(object sender, RoutedEventArgs e)
        {
            txtBackupInterval.IsEnabled = chkAutoBackup.IsChecked == true;
        }

        /// <summary>
        /// Обработчик изменения интервала бэкапа
        /// </summary>
        private void txtBackupInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtBackupInterval.Text))
            {
                if (!int.TryParse(txtBackupInterval.Text, out int value) || value < 1 || value > 30)
                {
                    txtBackupInterval.Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));
                }
                else
                {
                    txtBackupInterval.Background = SystemColors.WindowBrush;
                }
            }
        }

        // ========== ОСТАЛЬНЫЕ МЕТОДЫ ==========

        /// <summary>
        /// Применение светлой темы к окну настроек (всегда светлая)
        /// </summary>
        private void ApplyLightTheme()
        {
            // Фон окна
            this.Background = new SolidColorBrush(Color.FromRgb(245, 246, 250));

            // Цвета для TextBlock
            foreach (var child in FindVisualChildren<TextBlock>(this))
            {
                child.Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94));
            }

            // Цвета для ComboBox
            cmbTheme.Background = new SolidColorBrush(Colors.White);
            cmbTheme.Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94));
            cmbTheme.BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199));

            cmbScale.Background = new SolidColorBrush(Colors.White);
            cmbScale.Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94));
            cmbScale.BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199));

            cmbFont.Background = new SolidColorBrush(Colors.White);
            cmbFont.Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94));
            cmbFont.BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199));

            // Разделитель
            var separator = FindName("Separator") as Separator;
            if (separator != null)
            {
                separator.Background = new SolidColorBrush(Color.FromRgb(236, 240, 241));
            }

            // TextBox
            txtBackupInterval.Background = new SolidColorBrush(Colors.White);
            txtBackupInterval.Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94));
            txtBackupInterval.BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199));

            this.Title = "Настройки программы";
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) yield return typedChild;
                foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
            }
        }

        private async void btnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnCreateBackup.IsEnabled = false;
                btnRestoreBackup.IsEnabled = false;

                var progressWindow = new ProgressWindow("Создание резервной копии...");
                progressWindow.Show();

                await Task.Run(() =>
                {
                    _backupManager.CreateBackup(percent =>
                    {
                        Dispatcher.Invoke(() => progressWindow.UpdateProgress(percent));
                    });
                });

                progressWindow.Close();

                MessageBox.Show("Резервная копия успешно создана!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCreateBackup.IsEnabled = true;
                btnRestoreBackup.IsEnabled = true;
            }
        }

        private async void btnRestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? backupFile = _backupManager.ShowRestoreDialog();
                if (string.IsNullOrEmpty(backupFile)) return;

                var result = MessageBox.Show(
                    "ВНИМАНИЕ! Восстановление из бэкапа перезапишет текущую базу данных.\n\nПродолжить?",
                    "Подтверждение восстановления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                btnCreateBackup.IsEnabled = false;
                btnRestoreBackup.IsEnabled = false;

                var progressWindow = new ProgressWindow("Восстановление из резервной копии...");
                progressWindow.Show();

                await Task.Run(() =>
                {
                    _backupManager.RestoreBackup(backupFile, percent =>
                    {
                        Dispatcher.Invoke(() => progressWindow.UpdateProgress(percent));
                    });
                });

                progressWindow.Close();

                MessageBox.Show("База данных восстановлена! Рекомендуется перезапустить программу.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCreateBackup.IsEnabled = true;
                btnRestoreBackup.IsEnabled = true;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string oldTheme = _currentSettings.Theme;
                string newTheme = ((ComboBoxItem)cmbTheme.SelectedItem)?.Tag?.ToString() ?? "Light";
                int newScale = int.Parse(((ComboBoxItem)cmbScale.SelectedItem)?.Tag?.ToString() ?? "100");
                string newFont = ((ComboBoxItem)cmbFont.SelectedItem)?.Tag?.ToString() ?? "Segoe UI";

                _currentSettings.Theme = newTheme;
                _currentSettings.FontSizeScale = newScale;
                _currentSettings.FontFamily = newFont;

                if (_currentUser.Role == "Администратор")
                {
                    _currentSettings.AutoBackupEnabled = chkAutoBackup.IsChecked == true;
                    if (int.TryParse(txtBackupInterval.Text, out int interval) && interval >= 1 && interval <= 30)
                    {
                        _currentSettings.BackupIntervalDays = interval;
                    }
                }

                _settingsManager.SaveSettings(_currentUser, _currentSettings);

                if (oldTheme != newTheme)
                {
                    Logger.Info($"User {_currentUser.Username} changed theme from {oldTheme} to {newTheme}", "Settings");
                }

                BaseWindow.NotifySettingsChanged();

                MessageBox.Show("Настройки сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving settings: {ex.Message}", ex, "UI");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}