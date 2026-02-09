using Homunity_Data_Access;
using System.Text.Json.Serialization;

namespace Homunity_Buisness_Logic
{
    public class clsPropertyVideo
    {
        public enum enMode { AddNew = 0, Update = 1 }

        [JsonIgnore]
        public enMode Mode { get; set; } = enMode.AddNew;

        public int VideoId { get; private set; }
        public int PropertyId { get; set; }
        public string VideoPath { get; set; }
        public DateTime CreatedAt { get; private set; }

        // Business Rules
        private const long MAX_VIDEO_SIZE_MB = 30;


        private static readonly string[] AllowedVideoExtensions =
        {
            ".mp4", ".webm"
        };

        // Constructor
        public clsPropertyVideo()
        {
            VideoId = -1;
            PropertyId = -1;
            VideoPath = string.Empty;
            CreatedAt = DateTime.Now;
            Mode = enMode.AddNew;
        }



        // ================= VALIDATION =================
        private bool Validate(long fileSizeBytes)
        {
            // 1. تحقق من PropertyId
            if (PropertyId <= 0)
                return false;

            // 2. تحقق من وجود مسار الفيديو
            if (string.IsNullOrWhiteSpace(VideoPath))
                return false;

            // 3. تحقق من امتداد الملف
            string extension = Path.GetExtension(VideoPath).ToLower();
            if (string.IsNullOrEmpty(extension))
            {
                // إذا المسار مش واضح، نحاول من الاسم
                extension = Path.GetExtension(VideoPath.Split('/').Last()).ToLower();
            }

            if (!AllowedVideoExtensions.Contains(extension))
                return false;

            // 4. تحقق من حجم الملف
            long maxSizeBytes = MAX_VIDEO_SIZE_MB * 1024 * 1024;
            if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
                return false;



            // 5. تحقق من وجود فيديو آخر للعقار (فقط عند الإضافة)
            if (Mode == enMode.AddNew)
            {
                // تحقق إذا العقار عنده فيديو بالفعل
                var existingVideo = GetVideoByPropertyID(PropertyId);
                if (existingVideo != null)
                    return false; // العقار عنده فيديو بالفعل
            }

            return true;
        }



        // ================= ADD NEW VIDEO =================
        private bool AddNewVideo(long fileSizeBytes)
        {
            // Validate قبل الإضافة
            if (!Validate(fileSizeBytes))
                return false;

            // Add to database
            int newId = clsPropertyVideoData.AddNewVideo(PropertyId, VideoPath);
            if (newId == -1)
                return false;

            // Update properties
            VideoId = newId;
            Mode = enMode.Update;
            return true;
        }



        // ================= UPDATE VIDEO =================
        private bool UpdateVideo(long fileSizeBytes)
        {
            // Validate قبل التحديث
            if (!Validate(fileSizeBytes))
                return false;

            // Update in database
            return clsPropertyVideoData.UpdateVideo(VideoId, VideoPath);
        }



        // ================= Save (Add/Update) =================
        public bool Save(long fileSizeBytes = 0)
        {
            try
            {
                switch (Mode)
                {
                    case enMode.AddNew:
                        return AddNewVideo(fileSizeBytes);

                    case enMode.Update:
                        return UpdateVideo(fileSizeBytes);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving video: {ex.Message}");
                return false;
            }
        }



        // ================= DELETE =================
        public static bool Delete(int videoId)
        {
            if (videoId <= 0)
                return false;

            return clsPropertyVideoData.DeleteVideo(videoId);
        }




        // ================= FIND BY ID =================
        public static clsPropertyVideo FindByID(int videoId)
        {
            if (videoId <= 0)
                return null;

            int propertyId = 0;
            string videoPath = string.Empty;
            DateTime createdAt = DateTime.Now;

            // استخدام ref لأن الدالة بتعدل القيم
            bool isFound = clsPropertyVideoData.GetVideoByID(
                videoId, ref propertyId, ref videoPath, ref createdAt);

            if (!isFound)
                return null;

            return new clsPropertyVideo
            {
                VideoId = videoId,
                PropertyId = propertyId,
                VideoPath = videoPath,
                CreatedAt = createdAt,
                Mode = enMode.Update
            };
        }




        // ================= GET Video BY PROPERTY =================
        public static clsPropertyVideo GetVideoByPropertyID(int propertyId)
        {
            if (propertyId <= 0)
                return null;

            int videoId = 0;
            string videoPath = string.Empty;
            DateTime createdAt = DateTime.Now;

            bool isFound = clsPropertyVideoData.GetVideoByPropertyID(
                propertyId, ref videoId, ref videoPath, ref createdAt);

            if (!isFound)
                return null;

            return new clsPropertyVideo
            {
                VideoId = videoId,
                PropertyId = propertyId,
                VideoPath = videoPath,
                CreatedAt = createdAt,
                Mode = enMode.Update
            };
        }


    }
}