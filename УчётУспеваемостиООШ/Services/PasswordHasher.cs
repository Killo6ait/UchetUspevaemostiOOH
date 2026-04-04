using System;
using System.Security.Cryptography;
using System.Text;

namespace УчётУспеваемостиООШ.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password, out string salt)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            salt = Convert.ToBase64String(saltBytes);
            return HashPasswordWithSalt(password, salt);
        }

        public static string HashPasswordWithSalt(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] combined = Encoding.UTF8.GetBytes(password + salt);
                byte[] hash = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string computedHash = HashPasswordWithSalt(password, storedSalt);
            return computedHash == storedHash;
        }

        public static (string hash, string salt) MigratePlainPassword(string plainPassword)
        {
            string hash = HashPassword(plainPassword, out string salt);
            return (hash, salt);
        }
    }
}