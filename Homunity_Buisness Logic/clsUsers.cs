using Homunity_Data_Access;
using System.Security.Cryptography;
using System.Text;

namespace Homunity_Buisness_Logic
{
    public class clsUsers
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }


        public clsUsers()
        {
            UserID = -1;
            IsActive = true;
            Mode = enMode.AddNew;
        }

        // ================= Register =================
        private bool _AddNewUser()
        {
            if (!ValidateData())
                return false;

            if (clsUsersData.IsPhoneExists(Phone))
                return false;

            string passwordHash = HashPassword(Password);

            int newId = clsUsersData.AddNewUser(
                FirstName,
                LastName,
                Phone,
                passwordHash,
                RoleId,
                IsActive
            );

            if (newId == -1)
                return false;

            UserID = newId;
            Mode = enMode.Update;
            return true;
        }

        // ================= Update Status =================
        private bool _UpdateUser()
        {
            return clsUsersData.UpdateUserStatus(UserID, IsActive);
        }


        // ================= Get Profile =================

        public static clsUsers GetProfile(int userId)
        {
            string firstName = "";
            string lastName = "";
            string phone = "";
            string passwordHash = "";
            int roleId = -1;
            bool isActive = false;

            bool found = clsUsersData.GetUserByID(userId, out firstName, out lastName,
            out phone, out passwordHash, out roleId, out isActive);

            if (!found)
                return null;

            clsUsers user = new clsUsers();
            user.UserID = userId;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Phone = phone;
            user.RoleId = roleId;
            user.IsActive = isActive;
            user.Mode = enMode.Update;

            return user;
        }

        // ================= Save =================

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewUser())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case enMode.Update:
                    return _UpdateUser();
            }
            return false;
        }

        // ================= Login =================

        public static clsUsers Login(string phone, string password)
        {
            int id = -1;
            int roleId = -1;
            string firstName = string.Empty;
            string lastName = string.Empty;
            string passwordHash = string.Empty;
            bool isActive = false;

            // ✅ أضفنا out
            bool found = clsUsersData.GetUserByPhone(
                phone,
                out id,
                out firstName,
                out lastName,
                out passwordHash,
                out roleId,
                out isActive
            );

            if (!found)
                return null;

            if (!isActive)
                return null;

            string inputHash = HashPassword(password);
            if (inputHash != passwordHash)
                return null;

            return new clsUsers
            {
                UserID = id,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                RoleId = roleId,
                IsActive = isActive,
                Mode = enMode.Update
            };
        }
        // ================= Helpers =================

        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(FirstName)) return false;
            if (string.IsNullOrWhiteSpace(LastName)) return false;
            if (string.IsNullOrWhiteSpace(Phone)) return false;
            if (string.IsNullOrWhiteSpace(Password)) return false;
            if (Password.Length < 4) return false;

            return true;
        }

        private static string HashPassword(string password)
        {
            SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}