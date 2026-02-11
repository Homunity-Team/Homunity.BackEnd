using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

            // تحقق من الحقول الأساسية
            if (property.OwnerID <= 0)
                return BadRequest(new { message = "OwnerID is required and must be valid" });

            if (string.IsNullOrWhiteSpace(property.Title))
                return BadRequest(new { message = "Title is required" });

            if (property.Price <= 0)
                return BadRequest(new { message = "Price must be greater than 0" });

            property.Mode = clsProperties.enMode.AddNew;
            bool result = property.Save();

            if (!result)
                return BadRequest(new { message = "Failed to add property" });

            return CreatedAtAction(nameof(GetByID),
                new { id = property.PropertyID },
                new
                {
                    message = "Property added successfully",
                    propertyID = property.PropertyID
                });
        }







        // ================= Update Property =================
        [HttpPut("UpdateProperty", Name = "UpdateProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Update([FromBody] clsProperties property)
        {
            if (property == null)
                return BadRequest(new { message = "Invalid property data" });

            if (property.PropertyID <= 0)
                return BadRequest(new { message = "Property ID is required" });

            var existing = clsProperties.FindByID(property.PropertyID);
            if (existing == null)
                return NotFound(new { message = "Property not found" });

            property.Mode = clsProperties.enMode.Update;
            bool result = property.Save();

            if (!result)
                return BadRequest(new { message = "Update failed" });

            return Ok(new
            {
                message = "Property updated successfully",
                propertyID = property.PropertyID
            });
        }







        // ================= Delete Property =================
        [HttpDelete("DeleteProperty", Name = "DeleteProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            var existing = clsProperties.FindByID(id);
            if (existing == null)
                return NotFound(new { message = "Property not found" });

            bool deleted = clsProperties.Delete(id);

            if (!deleted)
                return StatusCode(500, new { message = "Failed to delete property" });

            return Ok(new
            {
                message = "Property deleted successfully",
                propertyID = id
            });
        }








        // ================= Search Properties =================
        [HttpGet("Search", Name = "SearchProperties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Search(string city = null, string area = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            // تحقق من صحة المدخلات
            if (minPrice.HasValue && minPrice < 0)
                return BadRequest(new { message = "MinPrice cannot be negative" });

            if (maxPrice.HasValue && maxPrice < 0)
                return BadRequest(new { message = "MaxPrice cannot be negative" });

            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                return BadRequest(new { message = "MinPrice cannot be greater than MaxPrice" });

            var result = clsProperties.Search(city, area, minPrice, maxPrice);

            return Ok(new
            {
                message = result.Count == 0 ? "No properties found matching your criteria" : "Properties found",
                count = result.Count,
                properties = result
            });
        }






        // ================= Get All Properties =================
        [HttpGet("GetAll", Name = "GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll()
        {
            var properties = clsProperties.GetAllProperties();

            return Ok(new
            {
                message = properties.Count == 0 ? "No properties found in the system" : "Properties retrieved successfully",
                count = properties.Count,
                properties = properties
            });
        }







        // ================= Get Property By ID =================
        [HttpGet("GetByID", Name = "GetByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetByID(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            var property = clsProperties.FindByID(id);

            if (property == null)
                return NotFound(new { message = $"Property with ID {id} not found" });

            return Ok(new
            {
                message = "Property found",
                property = property
            });
        }

    }
}