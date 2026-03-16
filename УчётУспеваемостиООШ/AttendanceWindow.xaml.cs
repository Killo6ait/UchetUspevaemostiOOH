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
    public partial class AttendanceWindow : Window
    {
        private SchoolContext _context = new SchoolContext();
        private List<AttendanceViewModel> _attendances = new List<AttendanceViewModel>();

        public AttendanceWindow()
        {
            InitializeComponent();
            LoadData();
        }


        public class AttendanceViewModel
        {
            public int AttendanceID { get; set; }
            public int StudentID { get; set; }
            public string StudentFullName { get; set; } = string.Empty;
            public string ClassName { get; set; } = string.Empty;
            public DateTime AttendanceDate { get; set; }
            public string Status { get; set; } = string.Empty;
        }


        public class StudentViewModel
        {
            public int StudentID { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string ClassName { get; set; } = string.Empty;
        }

        private void LoadData()
        {
            try
            {

                _attendances = _context.Attendances
                    .Include(a => a.Student)
                        .ThenInclude(s => s != null ? s.Class : null)
                    .Select(a => new AttendanceViewModel
                    {
                        AttendanceID = a.AttendanceID,
                        StudentID = a.StudentID,
                        StudentFullName = (a.Student != null) ?
                            a.Student.LastName + " " +
                            a.Student.FirstName + " " +
                            (a.Student.MiddleName ?? "") : "",
                        ClassName = (a.Student != null && a.Student.Class != null) ?
                            a.Student.Class.ClassName : "",
                        AttendanceDate = a.Date,
                        Status = a.Status
                    })
                    .OrderByDescending(a => a.AttendanceDate)
                    .ThenBy(a => a.StudentFullName)
                    .ToList();

                dgAttendance.ItemsSource = _attendances;

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


                dpDate.SelectedDate = DateTime.Today;


                if (cmbStatus.Items.Count > 0)
                {
                    cmbStatus.SelectedIndex = 0; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgAttendance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAttendance.SelectedItem is AttendanceViewModel selectedAttendance)
            {
          
                txtAttendanceID.Text = selectedAttendance.AttendanceID.ToString();

                cmbStudent.SelectedValue = selectedAttendance.StudentID;

         
                dpDate.SelectedDate = selectedAttendance.AttendanceDate;

                foreach (ComboBoxItem item in cmbStatus.Items)
                {
                    if (item.Content.ToString() == selectedAttendance.Status)
                    {
                        cmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
      
                if (cmbStudent.SelectedValue == null)
                {
                    MessageBox.Show("Выберите ученика!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbStatus.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

          
                var existingAttendance = _context.Attendances
                    .FirstOrDefault(a => a.StudentID == (int)cmbStudent.SelectedValue &&
                                         a.Date == dpDate.SelectedDate.Value);

                if (existingAttendance != null)
                {
                    MessageBox.Show("Запись посещаемости для этого ученика на выбранную дату уже существует!",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

  
                var newAttendance = new Attendance
                {
                    StudentID = (int)cmbStudent.SelectedValue,
                    Date = dpDate.SelectedDate.Value,
                    Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString()!
                };

                _context.Attendances.Add(newAttendance);
                _context.SaveChanges();

                MessageBox.Show("Запись посещаемости успешно добавлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);


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
                if (string.IsNullOrEmpty(txtAttendanceID.Text))
                {
                    MessageBox.Show("Выберите запись для редактирования!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int attendanceId = int.Parse(txtAttendanceID.Text);
                var attendance = _context.Attendances.Find(attendanceId);

                if (attendance == null)
                {
                    MessageBox.Show("Запись не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                if (cmbStudent.SelectedValue == null)
                {
                    MessageBox.Show("Выберите ученика!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbStatus.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

    
                var existingAttendance = _context.Attendances
                    .FirstOrDefault(a => a.StudentID == (int)cmbStudent.SelectedValue &&
                                         a.Date == dpDate.SelectedDate.Value &&
                                         a.AttendanceID != attendanceId);

                if (existingAttendance != null)
                {
                    MessageBox.Show("Запись посещаемости для этого ученика на выбранную дату уже существует!",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                attendance.StudentID = (int)cmbStudent.SelectedValue;
                attendance.Date = dpDate.SelectedDate.Value;
                attendance.Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString()!;

                _context.SaveChanges();

                MessageBox.Show("Запись посещаемости успешно обновлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

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
                if (string.IsNullOrEmpty(txtAttendanceID.Text))
                {
                    MessageBox.Show("Выберите запись для удаления!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы действительно хотите удалить эту запись посещаемости?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int attendanceId = int.Parse(txtAttendanceID.Text);
                    var attendance = _context.Attendances.Find(attendanceId);

                    if (attendance != null)
                    {
                        _context.Attendances.Remove(attendance);
                        _context.SaveChanges();

                        MessageBox.Show("Запись посещаемости успешно удалена!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

   
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
    
            txtAttendanceID.Text = "";
            cmbStudent.SelectedIndex = -1;
            dpDate.SelectedDate = DateTime.Today;
            cmbStatus.SelectedIndex = 0; 

   
            dgAttendance.SelectedItem = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}