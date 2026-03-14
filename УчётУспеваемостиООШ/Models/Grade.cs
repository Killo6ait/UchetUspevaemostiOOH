using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace УчётУспеваемостиООШ.Models
{
    [Table("Grades")]
    public class Grade
    {
        [Key]
        public int GradeID { get; set; }

        [Required]
        public int StudentID { get; set; }

        [Required]
        public int SubjectID { get; set; }

        [Required]
        [Column("Grade")] // Явно указываем имя колонки в БД
        public byte GradeValue { get; set; } // Переименовано, чтобы не совпадало с именем класса

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string ControlType { get; set; } = string.Empty;

        // Навигационные свойства
        [ForeignKey("StudentID")]
        public virtual Student? Student { get; set; }

        [ForeignKey("SubjectID")]
        public virtual Subject? Subject { get; set; }
    }
}