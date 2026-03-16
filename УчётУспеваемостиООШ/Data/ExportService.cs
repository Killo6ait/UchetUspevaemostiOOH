using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using УчётУспеваемостиООШ.Data;

namespace УчётУспеваемостиООШ.Services
{
    public class ExportService
    {
        private SchoolContext _context;

        public ExportService(SchoolContext context)
        {
            _context = context;
        }

        public void ExportClassGrades(string className, string filePath)
        {
            try
            {
                    var students = _context.Students
                 .Include(s => s.Class)
                 .Include(s => s.Grades)
                     .ThenInclude(g => g.Subject)
                 .Include(s => s.Attendances)
                 .Where(s => s.Class != null && s.Class.ClassName == className)
                 .OrderBy(s => s.LastName)
                 .ThenBy(s => s.FirstName)
                 .ToList();

                if (!students.Any())
                {
                    throw new Exception($"В классе {className} нет учащихся!");
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add($"Ведомость {className}");
                    worksheet.Cell("A1").Value = $"Ведомость успеваемости класса {className}";
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Range("A1:F1").Merge();
                    worksheet.Cell("A3").Value = "№";
                    worksheet.Cell("B3").Value = "ФИО ученика";
                    worksheet.Cell("C3").Value = "Предмет";
                    worksheet.Cell("D3").Value = "Оценка";
                    worksheet.Cell("E3").Value = "Дата";
                    worksheet.Cell("F3").Value = "Тип контроля";
                    var headerRange = worksheet.Range("A3:F3");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    int row = 4;
                    int studentNumber = 1;
                    foreach (var student in students)
                    {
                        if (student.Grades != null && student.Grades.Any())
                        {
                            foreach (var grade in student.Grades.OrderBy(g => g.Date))
                            {
                                worksheet.Cell(row, 1).Value = studentNumber;
                                worksheet.Cell(row, 2).Value = $"{student.LastName} " +
                                    $"{student.FirstName} {student.MiddleName}";
                                worksheet.Cell(row, 3).Value = grade.Subject?.SubjectName;
                                worksheet.Cell(row, 4).Value = grade.GradeValue;
                                worksheet.Cell(row, 5).Value = grade.Date.ToString("dd.MM.yyyy");
                                worksheet.Cell(row, 6).Value = grade.ControlType;
                                row++;
                            }
                            studentNumber++;
                        }
                    }
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при экспорте данных: {ex.Message}");
            }
        }
    }
}