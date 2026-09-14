using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // مكان مشترك للـHashing — استخدمه UsersService وAuthService، منعًا للتكرار.
    // ملاحظة: SHA256 بدون Salt — ضعف أمني موجود من قبل Sprint 3، مؤجل التعامل معه.
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
