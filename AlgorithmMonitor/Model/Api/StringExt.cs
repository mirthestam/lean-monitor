using System.Security.Cryptography;
using System.Text;

namespace Monitor.Model.Api
{
    public static class StringExt
    {
        public static string ToSHA256(this string data)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(data), 0, Encoding.UTF8.GetByteCount(data));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}