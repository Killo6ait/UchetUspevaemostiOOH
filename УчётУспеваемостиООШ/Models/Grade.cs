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
        [Column("Grade")] 
        public byte GradeValue { get; set; } 
        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string ControlType { get; set; } = string.Empty;


        [ForeignKey("StudentID")]
        public virtual Student? Student { get; set; }

        [ForeignKey("SubjectID")]
        public virtual Subject? Subject { get; set; }
    }
}