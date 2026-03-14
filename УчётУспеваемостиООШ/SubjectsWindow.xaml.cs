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
    public partial class SubjectsWindow : Window
    {
        private SchoolContext _context = new SchoolContext();
        private List<SubjectViewModel> _subjects = new List<SubjectViewModel>();
        private User _currentUser;

        public SubjectsWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadData();
            ApplyPermissions();
        }
        private void ApplyPermissions()
        {
            if (_currentUser.Role == "Учитель")
            {
                btnAdd.IsEnabled = false;
                btnAdd.Opacity = 0.5;
                btnUpdate.IsEnabled = false;
                btnUpdate.Opacity = 0.5;
                btnDelete.IsEnabled = false;
                btnDelete.Opacity = 0.5;

                txtSubjectName.IsReadOnly = true;
                cmbTeacher.IsEnabled = false;
                cmbHoursPerWeek.IsEnabled = false;

                txtSubjectName.Background = System.Windows.Media.Brushes.LightGray;
            }
        }
        // Класс для отображения в DataGrid
        public class SubjectViewModel
        {
            public int SubjectID { get; set; }
            public string SubjectName { get; set; } = string.Empty;
            public int TeacherID { get; set; }
            public string TeacherName { get; set; } = string.Empty;
            public byte HoursPerWeek { get; set; }
        }

        // Класс для отображения учителей в ComboBox
        public class TeacherViewModel
        {
            public int TeacherID { get; set; }
            public string FullName { get; set; } = string.Empty;
        }

        private void LoadData()
        {
            try
            {
                // Загрузка предметов
                _subjects = _context.Subjects
                    .Include(s => s.Teacher)
                    .Select(s => new SubjectViewModel
                    {
                        SubjectID = s.SubjectID,
                        SubjectName = s.SubjectName,
                        TeacherID = s.TeacherID,
                        TeacherName = s.Teacher != null ?
                            s.Teacher.LastName + " " +
                            s.Teacher.FirstName + " " +
                            (s.Teacher.MiddleName ?? "") : "",
                        HoursPerWeek = s.HoursPerWeek
                    })
                    .OrderBy(s => s.SubjectName)
                    .ToList();

                dgSubjects.ItemsSource = _subjects;

                // Загрузка учителей для ComboBox
                var teachers = _context.Teachers
                    .Select(t => new TeacherViewModel
                    {
                        TeacherID = t.TeacherID,
                        FullName = t.LastName + " " + t.FirstName + " " + (t.MiddleName ?? "")
                    })
                    .OrderBy(t => t.FullName)
                    .ToList();

                cmbTeacher.ItemsSource = teachers;
                cmbTeacher.DisplayMemberPath = "FullName";
                cmbTeacher.SelectedValuePath = "TeacherID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgSubjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSubjects.SelectedItem is SubjectViewModel selectedSubject)
            {
                // Заполняем поля данными выбранной записи
                txtSubjectID.Text = selectedSubject.SubjectID.ToString();
                txtSubjectName.Text = selectedSubject.SubjectName;

                // Выбираем учителя в ComboBox
                cmbTeacher.SelectedValue = selectedSubject.TeacherID;

                // Выбираем часы в неделю
                foreach (ComboBoxItem item in cmbHoursPerWeek.Items)
                {
                    if (item.Tag.ToString() == selectedSubject.HoursPerWeek.ToString())
                    {
                        cmbHoursPerWeek.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtSubjectName.Text))
            {
                MessageBox.Show("Введите название предмета!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbTeacher.SelectedValue == null)
            {
                MessageBox.Show("Выберите учителя!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbHoursPerWeek.SelectedItem == null)
            {
                MessageBox.Show("Выберите количество часов в неделю!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка на уникальность названия предмета
            if (_subjects.Any(s => s.SubjectName.Equals(txtSubjectName.Text.Trim(),
                StringComparison.OrdinalIgnoreCase) &&
                s.SubjectID.ToString() != txtSubjectID.Text))
            {
                MessageBox.Show("Предмет с таким названием уже существует!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateFields()) return;

                // Создание нового предмета
                var newSubject = new Subject
                {
                    SubjectName = txtSubjectName.Text.Trim(),
                    TeacherID = (int)cmbTeacher.SelectedValue,
                    HoursPerWeek = byte.Parse(((ComboBoxItem)cmbHoursPerWeek.SelectedItem).Tag.ToString()!)
                };

                _context.Subjects.Add(newSubject);
                _context.SaveChanges();

                MessageBox.Show("Предмет успешно добавлен!", "Успех",
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
            try
            {
                if (string.IsNullOrEmpty(txtSubjectID.Text))
                {
                    MessageBox.Show("Выберите запись для редактирования!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateFields()) return;

                int subjectId = int.Parse(txtSubjectID.Text);
                var subject = _context.Subjects.Find(subjectId);

                if (subject == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновление данных
                subject.SubjectName = txtSubjectName.Text.Trim();
                subject.TeacherID = (int)cmbTeacher.SelectedValue;
                subject.HoursPerWeek = byte.Parse(((ComboBoxItem)cmbHoursPerWeek.SelectedItem).Tag.ToString()!);

                _context.SaveChanges();

                MessageBox.Show("Данные предмета успешно обновлены!", "Успех",
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
            // Проверка прав доступа
            if (_currentUser.Role == "Учитель")
            {
                MessageBox.Show("У вас нет прав на удаление предметов!", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(txtSubjectID.Text))
                {
                    MessageBox.Show("Выберите запись для удаления!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int subjectId = int.Parse(txtSubjectID.Text);

                // Получаем предмет со всеми связанными оценками
                var subject = _context.Subjects
                    .Include(s => s.Grades)
                    .FirstOrDefault(s => s.SubjectID == subjectId);

                if (subject == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Подсчитываем количество связанных оценок
                int gradesCount = subject.Grades?.Count ?? 0;

                string message = $"Вы действительно хотите удалить предмет \"{subject.SubjectName}\"?\n\n";

                if (gradesCount > 0)
                {
                    message += $"❗ ВНИМАНИЕ: По этому предмету есть оценки: {gradesCount} шт.\n";
                    message += "Все эти оценки будут также удалены!";
                }
                else
                {
                    message += "У этого предмета нет связанных оценок.";
                }

                var result = MessageBox.Show(message, "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Если есть связанные оценки, удаляем их вручную
                    if (gradesCount > 0)
                    {
                        var grades = _context.Grades.Where(g => g.SubjectID == subjectId);
                        _context.Grades.RemoveRange(grades);
                    }

                    // Удаляем сам предмет
                    _context.Subjects.Remove(subject);
                    _context.SaveChanges();

                    MessageBox.Show("Предмет и все связанные оценки успешно удалены!", "Успех",
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
            txtSubjectID.Text = "";
            txtSubjectName.Text = "";
            cmbTeacher.SelectedIndex = -1;
            cmbHoursPerWeek.SelectedIndex = 2; // 3 часа по умолчанию

            // Снимаем выделение в DataGrid
            dgSubjects.SelectedItem = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}