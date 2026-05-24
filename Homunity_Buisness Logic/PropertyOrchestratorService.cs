using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homunity_Buisness_Logic
{
    public class PropertyOrchestratorService
    {
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
    }
}