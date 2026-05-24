
using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace Homunity_Buisness_Logic
{
    public class clsPropertyImages
    {
        public enum enMode { AddNew = 0, Update = 1 }

        [JsonIgnore]
        public enMode Mode { get; set; } = enMode.AddNew;

        public int ImageId { get;  set; }
        public int PropertyId { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get;  set; }

        // Business Rules
        private const int MAX_IMAGES = 6;
        private const long MAX_IMAGE_SIZE_BYTES = 2 * 1024 * 1024;
        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        // Constructor
        public clsPropertyImages()
        {
            ImageId = -1;
            PropertyId = -1;
            ImagePath = string.Empty;
            CreatedAt = DateTime.Now;
            Mode = enMode.AddNew;
        }

        // ================= VALIDATION =================
        private bool Validate(long fileSizeBytes)
        {
            // 1. تحقق من PropertyId
            if (PropertyId <= 0)
                return false;

            // 2. تحقق من وجود مسار الصورة
            if (string.IsNullOrWhiteSpace(ImagePath))
                return false;

            // 3. تحقق من امتداد الملف (سواء من المسار أو اسم الملف)
            string extension = Path.GetExtension(ImagePath).ToLower();
            if (string.IsNullOrEmpty(extension))
            {
                // إذا المسار مش واضح، نحاول من الاسم
                extension = Path.GetExtension(ImagePath.Split('/').Last()).ToLower();
            }

            if (!AllowedExtensions.Contains(extension))
                return false;

            // 4. تحقق من حجم الملف
            long maxSizeBytes = MAX_IMAGE_SIZE_BYTES * 1024 * 1024;
            if (fileSizeBytes <= 0 || fileSizeBytes > MAX_IMAGE_SIZE_BYTES)
                return false;
 
            return true;
        }

        
        // ================= ADD NEW IMAGE =================
        private bool _AddNewImage(long fileSizeBytes, int propertyId, string imagePath,
            SqlConnection connection, SqlTransaction transaction)
        {
            // ✅ صح
            this.PropertyId = propertyId;
            this.ImagePath = imagePath;

            if (!Validate(fileSizeBytes))
                return false;

            int newId = clsPropertyImagesData.AddImage(propertyId, imagePath, connection, transaction);
            if (newId == -1)
                return false;

            ImageId = newId;
            Mode = enMode.Update;
            return true;
        }
        
        
        // ================= UPDATE IMAGE =================
        private bool _UpdateImage(long fileSizeBytes)
        {
            // Validate قبل التحديث
            if (!Validate(fileSizeBytes))
                return false;

            // Update in database
            return clsPropertyImagesData.UpdateImage(ImageId, ImagePath);
        }


        // ================= Save (Add/Update) =================
        public bool Save(SqlConnection connection, SqlTransaction transaction, long fileSizeBytes = 0, int propertyId = 0, string imagePath = "")
        {
            try
            {
                switch (Mode) 
                {
                    case enMode.AddNew:
                        return _AddNewImage(fileSizeBytes, propertyId, imagePath,  connection, transaction);

                    case enMode.Update:
                        return _UpdateImage(fileSizeBytes);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                return false;
            }
        }
 
        
        // ================= FIND BY ID =================
        public static clsPropertyImages FindByImageID(int imageId)
        {
            if (imageId <= 0)
                return null;

            int propertyId = 0;
            string imagePath = string.Empty;
            DateTime createdAt = DateTime.Now;

            // استخدام ref لأن الدالة بتعدل القيم
            bool isFound = clsPropertyImagesData.GetImageByID(
                imageId, ref propertyId, ref imagePath, ref createdAt);

            if (!isFound)
                return null;

            return new clsPropertyImages
            {
                ImageId = imageId,
                PropertyId = propertyId,
                ImagePath = imagePath,
                CreatedAt = createdAt,
                Mode = enMode.Update
            };
        }

       


        // ================= GET IMAGES COUNT =================
        public static int GetImagesCount(int propertyId)
        {
            if (propertyId <= 0)
                return 0;

            return clsPropertyImagesData.GetImagesCountByPropertyID(propertyId);
        }



        public static List<clsPropertyImages> GetImagesByPropertyID(int propertyId)
        {
            if (propertyId <= 0)
                return new List<clsPropertyImages>();

            var dt = clsPropertyImagesData.GetImagesByPropertyID(propertyId);
            var list = new List<clsPropertyImages>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new clsPropertyImages
                {
                    ImageId = (int)row["ImageId"],      // ✅ أضفنا ImageId
                    PropertyId = (int)row["PropertyId"],
                    ImagePath = row["ImagePath"].ToString(),
                    CreatedAt = (DateTime)row["CreatedAt"],
                    Mode = enMode.Update
                });
            }

            return list;
        }



        public static clsPropertyImages GetFirstImageByPropertyID(int propertyId)
        {
            if (propertyId <= 0)
                return null;

            var images = GetImagesByPropertyID(propertyId);

            return images.Count == 0 ? null : images[0];
        }
 

        
        public static bool Delete(int imageId, int propertyId, SqlConnection connection, SqlTransaction transaction)
        {
            if (imageId <= 0 || propertyId <= 0)
                return false;

            // ✅ شلنا FindByImageID من هنا — الـ Validation بتتعمل بره الـ Transaction
            return clsPropertyImagesData.Delete(imageId, propertyId, connection, transaction);
        }

    }
}

