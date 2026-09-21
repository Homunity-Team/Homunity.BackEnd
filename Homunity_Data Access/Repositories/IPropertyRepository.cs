using Homunity_Shared_DTOs.Properties;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{

    public interface IPropertyRepository
    {
        Task<PropertyEntity> GetByIdWithDetailsAsync(int propertyId);
        Task<HashSet<int>> GetValidServiceIdsAsync(IEnumerable<int> serviceIds);
        Task<int> CreatePropertyAsync(PropertyEntity property, List<int> serviceIds);
        Task<bool> UpdatePropertyCoreAsync(PropertyUpdateCommand command);
        Task<bool> AddVideoAsync(int propertyId, string videoPath);
        Task DeleteVideosByPropertyIdAsync(int propertyId);
        Task<(List<PropertyListProjection> Items, int TotalCount)> GetPagedAsync(PropertyListQuery query);
        Task<List<PropertyChatProjection>> GetActiveForChatAsync(int maxCount);
        Task<(int OwnerId, int LocationId)?> GetOwnershipAsync(int propertyId);
        Task<List<PropertyImageProjection>> GetImagesByPropertyIdAsync(int propertyId);
        Task<PropertyImageProjection?> FindImageByIdAsync(int imageId);
        Task<int> CountImagesAsync(int propertyId);
        Task<PropertyVideoProjection?> GetVideoByPropertyIdAsync(int propertyId);
        Task<PropertyVideoProjection?> FindVideoByIdAsync(int videoId);
        Task<bool> DeletePropertyCascadeAsync(int propertyId);


    }
}
