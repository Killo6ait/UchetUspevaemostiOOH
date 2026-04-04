using System;
using System.Linq;
using System.Windows;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Models;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ
{
    public partial class LoginWindow : Window
    {
        public User? CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                using (var context = new SchoolContext())
                {
                    var user = context.Users
                        .FirstOrDefault(u => u.Username == username);

                    if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash, user.Salt))
                    {
                        CurrentUser = user;
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        Logger.Warning($"Failed login attempt for user {username}", "Security");
                        MessageBox.Show("Неверный логин или пароль");
                        txtPassword.Password = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Login error for user {username}", ex, "Security");
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}");
            }
        }
        private void RegisterHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegisterWindow();
            reg.ShowDialog();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}