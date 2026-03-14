using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using УчётУспеваемостиООШ.Models;

namespace УчётУспеваемостиООШ
{
    public partial class Menu : Window
    {
        private User _currentUser;

        public Menu(User user)
        {
            InitializeComponent();
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));

            DisplayUserInfo();
            ApplyRoleBasedAccess();
            InitializeResources();
        }

        private void InitializeResources()
        {
            // Создаем эффект тени для кнопок
            var buttonShadow = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = 5,
                Opacity = 0.3,
                BlurRadius = 10
            };

            // Добавляем ресурс в словарь ресурсов окна
            this.Resources["ButtonShadow"] = buttonShadow;
        }

        private void DisplayUserInfo()
        {
            txtUserInfo.Text = $"{_currentUser.Username} ({_currentUser.Role}) ";
            txtStatus.Text = $"Выберите раздел для работы.";
        }

        private void ApplyRoleBasedAccess()
        {
            // Настройка доступности кнопок в зависимости от роли
            switch (_currentUser.Role)
            {
                case "Директор":
                    // Полный доступ ко всем разделам
                    break;

                case "Завуч":
                    // Завуч имеет доступ ко всем разделам
                    break;

                case "Учитель":
                    // Учитель видит все кнопки, но с ограниченным функционалом внутри форм
                    // Все кнопки остаются доступными, ограничения будут внутри форм
                    //btnStudents.ToolTip = "Просмотр учащихся (только чтение)";
                    //btnSubjects.ToolTip = "Просмотр предметов (только чтение)";
                    //btnGrades.ToolTip = "Просмотр и управление оценками (только свои предметы)";
                    //btnAttendance.ToolTip = "Просмотр и управление посещаемостью (только свой класс)";
                    break;
            }
        }

        private void btnStudents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                AnimateButton(button);
            }

            // Передаем текущего пользователя в окно StudentsWindow
            StudentsWindow studentsWindow = new StudentsWindow(_currentUser);
            studentsWindow.Owner = this;
            studentsWindow.Show();
        }

        private void btnSubjects_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                AnimateButton(button);
            }

            // Передаем текущего пользователя в окно SubjectsWindow
            SubjectsWindow subjectsWindow = new SubjectsWindow(_currentUser);
            subjectsWindow.Owner = this;
            subjectsWindow.Show();
        }

        private void btnGrades_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                AnimateButton(button);
            }

            // Передаем текущего пользователя в окно GradesWindow
            GradesWindow grades = new GradesWindow(_currentUser);
            grades.Owner = this;
            grades.Show();
        }

        private void btnAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                AnimateButton(button);
            }

            // Передаем текущего пользователя в окно AttendanceWindow
            AttendanceWindow attendanceWindow = new AttendanceWindow();
            attendanceWindow.Owner = this;
            attendanceWindow.Show();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?",
                                         "Подтверждение выхода",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Открываем окно авторизации
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                // Закрываем текущее окно меню
                this.Close();
            }
        }

        private void AnimateButton(Button button)
        {
            if (button == null) return;

            var scaleTransform = new ScaleTransform(1, 1);
            button.RenderTransform = scaleTransform;
            button.RenderTransformOrigin = new Point(0.5, 0.5);

            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0.95,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Завершаем приложение при закрытии главного окна
            Application.Current.Shutdown();
        }
    }
}