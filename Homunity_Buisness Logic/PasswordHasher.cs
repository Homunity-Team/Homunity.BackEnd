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
 
        // Sprint 10: BCrypt بدل SHA256 الخام. بيدعم الترحيل التلقائي من الباسوردات القديمة.
        public static class PasswordHasher
        {
            // ================= الطريقة الجديدة (BCrypt) ================= //
            public static string Hash(string password)
            {
                return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            }

            // بيتحقق من الباسورد ضد أي نوع Hash (BCrypt أو SHA256 قديم)
            public static bool Verify(string password, string storedHash)
            {
                if (IsBCryptHash(storedHash))
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }

                // Fallback: باسورد قديم اتخزن بـSHA256 من قبل الترحيل
                return LegacyHashSha256(password) == storedHash;
            }

            // بيرجع true لو الباسورد محتاج ترحيل للـHash الجديد (يعني لسه SHA256 قديم)
            public static bool NeedsRehash(string storedHash)
            {
                return !IsBCryptHash(storedHash);
            }

            private static bool IsBCryptHash(string hash)
            {
                // BCrypt hashes بتبدأ دايمًا بـ$2a$ أو $2b$ أو $2y$
                return !string.IsNullOrEmpty(hash) &&
                       (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));
            }

            // ================= الطريقة القديمة (SHA256) — للـFallback بس، ما بتتستخدمش لباسوردات جديدة ================= //
            private static string LegacyHashSha256(string password)
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    
}
