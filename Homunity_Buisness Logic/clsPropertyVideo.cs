using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
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
            if (PropertyId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(VideoPath))
                return false;

            string extension = Path.GetExtension(VideoPath).ToLower();
            if (string.IsNullOrEmpty(extension))
                extension = Path.GetExtension(VideoPath.Split('/').Last()).ToLower();

            if (!AllowedVideoExtensions.Contains(extension))
                return false;

            // ✅ لو fileSizeBytes = 0 يعني مش موجود — بيعدي
            // لو موجود يتحقق من الحجم
            if (fileSizeBytes > 0)
            {
                long maxSizeBytes = MAX_VIDEO_SIZE_MB * 1024 * 1024;
                if (fileSizeBytes > maxSizeBytes)
                    return false;
            }

            return true;
        }

        // ================= ADD NEW VIDEO =================
        private bool AddNewVideo(long fileSizeBytes, int propertyId, string videoPath,SqlConnection connection, SqlTransaction transaction)
        {
            // نحط القيم في الـ Object عشان Validate تشتغل صح
            this.PropertyId = propertyId;
            this.VideoPath = videoPath;

            if (!Validate(fileSizeBytes))
                return false;

            bool added = clsPropertyVideoData.AddVideo(propertyId, videoPath, connection, transaction);
            if (!added)
                return false;

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


        // ================= Delete By Property ID =================
        public static bool DeleteByPropertyID(int propertyId,SqlConnection connection, SqlTransaction transaction)
        {
            if (propertyId <= 0)
                return false;

            return clsPropertyVideoData.DeleteByPropertyID(propertyId, connection, transaction);
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


        // ================= Save (Add/Update) =================
        public bool Save(SqlConnection connection, SqlTransaction transaction,
                         long fileSizeBytes = 0, int propertyId = 0, string videoPath = "")
        {
            try
            {
                switch (Mode)
                {
                    case enMode.AddNew:
                        return AddNewVideo(fileSizeBytes, propertyId, videoPath, connection, transaction);

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




    }
}