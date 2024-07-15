using System.Security.Cryptography;
using System.Text;

namespace ProyectoCanvas.Services.Utilities
{
    public class PasswordHelper
    {
        public static byte[] CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] storedHash)
        {
            using (var hmac = new HMACSHA512())
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }
    }
}
