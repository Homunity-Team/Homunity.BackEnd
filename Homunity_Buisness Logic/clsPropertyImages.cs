
using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsPropertyImages
    {
        public enum enMode { AddNew = 0, Update = 1 }

        [JsonIgnore]
        public enMode Mode { get; set; } = enMode.AddNew;

        public int ImageId { get; private set; }
        public int PropertyId { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; private set; }

        // Business Rules
        private const int MAX_IMAGES = 6;
        private const long MAX_IMAGE_SIZE_MB = 2;

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
            long maxSizeBytes = MAX_IMAGE_SIZE_MB * 1024 * 1024;
            if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
                return false;

            // 5. تحقق من عدد الصور (فقط عند الإضافة)
            if (Mode == enMode.AddNew)
            {
                int currentCount = GetImagesCount(PropertyId);
                if (currentCount >= MAX_IMAGES)
                    return false;
            }

            return true;
        }

        // ================= ADD NEW IMAGE =================
        private bool AddNewImage(long fileSizeBytes)
        {
            // Validate قبل الإضافة
            if (!Validate(fileSizeBytes))
                return false;

            int newId = clsPropertyImagesData.AddNewImage(PropertyId, ImagePath);
            if (newId == -1)
                return false;

            ImageId = newId;
            Mode = enMode.Update;
            return true;
        }

        // ================= UPDATE IMAGE =================
        private bool UpdateImage(long fileSizeBytes)
        {
            // Validate قبل التحديث
            if (!Validate(fileSizeBytes))
                return false;

            // Update in database
            return clsPropertyImagesData.UpdateImage(ImageId, ImagePath);
        }

        // ================= Save (Add/Update) =================
        public bool Save(long fileSizeBytes = 0)
        {
            try
            {
                switch (Mode)
                {
                    case enMode.AddNew:
                        return AddNewImage(fileSizeBytes);

                    case enMode.Update:
                        return UpdateImage(fileSizeBytes);

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

        // ================= DELETE =================
        public static bool Delete(int imageId)
        {
            if (imageId <= 0)
                return false;

            return clsPropertyImagesData.DeleteImage(imageId);
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
    }
}

