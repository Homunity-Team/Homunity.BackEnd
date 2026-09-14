using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs;
using Homunity_Shared_DTOs.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IPropertyOrchestratorService
    {
        Task<PropertyResponseDTO> GetPropertyByIdEfAsync(int propertyId, string baseUrl);
        Task<int> CreateFullPropertyAsync(CreateFullPropertyDTO dto);
        Task<bool> UpdateFullPropertyAsync(UpdateFullPropertyDTO dto);
        Task<PagedResult<PropertyListItemDto>> GetPropertiesPagedAsync(PropertyListQuery query, string baseUrl);
    }
}

