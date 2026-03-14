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
    public partial class GradesWindow : Window
    {
        private SchoolContext _context = new SchoolContext();
        private List<GradeViewModel> _grades = new List<GradeViewModel>();
        private User _currentUser;

        public GradesWindow(User user)
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
                // Учитель может добавлять, редактировать и удалять оценки, 
                // но только по своим предметам (фильтрация будет в LoadData)
                // Оставляем кнопки активными, но добавим проверки в методы

                // Можно добавить подсказку
                btnAdd.ToolTip = "Добавление оценки (только по вашим предметам)";
                btnUpdate.ToolTip = "Редактирование оценки (только по вашим предметам)";
                btnDelete.ToolTip = "Удаление оценки (только по вашим предметам)";
            }
        }

        // Класс для отображения в DataGrid
        public class GradeViewModel
        {
            public int GradeID { get; set; }
            public int StudentID { get; set; }
            public string StudentFullName { get; set; } = string.Empty;
            public string ClassName { get; set; } = string.Empty;
            public int SubjectID { get; set; }
            public string SubjectName { get; set; } = string.Empty;
            public int GradeValue { get; set; }
            public DateTime GradeDate { get; set; }
            public string ControlType { get; set; } = string.Empty;
        }

        // Класс для отображения учеников в ComboBox
        public class StudentViewModel
        {
            public int StudentID { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string ClassName { get; set; } = string.Empty;
        }

        // Класс для отображения предметов в ComboBox
        public class SubjectViewModel
        {
            public int SubjectID { get; set; }
            public string SubjectName { get; set; } = string.Empty;
        }

        private void LoadData()
        {
            try
            {
                IQueryable<Grade> query = _context.Grades
                    .Include(g => g.Student)
                        .ThenInclude(s => s != null ? s.Class : null)
                    .Include(g => g.Subject);

                // Если учитель - фильтруем оценки по его предметам
                if (_currentUser.Role == "Учитель" && _currentUser.TeacherID != null)
                {
                    var teacherSubjects = _context.Subjects
                        .Where(s => s.TeacherID == _currentUser.TeacherID)
                        .Select(s => s.SubjectID)
                        .ToList();

                    query = query.Where(g => teacherSubjects.Contains(g.SubjectID));
                }

                // Загрузка оценок
                _grades = query
                    .Select(g => new GradeViewModel
                    {
                        GradeID = g.GradeID,
                        StudentID = g.StudentID,
                        StudentFullName = (g.Student != null) ?
                            g.Student.LastName + " " +
                            g.Student.FirstName + " " +
                            (g.Student.MiddleName ?? "") : "",
                        ClassName = (g.Student != null && g.Student.Class != null) ?
                            g.Student.Class.ClassName : "",
                        SubjectID = g.SubjectID,
                        SubjectName = g.Subject != null ? g.Subject.SubjectName : "",
                        GradeValue = g.GradeValue,
                        GradeDate = g.Date,
                        ControlType = g.ControlType
                    })
                    .OrderByDescending(g => g.GradeDate)
                    .ToList();

                dgGrades.ItemsSource = _grades;

                // Загрузка учеников для ComboBox
                var students = _context.Students
                    .Include(s => s.Class)
                    .Select(s => new StudentViewModel
                    {
                        StudentID = s.StudentID,
                        FullName = s.LastName + " " + s.FirstName + " " + (s.MiddleName ?? ""),
                        ClassName = s.Class != null ? s.Class.ClassName : ""
                    })
                    .OrderBy(s => s.FullName)
                    .ToList();

                cmbStudent.ItemsSource = students;
                cmbStudent.DisplayMemberPath = "FullName";
                cmbStudent.SelectedValuePath = "StudentID";

                // Загрузка предметов для ComboBox (с фильтром для учителя)
                IQueryable<Subject> subjectQuery = _context.Subjects;

                if (_currentUser.Role == "Учитель" && _currentUser.TeacherID != null)
                {
                    subjectQuery = subjectQuery.Where(s => s.TeacherID == _currentUser.TeacherID);
                }

                var subjects = subjectQuery
                    .Select(s => new SubjectViewModel
                    {
                        SubjectID = s.SubjectID,
                        SubjectName = s.SubjectName
                    })
                    .OrderBy(s => s.SubjectName)
                    .ToList();

                cmbSubject.ItemsSource = subjects;
                cmbSubject.DisplayMemberPath = "SubjectName";
                cmbSubject.SelectedValuePath = "SubjectID";

                dpDate.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgGrades_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgGrades.SelectedItem is GradeViewModel selectedGrade)
            {
                // Заполняем поля данными выбранной записи
                txtGradeID.Text = selectedGrade.GradeID.ToString();

                // Выбираем ученика в ComboBox
                cmbStudent.SelectedValue = selectedGrade.StudentID;

                // Выбираем предмет в ComboBox
                cmbSubject.SelectedValue = selectedGrade.SubjectID;

                // Выбираем оценку
                foreach (ComboBoxItem item in cmbGrade.Items)
                {
                    if (item.Content.ToString() == selectedGrade.GradeValue.ToString())
                    {
                        cmbGrade.SelectedItem = item;
                        break;
                    }
                }

                // Устанавливаем дату
                dpDate.SelectedDate = selectedGrade.GradeDate;

                // Выбираем тип контроля
                foreach (ComboBoxItem item in cmbControlType.Items)
                {
                    if (item.Content.ToString() == selectedGrade.ControlType)
                    {
                        cmbControlType.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role == "Учитель" && _currentUser.TeacherID != null)
            {
                int subjectId = (int)cmbSubject.SelectedValue;
                var subject = _context.Subjects.Find(subjectId);

                if (subject == null || subject.TeacherID != _currentUser.TeacherID)
                {
                    MessageBox.Show("Вы можете добавлять оценки только по своим предметам!",
                        "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            try
            {
                // Проверка заполнения полей
                if (cmbStudent.SelectedValue == null)
                {
                    MessageBox.Show("Выберите ученика!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbSubject.SelectedValue == null)
                {
                    MessageBox.Show("Выберите предмет!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbGrade.SelectedItem == null)
                {
                    MessageBox.Show("Выберите оценку!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbControlType.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип контроля!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создание новой оценки
                var newGrade = new Grade
                {
                    StudentID = (int)cmbStudent.SelectedValue,
                    SubjectID = (int)cmbSubject.SelectedValue,
                    GradeValue = (byte)int.Parse(((ComboBoxItem)cmbGrade.SelectedItem).Content.ToString()!), // ! - подавляем предупреждение о null
                    Date = dpDate.SelectedDate.Value,
                    ControlType = ((ComboBoxItem)cmbControlType.SelectedItem).Content.ToString()!
                };

                _context.Grades.Add(newGrade);
                _context.SaveChanges();

                MessageBox.Show("Оценка успешно добавлена!", "Успех",
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
                if (string.IsNullOrEmpty(txtGradeID.Text))
                {
                    MessageBox.Show("Выберите запись для редактирования!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int gradeId = int.Parse(txtGradeID.Text);
                var grade = _context.Grades.Find(gradeId);

                if (grade == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка заполнения полей
                if (cmbStudent.SelectedValue == null)
                {
                    MessageBox.Show("Выберите ученика!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbSubject.SelectedValue == null)
                {
                    MessageBox.Show("Выберите предмет!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbGrade.SelectedItem == null)
                {
                    MessageBox.Show("Выберите оценку!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbControlType.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип контроля!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Обновление данных
                grade.StudentID = (int)cmbStudent.SelectedValue;
                grade.SubjectID = (int)cmbSubject.SelectedValue;
                grade.GradeValue = (byte)int.Parse(((ComboBoxItem)cmbGrade.SelectedItem).Content.ToString()!);
                grade.Date = dpDate.SelectedDate.Value;
                grade.ControlType = ((ComboBoxItem)cmbControlType.SelectedItem).Content.ToString()!;

                _context.SaveChanges();

                MessageBox.Show("Оценка успешно обновлена!", "Успех",
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
            try
            {
                if (string.IsNullOrEmpty(txtGradeID.Text))
                {
                    MessageBox.Show("Выберите запись для удаления!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы действительно хотите удалить эту оценку?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int gradeId = int.Parse(txtGradeID.Text);
                    var grade = _context.Grades.Find(gradeId);

                    if (grade != null)
                    {
                        _context.Grades.Remove(grade);
                        _context.SaveChanges();

                        MessageBox.Show("Оценка успешно удалена!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Обновляем данные
                        LoadData();
                        btnClear_Click(sender, e);
                    }
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
            txtGradeID.Text = "";
            cmbStudent.SelectedIndex = -1;
            cmbSubject.SelectedIndex = -1;
            cmbGrade.SelectedIndex = -1;
            dpDate.SelectedDate = DateTime.Today;
            cmbControlType.SelectedIndex = -1;

            // Снимаем выделение в DataGrid
            dgGrades.SelectedItem = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}