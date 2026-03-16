using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Models;

namespace УчётУспеваемостиООШ
{
    public partial class StudentsWindow : Window
    {
        private SchoolContext _context = new SchoolContext();
        private List<StudentViewModel> _students = new List<StudentViewModel>();
        private User _currentUser;

        public StudentsWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadData();
            ApplyPermissions();
        }

        // Класс для отображения в DataGrid
        public class StudentViewModel
        {
            public int StudentID { get; set; }
            public string LastName { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string? MiddleName { get; set; }
            public int ClassID { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public DateTime BirthDate { get; set; }
            public string ParentAddress { get; set; } = string.Empty;
        }

        // Класс для отображения классов в ComboBox
        public class ClassViewModel
        {
            public int ClassID { get; set; }
            public string ClassName { get; set; } = string.Empty;
        }
        private void ApplyPermissions()
        {
            // Если пользователь - учитель, отключаем все кнопки редактирования
            if (_currentUser.Role == "Учитель")
            {
                btnAdd.IsEnabled = false;
                btnAdd.Opacity = 0.5;
                btnAdd.ToolTip = "Добавление запрещено для вашей роли";

                btnUpdate.IsEnabled = false;
                btnUpdate.Opacity = 0.5;
                btnUpdate.ToolTip = "Редактирование запрещено для вашей роли";

                btnDelete.IsEnabled = false;
                btnDelete.Opacity = 0.5;
                btnDelete.ToolTip = "Удаление запрещено для вашей роли";

                // Поля ввода делаем доступными только для чтения
                txtLastName.IsReadOnly = true;
                txtFirstName.IsReadOnly = true;
                txtMiddleName.IsReadOnly = true;
                cmbClass.IsEnabled = false;
                dpBirthDate.IsEnabled = false;
                txtParentAddress.IsReadOnly = true;

                // Меняем цвет фона полей для индикации режима только для чтения
                txtLastName.Background = System.Windows.Media.Brushes.LightGray;
                txtFirstName.Background = System.Windows.Media.Brushes.LightGray;
                txtMiddleName.Background = System.Windows.Media.Brushes.LightGray;
                txtParentAddress.Background = System.Windows.Media.Brushes.LightGray;
            }
        }
        private void LoadData()
        {
            try
            {
                IQueryable<Student> query = _context.Students.Include(s => s.Class);

                // Если пользователь - учитель, фильтруем учеников (например, по классу учителя)
                if (_currentUser.Role == "Учитель")
                {
                    // Здесь можно добавить логику фильтрации по классу учителя
                    // Например, получить класс учителя из БД и отфильтровать
                }

                // Загрузка учащихся
                _students = query
                    .Select(s => new StudentViewModel
                    {
                        StudentID = s.StudentID,
                        LastName = s.LastName,
                        FirstName = s.FirstName,
                        MiddleName = s.MiddleName,
                        ClassID = s.ClassID,
                        ClassName = s.Class != null ? s.Class.ClassName : "",
                        BirthDate = s.BirthDate,
                        ParentAddress = s.ParentAddress
                    })
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .ToList();

                dgStudents.ItemsSource = _students;

                // Загрузка классов для ComboBox
                var classes = _context.Classes
                    .Select(c => new ClassViewModel
                    {
                        ClassID = c.ClassID,
                        ClassName = c.ClassName
                    })
                    .OrderBy(c => c.ClassName)
                    .ToList();

                cmbClass.ItemsSource = classes;
                cmbClass.DisplayMemberPath = "ClassName";
                cmbClass.SelectedValuePath = "ClassID";

                // Установка даты по умолчанию
                dpBirthDate.SelectedDate = DateTime.Today.AddYears(-10);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgStudents.SelectedItem is StudentViewModel selectedStudent)
            {
                // Заполняем поля данными выбранной записи
                txtStudentID.Text = selectedStudent.StudentID.ToString();
                txtLastName.Text = selectedStudent.LastName;
                txtFirstName.Text = selectedStudent.FirstName;
                txtMiddleName.Text = selectedStudent.MiddleName ?? "";

                // Выбираем класс в ComboBox
                cmbClass.SelectedValue = selectedStudent.ClassID;

                // Устанавливаем дату рождения
                dpBirthDate.SelectedDate = selectedStudent.BirthDate;

                // Устанавливаем адрес
                txtParentAddress.Text = selectedStudent.ParentAddress;
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbClass.SelectedValue == null)
            {
                MessageBox.Show("Выберите класс!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpBirthDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtParentAddress.Text))
            {
                MessageBox.Show("Введите адрес родителей!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role == "Учитель")
            {
                MessageBox.Show("У вас нет прав на добавление учащихся!", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                if (!ValidateFields()) return;

                // Создание нового ученика
                var newStudent = new Student
                {
                    LastName = txtLastName.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim(),
                    ClassID = (int)cmbClass.SelectedValue,
                    BirthDate = dpBirthDate.SelectedDate.Value,
                    ParentAddress = txtParentAddress.Text.Trim()
                };

                _context.Students.Add(newStudent);
                _context.SaveChanges();

                MessageBox.Show("Учащийся успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем данные
                LoadData();
                btnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role == "Учитель")
            {
                MessageBox.Show("У вас нет прав на редактирование учащихся!", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                if (string.IsNullOrEmpty(txtStudentID.Text))
                {
                    MessageBox.Show("Выберите запись для редактирования!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateFields()) return;

                int studentId = int.Parse(txtStudentID.Text);
                var student = _context.Students.Find(studentId);

                if (student == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновление данных
                student.LastName = txtLastName.Text.Trim();
                student.FirstName = txtFirstName.Text.Trim();
                student.MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
                student.ClassID = (int)cmbClass.SelectedValue;
                student.BirthDate = dpBirthDate.SelectedDate.Value;
                student.ParentAddress = txtParentAddress.Text.Trim();

                _context.SaveChanges();

                MessageBox.Show("Данные учащегося успешно обновлены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем данные
                LoadData();
                btnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role == "Учитель")
            {
                MessageBox.Show("У вас нет прав на удаление учащихся!", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(txtStudentID.Text))
                {
                    MessageBox.Show("Выберите запись для удаления!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int studentId = int.Parse(txtStudentID.Text);

                // Получаем ученика со всеми связанными записями
                var student = _context.Students
                    .Include(s => s.Grades)
                    .Include(s => s.Attendances)
                    .FirstOrDefault(s => s.StudentID == studentId);

                if (student == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Подсчитываем количество связанных записей для информации
                int gradesCount = student.Grades?.Count ?? 0;
                int attendanceCount = student.Attendances?.Count ?? 0;

                string message = $"Вы действительно хотите удалить учащегося {student.LastName} {student.FirstName}?\n\n";

                if (gradesCount > 0 || attendanceCount > 0)
                {
                    message += $"У ученика есть связанные записи:\n";
                    if (gradesCount > 0)
                        message += $"• Оценки: {gradesCount} шт.\n";
                    if (attendanceCount > 0)
                        message += $"• Посещаемость: {attendanceCount} записей\n\n";
                    message += "Все эти записи будут также удалены!";
                }
                else
                {
                    message += "У ученика нет связанных записей.";
                }

                var result = MessageBox.Show(message, "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем ученика (связанные записи удалятся автоматически,
                    // так как мы их загрузили через Include)
                    _context.Students.Remove(student);
                    _context.SaveChanges();

                    MessageBox.Show("Учащийся и все связанные записи успешно удалены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем данные
                    LoadData();
                    btnClear_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // Очистка всех полей
            txtStudentID.Text = "";
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            cmbClass.SelectedIndex = -1;
            dpBirthDate.SelectedDate = DateTime.Today.AddYears(-10);
            txtParentAddress.Text = "";

            // Снимаем выделение в DataGrid
            dgStudents.SelectedItem = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchWindow = new SearchWindow();
                searchWindow.Owner = this;

                if (searchWindow.ShowDialog() == true)
                {
                    var criteria = searchWindow.Criteria;
                    ApplyFilter(criteria);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна поиска: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter(SearchCriteria criteria)
        {
            try
            {
                var query = _context.Students
                    .Include(s => s.Class)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(criteria.LastName))
                    query = query.Where(s => s.LastName.Contains(criteria.LastName));

                if (!string.IsNullOrEmpty(criteria.FirstName))
                    query = query.Where(s => s.FirstName.Contains(criteria.FirstName));

                if (!string.IsNullOrEmpty(criteria.ClassName))
                    query = query.Where(s => s.Class.ClassName == criteria.ClassName);

                var filteredStudents = query
                    .Select(s => new StudentViewModel
                    {
                        StudentID = s.StudentID,
                        LastName = s.LastName,
                        FirstName = s.FirstName,
                        MiddleName = s.MiddleName,
                        ClassName = s.Class != null ? s.Class.ClassName : "",
                        BirthDate = s.BirthDate,
                        ParentAddress = s.ParentAddress
                    })
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .ToList();

                dgStudents.ItemsSource = filteredStudents;

                if (!filteredStudents.Any())
                {
                    MessageBox.Show("По заданным критериям ничего не найдено.",
                        "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтра: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}