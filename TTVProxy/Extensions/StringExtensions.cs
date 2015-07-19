using System.Text;

namespace P2pProxy.Extensions
{
    static class StringExtensions
    {
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static byte[] GetBytes(this string str, Encoding encoding)
        {
            return encoding.GetBytes(str);   
        }

        public static byte[] GetBytes(this string str)
        {
            return GetBytes(str, Encoding.UTF8);
        }
    }
}