using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace УчётУспеваемостиООШ.Models
{
    [Table("Subjects")]
    public class Subject
    {
        [Key]
        public int SubjectID { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [Required]
        public int TeacherID { get; set; }

        [Required]
        public byte HoursPerWeek { get; set; }


        [ForeignKey("TeacherID")]
        public virtual Teacher? Teacher { get; set; }

        public virtual ICollection<Grade>? Grades { get; set; }
    }
}