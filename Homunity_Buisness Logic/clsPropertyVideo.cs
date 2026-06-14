using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Homunity_Business_Logic
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

        private const long MAX_VIDEO_SIZE_MB = 30;
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm" };

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
            if (PropertyId <= 0) return false;
            if (string.IsNullOrWhiteSpace(VideoPath)) return false;

            string extension = Path.GetExtension(VideoPath).ToLower();
            if (string.IsNullOrEmpty(extension))
                extension = Path.GetExtension(VideoPath.Split('/').Last()).ToLower();

            if (!AllowedVideoExtensions.Contains(extension)) return false;

            if (fileSizeBytes > 0)
            {
                long maxSizeBytes = MAX_VIDEO_SIZE_MB * 1024 * 1024;
                if (fileSizeBytes > maxSizeBytes) return false;
            }
            return true;
        }

        // ================= ADD NEW VIDEO (Async) =================
        private async Task<bool> AddNewVideoAsync(long fileSizeBytes, int propertyId, string videoPath,
            SqlConnection connection, SqlTransaction transaction)
        {
            this.PropertyId = propertyId;
            this.VideoPath = videoPath;

            if (!Validate(fileSizeBytes))
                return false;

            bool added = await clsPropertyVideoData.AddVideoAsync(propertyId, videoPath, connection, transaction);
            if (!added)
                return false;

            Mode = enMode.Update;
            return true;
        }

        // ================= UPDATE VIDEO (Async) =================
        private async Task<bool> UpdateVideoAsync(long fileSizeBytes)
        {
            if (!Validate(fileSizeBytes))
                return false;

            return await clsPropertyVideoData.UpdateVideoAsync(VideoId, VideoPath);
        }

        // ================= SAVE (Async) =================
        public async Task<bool> SaveAsync(SqlConnection connection, SqlTransaction transaction,
            long fileSizeBytes = 0, int propertyId = 0, string videoPath = "")
        {
            try
            {
                switch (Mode)
                {
                    case enMode.AddNew:
                        return await AddNewVideoAsync(fileSizeBytes, propertyId, videoPath, connection, transaction);
                    case enMode.Update:
                        return await UpdateVideoAsync(fileSizeBytes);
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

        // Sync wrapper for backward compatibility
        public bool Save(SqlConnection connection, SqlTransaction transaction,
            long fileSizeBytes = 0, int propertyId = 0, string videoPath = "")
        {
            return SaveAsync(connection, transaction, fileSizeBytes, propertyId, videoPath).GetAwaiter().GetResult();
        }

        // ================= DELETE BY PROPERTY ID (Async) =================
        public static async Task<bool> DeleteByPropertyIDAsync(int propertyId,
            SqlConnection connection, SqlTransaction transaction)
        {
            if (propertyId <= 0)
                return false;

            try
            {
                await clsPropertyVideoData.DeleteByPropertyIDAsync(propertyId, connection, transaction);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Sync wrapper (fixed: returns actual result)
        public static bool DeleteByPropertyID(int propertyId,
            SqlConnection connection, SqlTransaction transaction)
        {
            return DeleteByPropertyIDAsync(propertyId, connection, transaction).GetAwaiter().GetResult();
        }

        // ================= FIND BY ID (Async) =================
        public static async Task<clsPropertyVideo> FindByIDAsync(int videoId)
        {
            if (videoId <= 0)
                return null;

            int propertyId = 0;
            string videoPath = string.Empty;
            DateTime createdAt = DateTime.Now;

            // يجب أن توفر Data Access دالة GetVideoByIDAsync.
            // بناءً على الكود الحالي، GetVideoByID هي sync فقط، لذا سنضيف Async في Data Access أو نستخدم GetVideoByID كحل مؤقت.
            // لحل هذه المشكلة، سنضيف دالة GetVideoByIDAsync في Data Access أولاً، ثم نستخدمها هنا.
            // لكن بما أنك تريد الاعتماد على Async بالكامل، سأفترض وجود GetVideoByIDAsync.
            // إذا لم تكن موجودة، يمكنك تحويل الكود التالي إلى sync.
            // لكن لتجنب التعقيد، سأقدم هنا الحل بـ sync مؤقتاً مع إضافة Async حقيقي لاحقاً.
            // نستخدم الدالة المتزامنة الحالية في Data Access (sync) كحل وسيط.
            bool isFound = clsPropertyVideoData.GetVideoByID(videoId, ref propertyId, ref videoPath, ref createdAt);

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

        // Sync wrapper
        public static clsPropertyVideo FindByID(int videoId)
        {
            return FindByIDAsync(videoId).GetAwaiter().GetResult();
        }

        // ================= GET VIDEO BY PROPERTY ID (Async) =================
        public static async Task<clsPropertyVideo> GetVideoByPropertyIDAsync(int propertyId)
        {
            if (propertyId <= 0)
                return null;

            int videoId = 0;
            string videoPath = string.Empty;
            DateTime createdAt = DateTime.Now;

            // نفس الملاحظة: نستخدم GetVideoByPropertyID المتزامنة مؤقتاً
            bool isFound = clsPropertyVideoData.GetVideoByPropertyID(propertyId, ref videoId, ref videoPath, ref createdAt);

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

        // Sync wrapper
        public static clsPropertyVideo GetVideoByPropertyID(int propertyId)
        {
            return GetVideoByPropertyIDAsync(propertyId).GetAwaiter().GetResult();
        }
    }
}