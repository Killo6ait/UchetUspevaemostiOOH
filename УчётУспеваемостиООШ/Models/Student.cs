using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace УчётУспеваемостиООШ.Models
{
    [Table("Students")]
    public class Student
    {
        [Key]
        public int StudentID { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required]
        public int ClassID { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string ParentAddress { get; set; } = string.Empty;


        [ForeignKey("ClassID")]
        public virtual Class? Class { get; set; }

        public virtual ICollection<Grade>? Grades { get; set; }
        public virtual ICollection<Attendance>? Attendances { get; set; }
    }
}