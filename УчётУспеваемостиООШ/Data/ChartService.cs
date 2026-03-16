using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using УчётУспеваемостиООШ.Data;

namespace УчётУспеваемостиООШ.Services
{
    public class ChartService
    {
        private SchoolContext _context;

        public ChartService(SchoolContext context)
        {
            _context = context;
        }


        public SeriesCollection GetClassAverageChart()
        {
            try
            {
                var seriesCollection = new SeriesCollection();
                var classes = _context.Classes.OrderBy(c => c.ClassName).ToList();

                var columnSeries = new ColumnSeries
                {
                    Title = "Средний балл",
                    Values = new ChartValues<double>(),
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F2")
                };

                foreach (var cls in classes)
                {
                    var averageGrade = _context.Grades
                        .Where(g => g.Student != null && g.Student.ClassID == cls.ClassID)
                        .Average(g => (double?)g.GradeValue) ?? 0;

                    columnSeries.Values.Add(Math.Round(averageGrade, 2));
                }

                seriesCollection.Add(columnSeries);
                return seriesCollection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при подготовке данных для графика: {ex.Message}");
            }
        }


        public SeriesCollection GetClassAverageChart(string className)
        {
            try
            {
                var seriesCollection = new SeriesCollection();

                var classData = _context.Classes
                    .FirstOrDefault(c => c.ClassName == className);

                if (classData == null)
                {
                    throw new Exception($"Класс {className} не найден");
                }

                var averageGrade = _context.Grades
                    .Where(g => g.Student != null && g.Student.ClassID == classData.ClassID)
                    .Average(g => (double?)g.GradeValue) ?? 0;

                var columnSeries = new ColumnSeries
                {
                    Title = $"Средний балл - {className}",
                    Values = new ChartValues<double> { Math.Round(averageGrade, 2) },
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F2")
                };

                seriesCollection.Add(columnSeries);
                return seriesCollection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при подготовке данных для графика: {ex.Message}");
            }
        }

        public SeriesCollection GetStudentProgressChart(int studentId, int subjectId)
        {
            try
            {
                var grades = _context.Grades
                    .Include(g => g.Subject)
                    .Where(g => g.StudentID == studentId && g.SubjectID == subjectId)
                    .OrderBy(g => g.Date)
                    .ToList();

                if (!grades.Any())
                {
                    throw new Exception("Нет данных для построения графика");
                }

                var seriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Динамика успеваемости",
                        Values = new ChartValues<double>(grades.Select(g => (double)g.GradeValue)),
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 10,
                        DataLabels = true,
                        LabelPoint = point => point.Y.ToString()
                    }
                };

                return seriesCollection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при подготовке данных для графика: {ex.Message}");
            }
        }

        public List<string> GetClassNames()
        {
            return _context.Classes
                .OrderBy(c => c.ClassName)
                .Select(c => c.ClassName)
                .ToList();
        }


        public List<StudentItem> GetStudentsList()
        {
            return _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Select(s => new StudentItem
                {
                    StudentID = s.StudentID,
                    FullName = s.LastName + " " + s.FirstName + " " + (s.MiddleName ?? "")
                })
                .ToList();
        }


        public List<SubjectItem> GetSubjectsList()
        {
            return _context.Subjects
                .OrderBy(s => s.SubjectName)
                .Select(s => new SubjectItem
                {
                    SubjectID = s.SubjectID,
                    SubjectName = s.SubjectName
                })
                .ToList();
        }
    }


    public class StudentItem
    {
        public int StudentID { get; set; }
        public string FullName { get; set; } = string.Empty;
    }


    public class SubjectItem
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }
}