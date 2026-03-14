using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace УчётУспеваемостиООШ.Models
{
    [Table("Classes")]
    public class Class
    {
        [Key]
        public int ClassID { get; set; }

        [Required]
        [MaxLength(10)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public byte GradeLevel { get; set; }

        [Required]
        [Column(TypeName = "nchar(1)")]
        public string ClassLetter { get; set; } = string.Empty;

        // Навигационные свойства
        public virtual ICollection<Student>? Students { get; set; }
    }
}