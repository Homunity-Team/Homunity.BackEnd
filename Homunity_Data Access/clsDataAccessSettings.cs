using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
 
        // القيمة الحقيقية بتتحدد مرة واحدة وقت الإقلاع من Program.cs (عبر User Secrets/Environment Variables).
        // كل الكلاسات القديمة (clsUsersData, clsPropertiesData, إلخ) تفضل تستخدمها بنفس الصيغة القديمة تمامًا.
        public static class clsDataAccessSettings
        {
            private static string? _connectionString;

            public static string ConnectionString
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_connectionString))
                        throw new InvalidOperationException(
                            "Connection string لم يتم تهيئتها. تأكد من استدعاء clsDataAccessSettings.Initialize() في Program.cs.");
                    return _connectionString;
                }
            }

            public static void Initialize(string connectionString)
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new ArgumentException("Connection string لا يمكن أن يكون فارغًا.", nameof(connectionString));
                _connectionString = connectionString;
            }
        }
    




    /*
   // public Host
   public class clsDataAccessSettings
   {
        public static string ConnectionString = "Server=db37868.public.databaseasp.net; Database=db37868; User Id=db37868; Password=fS-7T4s#zE!5; Encrypt=False; MultipleActiveResultSets=True;";
   }



   // Local Host
   public class clsDataAccessSettings
   {
       public static string ConnectionString = "Server = localhost;Database = Homunity;User Id=sa;Password=123456;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";
   }
         */


}
