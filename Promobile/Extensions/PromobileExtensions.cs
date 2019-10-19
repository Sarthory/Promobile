using System.Security.Cryptography;
using System.Text;

namespace Promobile.Extensions
{
    public class PromobileExtensions
    {
        public static string ToHash(string password)
        {
            var bytePassword = Encoding.UTF8.GetBytes(password);

            using (var md5 = MD5.Create())
            {
                var byteHashedPassword = md5.ComputeHash(bytePassword);

                var result = new StringBuilder(byteHashedPassword.Length * 2);

                foreach (var t in byteHashedPassword)
                {
                    result.Append(t.ToString("x2"));
                }

                return result.ToString();
            }
        }
    }
}
