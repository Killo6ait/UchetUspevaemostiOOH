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
    public partial class SubjectsWindow : BaseWindow
    {
        private SchoolContext _context = new SchoolContext();
        private List<SubjectViewModel> _subjects = new List<SubjectViewModel>();

        public SubjectsWindow(User user) : base(user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            InitializeComponent();
            ApplyCurrentSettings();
            LoadData();
            ApplyPermissions();
        }

        private void ApplyPermissions()
        {
            if (_currentUser.Role == "Учитель")
            {
                // Логируем, что учитель открыл окно предметов
                Logger.Info($"Teacher {_currentUser.Username} opened Subjects window (read-only mode)", "Security");

                btnAdd.IsEnabled = false;
                btnAdd.Opacity = 0.5;
                btnAdd.ToolTip = "Добавление запрещено для вашей роли";

                btnUpdate.IsEnabled = false;
                btnUpdate.Opacity = 0.5;
                btnUpdate.ToolTip = "Редактирование запрещено для вашей роли";

                btnDelete.IsEnabled = false;
                btnDelete.Opacity = 0.5;
                btnDelete.ToolTip = "Удаление запрещено для вашей роли";

                txtSubjectName.IsReadOnly = true;
                cmbTeacher.IsEnabled = false;
                cmbHoursPerWeek.IsEnabled = false;

                txtSubjectName.Background = System.Windows.Media.Brushes.LightGray;
            }
            else if (_currentUser.Role == "Завуч")
            {
                // Завуч не может добавлять/удалять предметы
                Logger.Info($"Zavuch {_currentUser.Username} opened Subjects window (limited access)", "Security");

                btnAdd.IsEnabled = false;
                btnUpdate.IsEnabled = false;
                btnDelete.IsEnabled = false;

                btnAdd.ToolTip = "Завуч не может добавлять предметы";
                btnUpdate.ToolTip = "Завуч не может изменять предметы";
                btnDelete.ToolTip = "Завуч не может удалять предметы";
            }
        }

        public class SubjectViewModel
        {
            public int SubjectID { get; set; }
            public string SubjectName { get; set; } = string.Empty;
            public int TeacherID { get; set; }
            public string TeacherName { get; set; } = string.Empty;
            public byte HoursPerWeek { get; set; }
        }

        public class TeacherViewModel
        {
            public int TeacherID { get; set; }
            public string FullName { get; set; } = string.Empty;
        }

        private void LoadData()
        {
            try
            {
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
                Logger.Error($"Error loading subjects: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgSubjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSubjects.SelectedItem is SubjectViewModel selectedSubject)
            {
                txtSubjectID.Text = selectedSubject.SubjectID.ToString();
                txtSubjectName.Text = selectedSubject.SubjectName;
                cmbTeacher.SelectedValue = selectedSubject.TeacherID;

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

        // CREATE - Добавление предмета
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Логируем попытку добавления
            Logger.Info($"User {_currentUser.Username} attempted to ADD subject", "Security");

            if (_currentUser.Role == "Учитель" || _currentUser.Role == "Завуч")
            {
                Logger.Warning($"{_currentUser.Role} {_currentUser.Username} attempted to add subject - ACCESS DENIED", "Security");
                MessageBox.Show("У вас нет прав на добавление предметов!", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!ValidateFields()) return;

                var newSubject = new Subject
                {
                    SubjectName = txtSubjectName.Text.Trim(),
                    TeacherID = (int)cmbTeacher.SelectedValue,
                    HoursPerWeek = byte.Parse(((ComboBoxItem)cmbHoursPerWeek.SelectedItem).Tag.ToString()!)
                };

                _context.Subjects.Add(newSubject);
                _context.SaveChanges();

                // АУДИТ: УСПЕШНОЕ ДОБАВЛЕНИЕ
                Logger.Audit(_currentUser.Username, "Added record to Subjects",
                    $"ID={newSubject.SubjectID}, Name={newSubject.SubjectName}");
                Logger.Info($"Record added to Subjects table", "DB");

                MessageBox.Show("Предмет успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
                btnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding subject: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // UPDATE - Редактирование предмета
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Логируем попытку изменения
            Logger.Info($"User {_currentUser.Username} attempted to UPDATE subject", "Security");

            if (_currentUser.Role == "Учитель" || _currentUser.Role == "Завуч")
            {
                Logger.Warning($"{_currentUser.Role} {_currentUser.Username} attempted to update subject - ACCESS DENIED", "Security");
                MessageBox.Show("У вас нет прав на редактирование предметов!", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

                string oldName = subject.SubjectName;
                int oldTeacherId = subject.TeacherID;
                byte oldHours = subject.HoursPerWeek;

                subject.SubjectName = txtSubjectName.Text.Trim();
                subject.TeacherID = (int)cmbTeacher.SelectedValue;
                subject.HoursPerWeek = byte.Parse(((ComboBoxItem)cmbHoursPerWeek.SelectedItem).Tag.ToString()!);

                _context.SaveChanges();

                // АУДИТ: УСПЕШНОЕ ИЗМЕНЕНИЕ
                Logger.Audit(_currentUser.Username, "Updated record in Subjects",
                    $"ID={subjectId}, Name: '{oldName}' → '{subject.SubjectName}'");
                Logger.Info($"Record updated in Subjects table", "DB");

                MessageBox.Show("Данные предмета успешно обновлены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
                btnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating subject ID={txtSubjectID.Text}: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // DELETE - Удаление предмета
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Логируем попытку удаления
            Logger.Info($"User {_currentUser.Username} attempted to DELETE subject", "Security");

            if (_currentUser.Role == "Учитель" || _currentUser.Role == "Завуч")
            {
                Logger.Warning($"{_currentUser.Role} {_currentUser.Username} attempted to delete subject - ACCESS DENIED", "Security");
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
                var subject = _context.Subjects
                    .Include(s => s.Grades)
                    .FirstOrDefault(s => s.SubjectID == subjectId);

                if (subject == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int gradesCount = subject.Grades?.Count ?? 0;
                string message = $"Вы действительно хотите удалить предмет \"{subject.SubjectName}\"?\n\n";

                if (gradesCount > 0)
                {
                    message += $"❗ ВНИМАНИЕ: По этому предмету есть оценки: {gradesCount} шт.\n";
                    message += "Все эти оценки будут также удалены!";
                }

                var result = MessageBox.Show(message, "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (gradesCount > 0)
                    {
                        var grades = _context.Grades.Where(g => g.SubjectID == subjectId);
                        _context.Grades.RemoveRange(grades);
                    }

                    _context.Subjects.Remove(subject);
                    _context.SaveChanges();

                    // АУДИТ: УСПЕШНОЕ УДАЛЕНИЕ
                    Logger.Audit(_currentUser.Username, "Deleted record from Subjects",
                        $"ID={subjectId}, Name={subject.SubjectName}, Deleted grades={gradesCount}");
                    Logger.Info($"Record deleted from Subjects table", "DB");

                    MessageBox.Show("Предмет и все связанные оценки успешно удалены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                    btnClear_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error deleting subject ID={txtSubjectID.Text}: {ex.Message}", ex, "DB");
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSubjectID.Text = "";
            txtSubjectName.Text = "";
            cmbTeacher.SelectedIndex = -1;
            cmbHoursPerWeek.SelectedIndex = 2;
            dgSubjects.SelectedItem = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}