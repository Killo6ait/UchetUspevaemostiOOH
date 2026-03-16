using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using УчётУспеваемостиООШ.Data;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ
{
    public partial class ChartWindow : Window
    {
        private SchoolContext _context = new SchoolContext();
        private ChartService? _chartService;
        private string _selectedClassForChart = "9А"; 

        public ChartWindow()
        {
            InitializeComponent();
            _chartService = new ChartService(_context);
            LoadClasses();
            LoadClassAverageChart();
            LoadStudentsAndSubjects();
        }

        private void LoadClasses()
        {
            try
            {
                var classes = _context.Classes
                    .OrderBy(c => c.ClassName)
                    .Select(c => c.ClassName)
                    .ToList();

                cmbClassForChart.ItemsSource = classes;

                if (classes.Count > 0)
                {
                    cmbClassForChart.SelectedItem = _selectedClassForChart;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке классов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbClassForChart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbClassForChart.SelectedItem != null)
            {
                _selectedClassForChart = cmbClassForChart.SelectedItem.ToString()!;
                LoadClassAverageChart(); 
            }
        }

        private void LoadClassAverageChart()
        {
            try
            {
                if (_chartService == null) return;

                ClassAverageChart.Series = _chartService.GetClassAverageChart(_selectedClassForChart);

                ClassAverageChart.AxisX.Clear();
                var axisX = new LiveCharts.Wpf.Axis
                {
                    Title = "Классы",
                    Labels = new[] { _selectedClassForChart }
                };
                ClassAverageChart.AxisX.Add(axisX);

                ClassAverageChart.AxisY.Clear();
                var axisY = new LiveCharts.Wpf.Axis
                {
                    Title = "Средний балл",
                    MinValue = 2,
                    MaxValue = 5
                };
                ClassAverageChart.AxisY.Add(axisY);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для графика: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStudentsAndSubjects()
        {
            try
            {
                var students = _context.Students
                    .Select(s => new
                    {
                        s.StudentID,
                        FullName = s.LastName + " " + s.FirstName + " " + (s.MiddleName ?? "")
                    })
                    .OrderBy(s => s.FullName)
                    .ToList();

                cmbStudent.ItemsSource = students;
                cmbStudent.DisplayMemberPath = "FullName";
                cmbStudent.SelectedValuePath = "StudentID";

                var subjects = _context.Subjects
                    .Select(s => new
                    {
                        s.SubjectID,
                        s.SubjectName
                    })
                    .OrderBy(s => s.SubjectName)
                    .ToList();

                cmbSubject.ItemsSource = subjects;
                cmbSubject.DisplayMemberPath = "SubjectName";
                cmbSubject.SelectedValuePath = "SubjectID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списков: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnShowProgress_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_chartService == null) return;

                if (cmbStudent.SelectedValue == null || cmbSubject.SelectedValue == null)
                {
                    MessageBox.Show("Выберите ученика и предмет!", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int studentId = (int)cmbStudent.SelectedValue;
                int subjectId = (int)cmbSubject.SelectedValue;

                ProgressChart.Series = _chartService.GetStudentProgressChart(studentId, subjectId);


                ProgressChart.AxisX.Clear();
                ProgressChart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Дата" });

                ProgressChart.AxisY.Clear();
                ProgressChart.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Оценка",
                    MinValue = 2,
                    MaxValue = 5
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении графика: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}