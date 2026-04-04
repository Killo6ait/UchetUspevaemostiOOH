using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Models;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            txtError.Visibility = Visibility.Collapsed;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Введите логин"); return false;
            }
            if (txtUsername.Text.Trim().Length < 3)
            {
                ShowError("Логин должен быть не менее 3 символов"); return false;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                ShowError("Введите пароль"); return false;
            }
            if (txtPassword.Password.Length < 6)
            {
                ShowError("Пароль должен быть не менее 6 символов"); return false;
            }
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Пароли не совпадают"); return false;
            }
            return true;
        }

        private void ShowError(string msg)
        {
            txtError.Text = msg;
            txtError.Visibility = Visibility.Visible;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;

            if (!Validate()) return;

            try
            {
                using (var context = new SchoolContext())
                {
                    string username = txtUsername.Text.Trim();

                    if (context.Users.Any(u => u.Username == username))
                    {
                        ShowError("Пользователь с таким логином уже существует");
                        return;
                    }

                    string role = ((ComboBoxItem)cmbRole.SelectedItem).Tag.ToString();

                    // Хеширование пароля
                    string hash = PasswordHasher.HashPassword(txtPassword.Password, out string salt);

                    var newUser = new User
                    {
                        Username = username,
                        PasswordHash = hash,
                        Salt = salt,
                        Role = role,
                       
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    Logger.Info($"Новый пользователь зарегистрирован: {username} ({role})", "Security");

                    MessageBox.Show($"✅ Регистрация прошла успешно!\n\nЛогин: {username}\nРоль: {role}\n\nТеперь можете войти в систему.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при регистрации пользователя", ex);

                // ←←← ПОКАЗЫВАЕМ ПОЛНУЮ ИНФОРМАЦИЮ ОБ ОШИБКЕ
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                    errorMsg += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";

                ShowError($"Ошибка регистрации:\n{errorMsg}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}