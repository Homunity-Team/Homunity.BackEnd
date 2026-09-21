using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs;
using Homunity_Shared_DTOs.Properties;
namespace Homunity_Buisness_Logic
{
    public interface IPropertyService
    {
        Task<PropertyResponseDTO> GetPropertyByIdEfAsync(int propertyId, string baseUrl);
        Task<int> CreateFullPropertyAsync(CreateFullPropertyDTO dto);
        Task<bool> UpdateFullPropertyAsync(UpdateFullPropertyDTO dto);
        Task<PagedResult<PropertyListItemDto>> GetPropertiesPagedAsync(PropertyListQuery query, string baseUrl);

        // Added in Sprint 1.5, replacing clsProperties.FindByID / clsProperties.Delete
        Task<PropertyOwnershipInfo?> GetOwnershipAsync(int propertyId);
        Task<PropertyDeleteOutcome> DeletePropertyAsync(int propertyId, int currentUserId, bool isAdmin);

        // Media reads — so controllers do not inject IPropertyRepository
        Task<List<PropertyImageProjection>> GetImagesByPropertyIdAsync(int propertyId);
        Task<PropertyImageProjection?> FindImageByIdAsync(int imageId);
        Task<int> CountImagesAsync(int propertyId);
        Task<PropertyVideoProjection?> GetVideoByPropertyIdAsync(int propertyId);
        Task<PropertyVideoProjection?> FindVideoByIdAsync(int videoId);
    }

    public class PropertyOwnershipInfo
    {
        public int OwnerId { get; set; }
        public int LocationId { get; set; }
    }

    public enum PropertyDeleteStatus { Success, NotFound, Forbidden, Failed }

    public class PropertyDeleteOutcome
    {
        public PropertyDeleteStatus Status { get; set; }
        public int PropertyId { get; set; }
    }
}