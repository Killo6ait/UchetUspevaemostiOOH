using System;
using System.Linq;
using System.Windows;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Models;

namespace УчётУспеваемостиООШ
{
    public partial class LoginWindow : Window
    {
        public User CurrentUser { get; private set; }

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
                    // Ищем пользователя в БД
                    var user = context.Users
                        .FirstOrDefault(u => u.Username == username && u.Password == password);

                    if (user != null)
                    {
                        CurrentUser = user;
                        Menu menu = new Menu(CurrentUser);
                        menu.Show();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль");
                        txtPassword.Password = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }

        private void ShowError(string message)
        {
            txtErrorMessage.Text = message;
            txtErrorMessage.Visibility = Visibility.Visible;
        }
    }
}