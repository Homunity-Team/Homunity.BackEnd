using Homunity_Buisness_Logic;
using Homunity_Data_Access;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Properties")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {

        // ================= Add Property =================
        [HttpPost("AddProperty", Name = "AddProperty")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Add([FromBody] clsProperties property)
        {
            if (property == null)
                return BadRequest(new { message = "Invalid request body" });

            property.Mode = clsProperties.enMode.AddNew;
            bool result = property.Save();

            if (!result)
                return BadRequest(new { message = "Failed to add property" });

            return CreatedAtAction(nameof(GetByID),
                new { id = property.PropertyID },
                property);
        }



        // ================= Update Property =================
        [HttpPut("UpdateProperty", Name = "UpdateProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Update([FromBody] clsProperties property)
        {
            if (property == null || property.PropertyID <= 0)
                return BadRequest(new { message = "Invalid property data" });

            var existing = clsProperties.FindByID(property.PropertyID);
            if (existing == null)
                return NotFound(new { message = "Property not found" });

            property.Mode = clsProperties.enMode.Update;
            bool result = property.Save();

            if (!result)
                return BadRequest(new { message = "Update failed" });

            return Ok(new { message = "Property updated successfully" });
        }




        // ================= Delete Property =================
        [HttpDelete("DeleteProperty", Name = "DeleteProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(int id)
        {
            bool deleted = clsProperties.Delete(id);

            if (!deleted)
                return NotFound(new { message = "Property not found" });

            return Ok(new { message = "Property deleted successfully" });
        }







        // ================= Search Properties =================
        [HttpGet("Search", Name = "SearchProperties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Search(string city, string area, decimal? minPrice, decimal? maxPrice)
        {
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                return BadRequest(new { message = "MinPrice cannot be greater than MaxPrice" });

            var result = clsProperties.Search(city, area, minPrice, maxPrice);
            return Ok(result);
        }



        // ================= Get All Properties =================
        [HttpGet("GetAll", Name = "GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll()
        {
            var properties = clsProperties.GetAllProperties();

            if (properties == null || properties.Count == 0)
                return Ok(new List<clsProperties>());

            return Ok(properties);
        }




        // ================= Get Property By ID =================
        [HttpGet("GetByID", Name = "GetByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetByID(int id)
        {
            var property = clsProperties.FindByID(id);

            if (property == null)
                return NotFound(new { message = "Property not found" });

            return Ok(property);
        }


    }
}
