using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Models;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ
{
    public partial class StudentsWindow : BaseWindow
    {
        private SchoolContext _context = new SchoolContext();
        private List<StudentViewModel> _students = new List<StudentViewModel>();
        private User _currentUser;

        public StudentsWindow(User user) : base(user)  // Правильный вызов конструктора
        {
            InitializeComponent();
            _currentUser = user;
            LoadData();
            ApplyPermissions();
        }

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

        public class ClassViewModel
        {
            public int ClassID { get; set; }
            public string ClassName { get; set; } = string.Empty;
        }

        private void ApplyPermissions()
        {
            if (_currentUser.Role == "Учитель")
            {
                // Логируем, что учитель открыл окно учащихся
                Logger.Info($"Teacher {_currentUser.Username} opened Students window (read-only mode)", "Security");

                btnAdd.IsEnabled = false;
                btnAdd.Opacity = 0.5;
                btnAdd.ToolTip = "Добавление запрещено для вашей роли";

                btnUpdate.IsEnabled = false;
                btnUpdate.Opacity = 0.5;
                btnUpdate.ToolTip = "Редактирование запрещено для вашей роли";

                btnDelete.IsEnabled = false;
                btnDelete.Opacity = 0.5;
                btnDelete.ToolTip = "Удаление запрещено для вашей роли";

                txtLastName.IsReadOnly = true;
                txtFirstName.IsReadOnly = true;
                txtMiddleName.IsReadOnly = true;
                cmbClass.IsEnabled = false;
                dpBirthDate.IsEnabled = false;
                txtParentAddress.IsReadOnly = true;

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

                // Для учителя просто убираем фильтрацию – он видит всех учеников
                // (но кнопки добавления/удаления уже заблокированы)

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

                dpBirthDate.SelectedDate = DateTime.Today.AddYears(-10);

                // Логируем для учителя
                if (_currentUser.Role == "Учитель")
                {
                    Logger.Info($"Teacher {_currentUser.Username} viewed students list (read-only)", "Security");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading students: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgStudents.SelectedItem is StudentViewModel selectedStudent)
            {
                txtStudentID.Text = selectedStudent.StudentID.ToString();
                txtLastName.Text = selectedStudent.LastName;
                txtFirstName.Text = selectedStudent.FirstName;
                txtMiddleName.Text = selectedStudent.MiddleName ?? "";
                cmbClass.SelectedValue = selectedStudent.ClassID;
                dpBirthDate.SelectedDate = selectedStudent.BirthDate;
                txtParentAddress.Text = selectedStudent.ParentAddress;
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (cmbClass.SelectedValue == null)
            {
                MessageBox.Show("Выберите класс!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (dpBirthDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtParentAddress.Text))
            {
                MessageBox.Show("Введите адрес родителей!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        // CREATE - Добавление записи
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Логируем попытку добавления (даже если у учителя нет прав)
            Logger.Info($"User {_currentUser.Username} attempted to ADD student", "Security");

            if (_currentUser.Role == "Учитель")
            {
                Logger.Warning($"Teacher {_currentUser.Username} attempted to add student - ACCESS DENIED", "Security");
                MessageBox.Show("У вас нет прав на добавление учащихся!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!ValidateFields()) return;

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

                // АУДИТ: УСПЕШНОЕ ДОБАВЛЕНИЕ
                Logger.Audit(_currentUser.Username, "Added record to Students",
                    $"ID={newStudent.StudentID}, {newStudent.LastName} {newStudent.FirstName}");
                Logger.Info($"Record added to Students table", "DB");

                MessageBox.Show("Учащийся успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
                btnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding student: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // UPDATE - Изменение записи
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Логируем попытку изменения
            Logger.Info($"User {_currentUser.Username} attempted to UPDATE student", "Security");

            if (_currentUser.Role == "Учитель")
            {
                Logger.Warning($"Teacher {_currentUser.Username} attempted to update student - ACCESS DENIED", "Security");
                MessageBox.Show("У вас нет прав на редактирование учащихся!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(txtStudentID.Text))
                {
                    MessageBox.Show("Выберите запись для редактирования!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateFields()) return;

                int studentId = int.Parse(txtStudentID.Text);
                var student = _context.Students.Find(studentId);

                if (student == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                student.LastName = txtLastName.Text.Trim();
                student.FirstName = txtFirstName.Text.Trim();
                student.MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
                student.ClassID = (int)cmbClass.SelectedValue;
                student.BirthDate = dpBirthDate.SelectedDate.Value;
                student.ParentAddress = txtParentAddress.Text.Trim();

                _context.SaveChanges();

                // АУДИТ: УСПЕШНОЕ ИЗМЕНЕНИЕ
                Logger.Audit(_currentUser.Username, "Updated record in Students", $"ID={studentId}");
                Logger.Info($"Record updated in Students table", "DB");

                MessageBox.Show("Данные учащегося успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
                btnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating student: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // DELETE - Удаление записи
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Логируем попытку удаления
            Logger.Info($"User {_currentUser.Username} attempted to DELETE student", "Security");

            if (_currentUser.Role == "Учитель")
            {
                Logger.Warning($"Teacher {_currentUser.Username} attempted to delete student - ACCESS DENIED", "Security");
                MessageBox.Show("У вас нет прав на удаление учащихся!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(txtStudentID.Text))
                {
                    MessageBox.Show("Выберите запись для удаления!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int studentId = int.Parse(txtStudentID.Text);
                var student = _context.Students
                    .Include(s => s.Grades)
                    .Include(s => s.Attendances)
                    .FirstOrDefault(s => s.StudentID == studentId);

                if (student == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Вы действительно хотите удалить учащегося {student.LastName} {student.FirstName}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Students.Remove(student);
                    _context.SaveChanges();

                    // АУДИТ: УСПЕШНОЕ УДАЛЕНИЕ
                    Logger.Audit(_currentUser.Username, "Deleted record from Students", $"ID={studentId}");
                    Logger.Info($"Record deleted from Students table", "DB");

                    MessageBox.Show("Учащийся и все связанные записи успешно удалены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                    btnClear_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error deleting student: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtStudentID.Text = "";
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            cmbClass.SelectedIndex = -1;
            dpBirthDate.SelectedDate = DateTime.Today.AddYears(-10);
            txtParentAddress.Text = "";
            dgStudents.SelectedItem = null;
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
                Logger.Error($"Error opening search window: {ex.Message}", ex, "UI");
                MessageBox.Show($"Ошибка при открытии окна поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter(SearchCriteria criteria)
        {
            try
            {
                var query = _context.Students.Include(s => s.Class).AsQueryable();

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
                    MessageBox.Show("По заданным критериям ничего не найдено.", "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error applying filter: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при фильтрации данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Logger.Error($"Error resetting filter: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при сбросе фильтра: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}