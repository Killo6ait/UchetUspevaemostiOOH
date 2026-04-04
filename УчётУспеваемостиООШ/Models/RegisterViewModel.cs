using System.ComponentModel.DataAnnotations;

namespace УчётУспеваемостиООШ.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [MinLength(3, ErrorMessage = "Логин должен содержать минимум 3 символа")]
        [MaxLength(50, ErrorMessage = "Логин не может превышать 50 символов")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите роль")]
        public string Role { get; set; } = string.Empty;

        public int? TeacherID { get; set; }
    }
}