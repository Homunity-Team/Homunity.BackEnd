using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Homunity_Buisness_Logic
{
    public class PropertyOrchestratorService
    {
    
            // ================================================
            // CREATE FULL PROPERTY — async optimized
            // ================================================
            public static async Task<int> CreateFullPropertyAsync(CreateFullPropertyDTO dto)
            {
                // ── 1. Validate Images/Sizes match BEFORE any DB call ──
                if (dto.Images != null && dto.ImageSizes != null
                    && dto.Images.Count != dto.ImageSizes.Count)
                {
                    Console.WriteLine("CreateFullProperty: Images.Count != ImageSizes.Count");
                    return -1;
                }

                // ── 2. Batch validate services in ONE query (بدل N queries) ──
                if (dto.Services != null && dto.Services.Count > 0)
                {
                    var validIds = await clsPropertyServicesData.GetValidServiceIdsAsync(dto.Services);
                    var invalid = dto.Services.FirstOrDefault(id => !validIds.Contains(id));
                    if (invalid != 0)
                    {
                        Console.WriteLine($"CreateFullProperty: Invalid service {invalid}");
                        return -1;
                    }
                }

                await using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                await connection.OpenAsync();
                await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

                try
                {
                    // ── 3. AddLocation ──
                    int locationId = await _AddLocationAsync(connection, transaction,
                        city: string.IsNullOrWhiteSpace(dto.City) ? "Default City" : dto.City,
                        area: string.IsNullOrWhiteSpace(dto.Area) ? "Default Area" : dto.Area,
                        street: dto.Street ?? "",
                        latitude: dto.Latitude != 0 ? (double?)dto.Latitude : null,
                        longitude: dto.Longitude != 0 ? (double?)dto.Longitude : null);

                    if (locationId == -1) throw new Exception("Failed to create location");

                    // ── 4. AddProperty ──
                    int propertyID = await _AddPropertyAsync(connection, transaction, dto, locationId);
                    if (propertyID == -1) throw new Exception("Failed to add property");

                    // ── 5. FullAddress + University (single UPDATE if both exist) ──
                    await _UpdatePropertyMetaAsync(connection, transaction, propertyID, dto.Address, dto.UniversityId);

                    // ── 6. Bulk insert images ──
                    if (dto.Images != null && dto.Images.Count > 0)
                    {
                        for (int i = 0; i < dto.Images.Count; i++)
                        {
                            long size = (dto.ImageSizes != null && i < dto.ImageSizes.Count)
                                        ? dto.ImageSizes[i] : 0;

                            if (!_ValidateImage(dto.Images[i], size))
                                throw new Exception($"Invalid image: {dto.Images[i]}");

                            int imgId = await clsPropertyImagesData.AddImageAsync(
                                propertyID, dto.Images[i], connection, transaction);

                            if (imgId == -1) throw new Exception($"Failed to add image: {dto.Images[i]}");
                        }
                    }

                    // ── 7. Video (non-fatal) ──
                    if (!string.IsNullOrEmpty(dto.VideoUrl))
                    {
                        try
                        {
                            await clsPropertyVideoData.AddVideoAsync(
                                propertyID, dto.VideoUrl, connection, transaction);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Video save failed — {ex.Message}");
                            // non-fatal: نكمل
                        }
                    }

                    // ── 8. Bulk insert services (single INSERT) ──
                    if (dto.Services != null && dto.Services.Count > 0)
                        await clsPropertyServicesData.BulkAddServicesAsync(
                            propertyID, dto.Services, connection, transaction);

                    await transaction.CommitAsync();
                    return propertyID;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"CreateFullProperty error: {ex.Message}");
                    return -1;
                }
            }

            // Sync wrapper للـ Controller القديم
            public static int CreateFullProperty(CreateFullPropertyDTO dto)
                => CreateFullPropertyAsync(dto).GetAwaiter().GetResult();


            // ================================================
            // UPDATE FULL PROPERTY — async optimized
            // ================================================
            public static async Task<bool> UpdateFullPropertyAsync(UpdateFullPropertyDTO dto)
            {
                // ── 1. Get property (single query) ──
                var property = clsProperties.FindByID(dto.PropertyID);
                if (property == null) return false;

                // ── 2. Validate images count BEFORE DB ──
                if (dto.NewImages != null && dto.NewImageSizes != null
                    && dto.NewImages.Count != dto.NewImageSizes.Count)
                {
                    Console.WriteLine("UpdateFullProperty: NewImages.Count != NewImageSizes.Count");
                    return false;
                }

                // ── 3. Batch validate services ──
                if (dto.Services != null && dto.Services.Count > 0)
                {
                    var validIds = await clsPropertyServicesData.GetValidServiceIdsAsync(dto.Services);
                    var invalid = dto.Services.FirstOrDefault(id => !validIds.Contains(id));
                    if (invalid != 0) return false;
                }

                await using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                await connection.OpenAsync();
                await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

                try
                {
                    // ── 4. Update Location ──
                    if (dto.Latitude != 0 && dto.Longitude != 0)
                        await _UpdateLocationAsync(connection, transaction,
                            property.LocationID, dto.Address ?? "", dto.Latitude, dto.Longitude);

                    // ── 5. Update FullAddress + University ──
                    await _UpdatePropertyMetaAsync(connection, transaction,
                        dto.PropertyID, dto.Address, dto.UniversityId);

                    // ── 6. Update Property basic data ──
                    property.Title = dto.Title;
                    property.Description = dto.Description;
                    property.Price = dto.Price;
                    property.Rooms = dto.Rooms;
                    property.PropertyType = dto.PropertyType;
                    property.Mode = clsProperties.enMode.Update;

                    if (!property.Save(connection, transaction))
                        throw new Exception("Failed to update property");

                    // ── 7. Delete images ──
                    if (dto.ImageIdsToDelete != null && dto.ImageIdsToDelete.Count > 0)
                    {
                        foreach (var imageId in dto.ImageIdsToDelete)
                        {
                            bool deleted = await clsPropertyImagesData.DeleteAsync(
                                imageId, dto.PropertyID, connection, transaction);
                            if (!deleted) throw new Exception($"Failed to delete image {imageId}");
                        }
                    }

                    // ── 8. Add new images ──
                    if (dto.NewImages != null && dto.NewImages.Count > 0)
                    {
                        for (int i = 0; i < dto.NewImages.Count; i++)
                        {
                            long size = (dto.NewImageSizes != null && i < dto.NewImageSizes.Count)
                                        ? dto.NewImageSizes[i] : 0;

                            if (!_ValidateImage(dto.NewImages[i], size))
                                throw new Exception($"Invalid image: {dto.NewImages[i]}");

                            int imgId = await clsPropertyImagesData.AddImageAsync(
                                dto.PropertyID, dto.NewImages[i], connection, transaction);

                            if (imgId == -1) throw new Exception($"Failed to add image: {dto.NewImages[i]}");
                        }
                    }

                    // ── 9. Handle Video ──
                    if (dto.DeleteVideo || !string.IsNullOrEmpty(dto.NewVideoUrl))
                        await clsPropertyVideoData.DeleteByPropertyIDAsync(
                            dto.PropertyID, connection, transaction);

                    if (!string.IsNullOrEmpty(dto.NewVideoUrl))
                    {
                        try
                        {
                            await clsPropertyVideoData.AddVideoAsync(
                                dto.PropertyID, dto.NewVideoUrl, connection, transaction);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Video update failed — {ex.Message}");
                        }
                    }

                    // ── 10. Update Services — delete + bulk insert ──
                    if (dto.Services != null)
                    {
                        await clsPropertyServicesData.DeleteAllByPropertyIDAsync(
                            dto.PropertyID, connection, transaction);

                        if (dto.Services.Count > 0)
                            await clsPropertyServicesData.BulkAddServicesAsync(
                                dto.PropertyID, dto.Services, connection, transaction);
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"UpdateFullProperty error: {ex.Message}");
                    return false;
                }
            }

            public static bool UpdateFullProperty(UpdateFullPropertyDTO dto)
                => UpdateFullPropertyAsync(dto).GetAwaiter().GetResult();


            // ================================================
            // PRIVATE HELPERS
            // ================================================

            private static bool _ValidateImage(string imagePath, long fileSizeBytes)
            {
                if (string.IsNullOrWhiteSpace(imagePath)) return false;

                var ext = Path.GetExtension(imagePath.Split('?')[0]).ToLower();
                if (string.IsNullOrEmpty(ext))
                    ext = Path.GetExtension(imagePath.Split('/').Last()).ToLower();

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext)) return false;

                const long MAX = 2L * 1024 * 1024; // 2MB
                if (fileSizeBytes > 0 && fileSizeBytes > MAX) return false;

                return true;
            }

            private static async Task<int> _AddLocationAsync(SqlConnection conn, SqlTransaction tx,
                string city, string area, string street, double? latitude, double? longitude)
            {
                const string sql = @"
                INSERT INTO Location (City, Area, Street, Latitude, Longitude)
                OUTPUT INSERTED.LocationId
                VALUES (@City, @Area, @Street, @Latitude, @Longitude)";

                using var cmd = new SqlCommand(sql, conn, tx);
                cmd.Parameters.Add("@City", SqlDbType.NVarChar, 20).Value = city;
                cmd.Parameters.Add("@Area", SqlDbType.NVarChar, 50).Value = area;
                cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 50).Value = (object)street ?? DBNull.Value;
                cmd.Parameters.Add("@Latitude", SqlDbType.Float).Value = (object)latitude ?? DBNull.Value;
                cmd.Parameters.Add("@Longitude", SqlDbType.Float).Value = (object)longitude ?? DBNull.Value;

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : -1;
            }

            private static async Task<int> _AddPropertyAsync(SqlConnection conn, SqlTransaction tx,
                CreateFullPropertyDTO dto, int locationId)
            {
                const string sql = @"
                INSERT INTO Properties
                    (OwnerID, Title, Description, Price, Rooms,
                     PropertyType, LocationID, StatusID, RejectReason, CreatedAt)
                OUTPUT INSERTED.PropertyID
                VALUES
                    (@OwnerID, @Title, @Description, @Price, @Rooms,
                     @PropertyType, @LocationID, 1, NULL, GETDATE())";

                using var cmd = new SqlCommand(sql, conn, tx);
                cmd.Parameters.Add("@OwnerID", SqlDbType.Int).Value = dto.OwnerID;
                cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 150).Value = dto.Title;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = dto.Description ?? "";
                cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = dto.Price;
                cmd.Parameters["@Price"].Precision = 10; cmd.Parameters["@Price"].Scale = 2;
                cmd.Parameters.Add("@Rooms", SqlDbType.Int).Value = dto.Rooms;
                cmd.Parameters.Add("@PropertyType", SqlDbType.VarChar, 20).Value = dto.PropertyType;
                cmd.Parameters.Add("@LocationID", SqlDbType.Int).Value = locationId;

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : -1;
            }

            // ✅ Single UPDATE لـ FullAddress + UniversityId بدل 2 queries
            private static async Task _UpdatePropertyMetaAsync(SqlConnection conn, SqlTransaction tx,
                int propertyId, string address, int universityId)
            {
                bool hasAddress = !string.IsNullOrWhiteSpace(address);
                bool hasUniversity = universityId > 0;

                if (!hasAddress && !hasUniversity) return;

                string sql;
                if (hasAddress && hasUniversity)
                    sql = "UPDATE Properties SET FullAddress=@A, UniversityId=@U WHERE PropertyId=@P";
                else if (hasAddress)
                    sql = "UPDATE Properties SET FullAddress=@A WHERE PropertyId=@P";
                else
                    sql = "UPDATE Properties SET UniversityId=@U WHERE PropertyId=@P";

                using var cmd = new SqlCommand(sql, conn, tx);
                cmd.Parameters.Add("@P", SqlDbType.Int).Value = propertyId;
                if (hasAddress)
                    cmd.Parameters.Add("@A", SqlDbType.NVarChar, 300).Value = address;
                if (hasUniversity)
                    cmd.Parameters.Add("@U", SqlDbType.Int).Value = universityId;

                await cmd.ExecuteNonQueryAsync();
            }

            private static async Task _UpdateLocationAsync(SqlConnection conn, SqlTransaction tx,
                int locationId, string address, double lat, double lon)
            {
                const string sql = @"
                UPDATE Location
                SET Street=@Address, Latitude=@Lat, Longitude=@Lon
                WHERE LocationId=@LocationId";

                using var cmd = new SqlCommand(sql, conn, tx);
                cmd.Parameters.Add("@LocationId", SqlDbType.Int).Value = locationId;
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 50).Value = (object)address ?? DBNull.Value;
                cmd.Parameters.Add("@Lat", SqlDbType.Float).Value = lat;
                cmd.Parameters.Add("@Lon", SqlDbType.Float).Value = lon;
                await cmd.ExecuteNonQueryAsync();
            }
        




        //
        /*
        public static int CreateFullProperty(CreateFullPropertyDTO dto)
        {
            // ── Validate Services ──
            if (dto.Services != null)
            {
                foreach (var serviceId in dto.Services)
                    if (!clsServices.IsValidService(serviceId)) return -1;
            }

            // ── Validate Images/Sizes match ──
            if (dto.Images != null && dto.ImageSizes != null &&
                dto.Images.Count != dto.ImageSizes.Count)
            {
                Console.WriteLine("CreateFullProperty: Images count != ImageSizes count");
                return -1;
            }

            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // ── 1. Create Location (same connection + transaction) ──
                int locationId = _AddLocation(
                    connection, transaction,
                    city: !string.IsNullOrWhiteSpace(dto.City) ? dto.City : "Default City",
                    area: !string.IsNullOrWhiteSpace(dto.Area) ? dto.Area : "Default Area",
                    street: dto.Street ?? "",
                    latitude: dto.Latitude != 0 ? (double?)dto.Latitude : null,
                    longitude: dto.Longitude != 0 ? (double?)dto.Longitude : null
                );

                if (locationId == -1)
                    throw new Exception("Failed to create location");

                // ── 2. Create Property ──
                var property = new clsProperties
                {
                    OwnerID = dto.OwnerID,
                    Title = dto.Title,
                    Description = dto.Description,
                    Price = dto.Price,
                    Rooms = dto.Rooms,
                    PropertyType = dto.PropertyType,
                    LocationID = locationId,
                    PropertyStatusID = 1,
                    Mode = clsProperties.enMode.AddNew
                };

                if (!property.Save(connection, transaction))
                    throw new Exception("Failed to add property");

                int propertyID = property.PropertyID;

                // ── 3. Save FullAddress + University (same transaction) ──
                if (!string.IsNullOrWhiteSpace(dto.Address))
                    _UpdatePropertyFullAddress(connection, transaction, propertyID, dto.Address);

                if (dto.UniversityId > 0)
                    _UpdatePropertyUniversity(connection, transaction, propertyID, dto.UniversityId);

                // ── 4. Save Images ──
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    for (int i = 0; i < dto.Images.Count; i++)
                    {
                        long size = (dto.ImageSizes != null && i < dto.ImageSizes.Count)
                                    ? dto.ImageSizes[i] : 0;

                        var img = new clsPropertyImages();
                        if (!img.Save(connection, transaction,
                                fileSizeBytes: size,
                                propertyId: propertyID,
                                imagePath: dto.Images[i]))
                            throw new Exception($"Failed to add image: {dto.Images[i]}");
                    }
                }

                // ── 5. Save Video (optional — non-fatal) ──
                if (!string.IsNullOrEmpty(dto.VideoUrl))
                {
                    var video = new clsPropertyVideo();
                    bool videoSaved = video.Save(connection, transaction,
                        fileSizeBytes: dto.VideoSize,
                        propertyId: propertyID,
                        videoPath: dto.VideoUrl);

                    if (!videoSaved)
                        Console.WriteLine($"Warning: Video not saved for property {propertyID}");
                }

                // ── 6. Save Services ──
                if (dto.Services != null)
                {
                    foreach (var serviceID in dto.Services)
                    {
                        var svc = new clsPropertyServices();
                        if (!svc.Save(connection, transaction,
                                propertyId: propertyID,
                                serviceId: serviceID))
                            throw new Exception($"Failed to add service: {serviceID}");
                    }
                }

                transaction.Commit();
                return propertyID;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error in CreateFullProperty: {ex.Message}");
                return -1;
            }
        }


        // =====================================================
        // UPDATE FULL PROPERTY
        // نفس المراحل
        // =====================================================
        public static bool UpdateFullProperty(UpdateFullPropertyDTO dto)
        {
            var property = clsProperties.FindByID(dto.PropertyID);
            if (property == null) return false;

            // ── Validate Services ──
            if (dto.Services != null)
            {
                foreach (var serviceId in dto.Services)
                    if (!clsServices.IsValidService(serviceId)) return false;
            }

            // ── Validate NewImages/NewImageSizes match ──
            if (dto.NewImages != null && dto.NewImageSizes != null &&
                dto.NewImages.Count != dto.NewImageSizes.Count)
            {
                Console.WriteLine("UpdateFullProperty: NewImages count != NewImageSizes count");
                return false;
            }

            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // ── 1. Update Location (same transaction) ──
                if (dto.Latitude != 0 && dto.Longitude != 0)
                {
                    bool locOk = _UpdateLocation(
                        connection, transaction,
                        locationId: property.LocationID,
                        address: dto.Address ?? "",
                        latitude: dto.Latitude,
                        longitude: dto.Longitude
                    );
                    if (!locOk)
                        Console.WriteLine($"Warning: Failed to update location for property {dto.PropertyID}");
                }

                // ── 2. Update FullAddress + University (same transaction) ──
                if (!string.IsNullOrWhiteSpace(dto.Address))
                    _UpdatePropertyFullAddress(connection, transaction, dto.PropertyID, dto.Address);

                if (dto.UniversityId > 0)
                    _UpdatePropertyUniversity(connection, transaction, dto.PropertyID, dto.UniversityId);

                // ── 3. Update Property basic data ──
                property.Title = dto.Title;
                property.Description = dto.Description;
                property.Price = dto.Price;
                property.Rooms = dto.Rooms;
                property.PropertyType = dto.PropertyType;
                property.Mode = clsProperties.enMode.Update;

                if (!property.Save(connection, transaction))
                    throw new Exception("Failed to update property");

                // ── 4. Delete specified images ──
                if (dto.ImageIdsToDelete != null && dto.ImageIdsToDelete.Count > 0)
                {
                    foreach (var imageId in dto.ImageIdsToDelete)
                    {
                        if (!clsPropertyImages.Delete(imageId, dto.PropertyID, connection, transaction))
                            throw new Exception($"Failed to delete image: {imageId}");
                    }
                }

                // ── 5. Add new images ──
                if (dto.NewImages != null && dto.NewImages.Count > 0)
                {
                    for (int i = 0; i < dto.NewImages.Count; i++)
                    {
                        long size = (dto.NewImageSizes != null && i < dto.NewImageSizes.Count)
                                    ? dto.NewImageSizes[i] : 0;

                        var img = new clsPropertyImages();
                        if (!img.Save(connection, transaction,
                                fileSizeBytes: size,
                                propertyId: dto.PropertyID,
                                imagePath: dto.NewImages[i]))
                            throw new Exception($"Failed to add image: {dto.NewImages[i]}");
                    }
                }

                // ── 6. Handle Video ──
                if (dto.DeleteVideo || !string.IsNullOrEmpty(dto.NewVideoUrl))
                    clsPropertyVideo.DeleteByPropertyID(dto.PropertyID, connection, transaction);

                if (!string.IsNullOrEmpty(dto.NewVideoUrl))
                {
                    var video = new clsPropertyVideo();
                    bool videoSaved = video.Save(connection, transaction,
                        fileSizeBytes: dto.NewVideoSize,
                        propertyId: dto.PropertyID,
                        videoPath: dto.NewVideoUrl);

                    if (!videoSaved)
                        Console.WriteLine($"Warning: Video not updated for property {dto.PropertyID}");
                }

                // ── 7. Update Services — Delete all then re-insert ──
                if (dto.Services != null)
                {
                    if (!clsPropertyServices.DeleteAllByPropertyID(dto.PropertyID, connection, transaction))
                        throw new Exception("Failed to delete old services");

                    foreach (var serviceID in dto.Services)
                    {
                        var svc = new clsPropertyServices();
                        if (!svc.Save(connection, transaction,
                                propertyId: dto.PropertyID,
                                serviceId: serviceID))
                            throw new Exception($"Failed to add service: {serviceID}");
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error in UpdateFullProperty: {ex.Message}");
                return false;
            }
        }


        // =====================================================
        // PRIVATE HELPERS — كلهم بيشتغلوا بنفس Connection + Transaction
        // المرحلة 1 + المرحلة 2: typed parameters بدل AddWithValue
        // =====================================================

        private static int _AddLocation(SqlConnection conn, SqlTransaction tx,
            string city, string area, string street,
            double? latitude, double? longitude)
        {
            try
            {
                const string query = @"
                    INSERT INTO Location (City, Area, Street, Latitude, Longitude)
                    OUTPUT INSERTED.LocationId
                    VALUES (@City, @Area, @Street, @Latitude, @Longitude)";

                using var cmd = new SqlCommand(query, conn, tx);

                cmd.Parameters.Add("@City", System.Data.SqlDbType.NVarChar, 20).Value = city;
                cmd.Parameters.Add("@Area", System.Data.SqlDbType.NVarChar, 50).Value = area;
                cmd.Parameters.Add("@Street", System.Data.SqlDbType.NVarChar, 50).Value = (object)street ?? DBNull.Value;
                cmd.Parameters.Add("@Latitude", System.Data.SqlDbType.Float).Value = (object)latitude ?? DBNull.Value;
                cmd.Parameters.Add("@Longitude", System.Data.SqlDbType.Float).Value = (object)longitude ?? DBNull.Value;

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"_AddLocation error: {ex.Message}");
                return -1;
            }
        }

        private static bool _UpdateLocation(SqlConnection conn, SqlTransaction tx,
            int locationId, string address, double latitude, double longitude)
        {
            try
            {
                const string query = @"
                    UPDATE Location
                    SET Street    = @Address,
                        Latitude  = @Latitude,
                        Longitude = @Longitude
                    WHERE LocationId = @LocationId";

                using var cmd = new SqlCommand(query, conn, tx);

                cmd.Parameters.Add("@LocationId", System.Data.SqlDbType.Int).Value = locationId;
                cmd.Parameters.Add("@Address", System.Data.SqlDbType.NVarChar, 50).Value = (object)address ?? DBNull.Value;
                cmd.Parameters.Add("@Latitude", System.Data.SqlDbType.Float).Value = latitude;
                cmd.Parameters.Add("@Longitude", System.Data.SqlDbType.Float).Value = longitude;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"_UpdateLocation error: {ex.Message}");
                return false;
            }
        }

        private static void _UpdatePropertyFullAddress(SqlConnection conn, SqlTransaction tx,
            int propertyId, string fullAddress)
        {
            try
            {
                const string query = "UPDATE Properties SET FullAddress = @FullAddress WHERE PropertyId = @PropertyId";

                using var cmd = new SqlCommand(query, conn, tx);
                cmd.Parameters.Add("@PropertyId", System.Data.SqlDbType.Int).Value = propertyId;
                cmd.Parameters.Add("@FullAddress", System.Data.SqlDbType.NVarChar, 300).Value = (object)fullAddress ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"_UpdatePropertyFullAddress error: {ex.Message}");
            }
        }

        private static void _UpdatePropertyUniversity(SqlConnection conn, SqlTransaction tx,
            int propertyId, int universityId)
        {
            try
            {
                const string query = "UPDATE Properties SET UniversityId = @UniversityId WHERE PropertyId = @PropertyId";

                using var cmd = new SqlCommand(query, conn, tx);
                cmd.Parameters.Add("@PropertyId", System.Data.SqlDbType.Int).Value = propertyId;
                cmd.Parameters.Add("@UniversityId", System.Data.SqlDbType.Int).Value = universityId;

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"_UpdatePropertyUniversity error: {ex.Message}");
            }
        }
        */



        /*
        public static int CreateFullProperty(CreateFullPropertyDTO dto)
        {
            if (dto.Services != null)
            {
                foreach (var serviceId in dto.Services)
                {
                    if (!clsServices.IsValidService(serviceId))
                        return -1;
                }
            }

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. إنشاء Location
                    int locationId = clsLocation.AddLocation(
                        city: !string.IsNullOrWhiteSpace(dto.City) ? dto.City : "Default City",
                        area: !string.IsNullOrWhiteSpace(dto.Area) ? dto.Area : "Default Area",
                        street: dto.Street ?? "",
                        latitude: dto.Latitude != 0 ? (double?)dto.Latitude : null,
                        longitude: dto.Longitude != 0 ? (double?)dto.Longitude : null
                    );

                    if (locationId == -1)
                        throw new Exception("Failed to create location");

                    // 2. إنشاء Property
                    var property = new clsProperties
                    {
                        OwnerID = dto.OwnerID,
                        Title = dto.Title,
                        Description = dto.Description,
                        Price = dto.Price,
                        Rooms = dto.Rooms,
                        PropertyType = dto.PropertyType,
                        LocationID = locationId,
                        PropertyStatusID = 1,
                        Mode = clsProperties.enMode.AddNew
                    };

                    bool propertySaved = property.Save(connection, transaction);
                    if (!propertySaved)
                        throw new Exception("Failed to add property");

                    int propertyID = property.PropertyID;

                    // 3. حفظ العنوان والجامعة
                    if (!string.IsNullOrWhiteSpace(dto.Address))
                        clsLocation.UpdatePropertyFullAddress(propertyID, dto.Address);

                    if (dto.UniversityId > 0)
                        clsLocation.UpdatePropertyUniversity(propertyID, dto.UniversityId);

                    // 4. حفظ الصور
                    if (dto.Images != null && dto.Images.Count > 0)
                    {
                        for (int i = 0; i < dto.Images.Count; i++)
                        {
                            var img = new clsPropertyImages();
                            bool imageSaved = img.Save(connection, transaction,
                                fileSizeBytes: dto.ImageSizes[i],
                                propertyId: propertyID,
                                imagePath: dto.Images[i]);

                            if (!imageSaved)
                                throw new Exception($"Failed to add image: {dto.Images[i]}");
                        }
                    }

                    // 5. حفظ الفيديو (اختياري)
                    if (!string.IsNullOrEmpty(dto.VideoUrl))
                    {
                        try
                        {
                            var video = new clsPropertyVideo();
                            bool videoSaved = video.Save(connection, transaction,
                                fileSizeBytes: dto.VideoSize,
                                propertyId: propertyID,
                                videoPath: dto.VideoUrl);

                            if (!videoSaved)
                                Console.WriteLine($"Warning: Video not saved for property {propertyID}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Video save failed: {ex.Message}");
                        }
                    }

                    // 6. حفظ الخدمات
                    if (dto.Services != null)
                    {
                        foreach (var serviceID in dto.Services)
                        {
                            var service = new clsPropertyServices();
                            bool serviceSaved = service.Save(connection, transaction,
                                propertyId: propertyID,
                                serviceId: serviceID);

                            if (!serviceSaved)
                                throw new Exception($"Failed to add service: {serviceID}");
                        }
                    }

                    transaction.Commit();
                    return propertyID;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error in CreateFullProperty: {ex.Message}");
                    return -1;
                }
            }
        }


        public static bool UpdateFullProperty(UpdateFullPropertyDTO dto)
        {
            var property = clsProperties.FindByID(dto.PropertyID);
            if (property == null)
                return false;

            if (dto.Services != null)
            {
                foreach (var serviceId in dto.Services)
                {
                    if (!clsServices.IsValidService(serviceId))
                        return false;
                }
            }

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. تحديث Location (اختياري - بس لو في coordinates)
                    if (dto.Latitude != 0 && dto.Longitude != 0)
                    {
                        bool locationUpdated = clsLocationData.UpdateLocation(
                            locationId: property.LocationID,
                            address: dto.Address ?? "",
                            latitude: dto.Latitude,
                            longitude: dto.Longitude
                        );

                        if (!locationUpdated)
                            Console.WriteLine($"Warning: Failed to update location for property {dto.PropertyID}");
                    }

                    if (!string.IsNullOrWhiteSpace(dto.Address))
                        clsLocation.UpdatePropertyFullAddress(dto.PropertyID, dto.Address);

                    if (dto.UniversityId > 0)
                        clsLocation.UpdatePropertyUniversity(dto.PropertyID, dto.UniversityId);

                    // 2. تحديث بيانات العقار
                    property.Title = dto.Title;
                    property.Description = dto.Description;
                    property.Price = dto.Price;
                    property.Rooms = dto.Rooms;
                    property.PropertyType = dto.PropertyType;
                    property.Mode = clsProperties.enMode.Update;

                    bool propertySaved = property.Save(connection, transaction);
                    if (!propertySaved)
                        throw new Exception("Failed to update property");

                    // 3. حذف الصور المحددة
                    if (dto.ImageIdsToDelete != null && dto.ImageIdsToDelete.Count > 0)
                    {
                        foreach (var imageId in dto.ImageIdsToDelete)
                        {
                            bool deleted = clsPropertyImages.Delete(imageId, dto.PropertyID, connection, transaction);
                            if (!deleted)
                                throw new Exception($"Failed to delete image: {imageId}");
                        }
                    }

                    // 4. إضافة صور جديدة
                    if (dto.NewImages != null && dto.NewImages.Count > 0)
                    {
                        for (int i = 0; i < dto.NewImages.Count; i++)
                        {
                            var img = new clsPropertyImages();
                            bool imageSaved = img.Save(connection, transaction,
                                fileSizeBytes: dto.NewImageSizes[i],
                                propertyId: dto.PropertyID,
                                imagePath: dto.NewImages[i]);

                            if (!imageSaved)
                                throw new Exception($"Failed to add image: {dto.NewImages[i]}");
                        }
                    }

                    // 5. معالجة الفيديو (اختياري)
                    if (dto.DeleteVideo || !string.IsNullOrEmpty(dto.NewVideoUrl))
                        clsPropertyVideo.DeleteByPropertyID(dto.PropertyID, connection, transaction);

                    if (!string.IsNullOrEmpty(dto.NewVideoUrl))
                    {
                        try
                        {
                            var video = new clsPropertyVideo();
                            bool videoSaved = video.Save(connection, transaction,
                                fileSizeBytes: dto.NewVideoSize,
                                propertyId: dto.PropertyID,
                                videoPath: dto.NewVideoUrl);

                            if (!videoSaved)
                                Console.WriteLine($"Warning: Video not updated for property {dto.PropertyID}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Video update failed: {ex.Message}");
                        }
                    }

                    // 6. تحديث الخدمات
                    if (dto.Services != null)
                    {
                        bool deleted = clsPropertyServices.DeleteAllByPropertyID(dto.PropertyID, connection, transaction);
                        if (!deleted)
                            throw new Exception("Failed to delete old services");

                        foreach (var serviceID in dto.Services)
                        {
                            var service = new clsPropertyServices();
                            bool serviceSaved = service.Save(connection, transaction,
                                propertyId: dto.PropertyID,
                                serviceId: serviceID);

                            if (!serviceSaved)
                                throw new Exception($"Failed to add service: {serviceID}");
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error in UpdateFullProperty: {ex.Message}");
                    return false;
                }
            }
        }



        */
    }
}