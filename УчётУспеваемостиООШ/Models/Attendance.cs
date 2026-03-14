using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace УчётУспеваемостиООШ.Models
{
    [Table("Attendance")]
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }

        [Required]
        public int StudentID { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty; // 'Присутствовал', 'Отсутствовал', 'Уважительная причина'

        // Навигационное свойство
        [ForeignKey("StudentID")]
        public virtual Student? Student { get; set; }
    }
}