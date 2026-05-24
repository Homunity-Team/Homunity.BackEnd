using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Homunity_Buisness_Logic
{
    public class clsProperties
    {
        public enum enMode { AddNew = 0, Update = 1 }

        [JsonIgnore]
        public enMode Mode { get; set; } = enMode.AddNew;

        // الحقول الأساسية
        public int PropertyID { get; set; }
        public int OwnerID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public int LocationID { get; set; }
        public int PropertyStatusID { get; set; }
        public string PropertyType { get; set; }
        public string RejectReason { get; set; }
        public DateTime CreatedAt { get;  set; }

        [JsonIgnore]
        public string? OwnerName { get; set; }

        [JsonIgnore]
        public string? City { get; set; }

        [JsonIgnore]
        public string? Area { get; set; }

        [JsonIgnore]
        public string? Street { get; set; }

        [JsonIgnore]
        public double? Latitude { get; set; }

        [JsonIgnore]
        public double? Longitude { get; set; }

        [JsonIgnore]
        public int? UniversityId { get; set; }

        [JsonIgnore]
        public string? UniversityName { get; set; }

        public string? Address { get; set; }


        [JsonIgnore]
        public double? DistanceKm { get; set; }



        // =============================================
        // Status Constants
        // =============================================
        private const int STATUS_PENDING = 1;
        private const int STATUS_APPROVED = 2;
        private const int STATUS_REJECTED = 3;

        // كونستراكتور واحد فقط
        public clsProperties()
        {
            PropertyID = -1;
            OwnerID = -1;
            Title = "";
            Description = "";
            Price = -1;
            Rooms = -1;
            LocationID = -1;
            PropertyStatusID = -1;
            PropertyType = "";
            RejectReason = "";
            CreatedAt = DateTime.Now;
            Mode = enMode.AddNew;
        }

 

        public void LoadFromDataRow(DataRow row)
        {
            PropertyID = Convert.ToInt32(row["PropertyID"]);
            OwnerID = Convert.ToInt32(row["OwnerID"]);
            Title = row["Title"].ToString();
            Description = row["Description"].ToString();
            Price = Convert.ToDecimal(row["Price"]);
            Rooms = Convert.ToInt32(row["Rooms"]);
            PropertyType = row["PropertyType"] == DBNull.Value ? "" : row["PropertyType"].ToString();
            RejectReason = row["RejectReason"] == DBNull.Value ? "" : row["RejectReason"].ToString();
            CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            LocationID = Convert.ToInt32(row["LocationID"]);
            PropertyStatusID = Convert.ToInt32(row["StatusID"]);

            //  City + Area لو الـ Query جابتهم
            City = row.Table.Columns.Contains("City") && row["City"] != DBNull.Value
                   ? row["City"].ToString() : string.Empty;

            Area = row.Table.Columns.Contains("Area") && row["Area"] != DBNull.Value
                   ? row["Area"].ToString() : string.Empty;
            //  صح — زي City و Area بالظبط
            Street = row.Table.Columns.Contains("Street") && row["Street"] != DBNull.Value
                   ? row["Street"].ToString() : null;


            Latitude = row.Table.Columns.Contains("Latitude") && row["Latitude"] != DBNull.Value
            ? Convert.ToDouble(row["Latitude"]) : (double?)null;

            Longitude = row.Table.Columns.Contains("Longitude") && row["Longitude"] != DBNull.Value
                        ? Convert.ToDouble(row["Longitude"]) : (double?)null;

            OwnerName = row.Table.Columns.Contains("OwnerName") && row["OwnerName"] != DBNull.Value
            ? row["OwnerName"].ToString() : null;

            UniversityId = row.Table.Columns.Contains("UniversityId") && row["UniversityId"] != DBNull.Value
               ? Convert.ToInt32(row["UniversityId"]) : (int?)null;

            UniversityName = row.Table.Columns.Contains("UniversityName") && row["UniversityName"] != DBNull.Value
                             ? row["UniversityName"].ToString() : null;

            Address = row.Table.Columns.Contains("Address") && row["Address"] != DBNull.Value
          ? row["Address"].ToString() : null;


            Mode = enMode.Update;
        }



        // ================= Add New Property =================
        private bool _AddNewProperty(SqlConnection connection, SqlTransaction transaction)
        {
            if (!_Validate())
                return false;

            int newId = clsPropertiesData.AddNewProperty(
                OwnerID, Title, Description, Price, Rooms,
                PropertyType, LocationID, PropertyStatusID, RejectReason,
                connection, transaction
            );

            if (newId == -1)
                return false;

            PropertyID = newId;
            Mode = enMode.Update;
            return true;
        }


        // ================= Update Property =================
        private bool _UpdateProperty(SqlConnection connection, SqlTransaction transaction)
        {
            return clsPropertiesData.UpdateProperty(
                PropertyID, OwnerID, Title, Description, Price, Rooms,
                PropertyType, LocationID, PropertyStatusID, RejectReason,
                connection, transaction
            );
        }

        // ================= Validation =================
        private bool _Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return false;

            if (Price <= 0)
                return false;

            if (!clsLocation.IsValidLocation(LocationID))
                return false;

            if (!clsPropertyStatus.IsValidStatus(PropertyStatusID))
                return false;

            return true;
        }




        // ================= Save (Add / Update) =================
        public bool Save(SqlConnection connection = null, SqlTransaction transaction = null)
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewProperty(connection, transaction))
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateProperty(connection, transaction);
            }
            return false;
        }

        // ================= Delete =================
        public static bool Delete(int PropertyID)
        {
            return clsPropertiesData.DeleteProperty(PropertyID);
        }




        // ================= Find By ID =================
        public static clsProperties FindByID(int propertyID)
        {
            int ownerId = 0, rooms = 0, locationId = 0, statusId = 0;
            string title = "", description = "", propertyType = "";
            string rejectReason = "", city = "", area = "";
            string street = null;
            double? latitude = null;   // ✅ أضفنا
            double? longitude = null;  // ✅ أضفنا
            decimal price = 0;
            DateTime createdAt = DateTime.Now;

            bool found = clsPropertiesData.GetPropertyByID(
                propertyID,
                out ownerId, out title, out description,
                out price, out rooms, out propertyType,
                out locationId, out statusId, out rejectReason,
                out createdAt, out city, out area, out street,
                out latitude, out longitude  // ✅ أضفنا
            );

            if (!found)
                return null;

            return new clsProperties
            {
                PropertyID = propertyID,
                OwnerID = ownerId,
                Title = title,
                Description = description,
                Price = price,
                Rooms = rooms,
                PropertyType = propertyType,
                LocationID = locationId,
                PropertyStatusID = statusId,
                RejectReason = rejectReason,
                CreatedAt = createdAt,
                City = city,
                Area = area,
                Street = street,
                Latitude = latitude,   // ✅ أضفنا
                Longitude = longitude   // ✅ أضفنا
            };
        }


        // ================= Get All Properties =================
        public static List<clsProperties> GetAllProperties()
        {
            var dt = clsPropertiesData.GetAllProperties();
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);
                list.Add(property);
            }

            return list;
        }



        // ================= Search / Filter =================
        public static List<clsProperties> Search(string city, string area, decimal? minPrice, decimal? maxPrice)
        {
            var dt = clsPropertiesData.SearchProperties(city, area, minPrice, maxPrice);
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);
                list.Add(property);
            }

            return list;
        }

       
        
        
        // ================= Get PropertiesBy Owner ID =================
        public static List<clsProperties> GetPropertiesByOwnerID(int ownerID)
        {
            var dt = clsPropertiesData.GetPropertiesByOwnerID(ownerID);
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);
                list.Add(property);
            }

            return list;
        }




        // =============================================
        // APPROVE PROPERTY
        // =============================================
        public static bool Approve(int propertyId, int adminId)
        {
            // 1. Validate AdminId
            if (adminId <= 0)
                return false;

            if (!clsAdminActionsData.IsAdminValid(adminId))
                return false;

            // 2. Find Property
            var property = FindByID(propertyId);
            if (property == null)
                return false;

            // 3. Validate State Machine
            if (!clsPropertyStatus.CanChangeStatus(property.PropertyStatusID, STATUS_APPROVED))
                return false;

            // 4. Update Status
            return clsAdminActionsData.UpdatePropertyStatus(propertyId, STATUS_APPROVED);
        }




        // =============================================
        // REJECT PROPERTY
        // =============================================
        public static bool Reject(int propertyId, int adminId, string reason)
        {
            // 1. Validate AdminId
            if (adminId <= 0)
                return false;

            if (!clsAdminActionsData.IsAdminValid(adminId))
                return false;

            // 2. Validate Reason
            if (string.IsNullOrWhiteSpace(reason))
                return false;

            if (reason.Trim().Length < 10)
                return false;

            // 3. Find Property
            var property = FindByID(propertyId);
            if (property == null)
                return false;

            // 4. Validate State Machine
            if (!clsPropertyStatus.CanChangeStatus(property.PropertyStatusID, STATUS_REJECTED))
                return false;

            // 5. Save Reject Reason
            if (!clsAdminActionsData.SaveRejectReason(propertyId, reason.Trim()))
                return false;

            // 6. Update Status
            return clsAdminActionsData.UpdatePropertyStatus(propertyId, STATUS_REJECTED);
        }




        // =============================================
        // GET PENDING PROPERTIES (مع Pagination)
        // =============================================
        public static (List<clsProperties> Properties, int TotalCount) GetPendingProperties(int pageNumber, int pageSize)
        {
            // Validate Pagination
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var dt = clsAdminActionsData.GetPendingProperties(pageNumber, pageSize);
            var totalCount = clsAdminActionsData.GetPendingPropertiesCount();
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);
                list.Add(property);
            }

            return (list, totalCount);
        }




        // =============================================
        // GET REJECTED PROPERTIES
        // =============================================
        public static List<clsProperties> GetRejectedProperties()
        {
            var dt = clsAdminActionsData.GetRejectedProperties();
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);
                list.Add(property);
            }

            return list;
        }







        // ================= Find By ID V2 =================
        public static clsProperties FindByIDV2(int propertyID)
        {
            int ownerId = 0, rooms = 0, locationId = 0, statusId = 0;
            string title = "", description = "", propertyType = "";
            string rejectReason = "", city = "", area = "", street = null;
            double? latitude = null, longitude = null;
            int? universityId = null;
            string universityName = null;
            double? uniLat = null, uniLon = null;
            decimal price = 0;
            DateTime createdAt = DateTime.Now;
            string address = "";   // ✅ NEW

            bool found = clsPropertiesData.GetPropertyByIDV2(
                propertyID,
                out ownerId, out title, out description,
                out price, out rooms, out propertyType,
                out locationId, out statusId, out rejectReason,
                out createdAt, out city, out area, out street,
                out latitude, out longitude,
                out universityId, out universityName,
                out uniLat, out uniLon,
                out address);   // ✅ NEW

            if (!found) return null;

            var property = new clsProperties
            {
                PropertyID = propertyID,
                OwnerID = ownerId,
                Title = title,
                Description = description,
                Price = price,
                Rooms = rooms,
                PropertyType = propertyType,
                LocationID = locationId,
                PropertyStatusID = statusId,
                RejectReason = rejectReason,
                CreatedAt = createdAt,
                City = city,
                Area = area,
                Street = street,
                Latitude = latitude,
                Longitude = longitude,
                UniversityId = universityId,
                UniversityName = universityName,
                Address = address   // ✅ NEW
            };

            if (latitude.HasValue && longitude.HasValue && uniLat.HasValue && uniLon.HasValue)
            {
                property.DistanceKm = clsUniversities.CalculateDistance(
                    latitude.Value, longitude.Value, uniLat.Value, uniLon.Value);
            }

            return property;
        }
        // ================= Get All Properties V2 =================
        public static List<clsProperties> GetAllPropertiesV2()
        {
            var dt = clsPropertiesData.GetAllPropertiesV2();
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);

                // احسب المسافة
                if (property.Latitude.HasValue && property.Longitude.HasValue &&
                    row.Table.Columns.Contains("UniLat") && row["UniLat"] != DBNull.Value &&
                    row.Table.Columns.Contains("UniLon") && row["UniLon"] != DBNull.Value)
                {
                    double uniLat = Convert.ToDouble(row["UniLat"]);
                    double uniLon = Convert.ToDouble(row["UniLon"]);
                    property.DistanceKm = clsUniversities.CalculateDistance(
                        property.Latitude.Value, property.Longitude.Value,
                        uniLat, uniLon);
                }

                list.Add(property);
            }
            return list;
        }

        // ================= Get Properties By Owner V2 =================
        public static List<clsProperties> GetPropertiesByOwnerIDV2(int ownerID)
        {
            var dt = clsPropertiesData.GetPropertiesByOwnerIDV2(ownerID);
            var list = new List<clsProperties>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);

                // احسب المسافة
                if (property.Latitude.HasValue && property.Longitude.HasValue &&
                    row.Table.Columns.Contains("UniLat") && row["UniLat"] != DBNull.Value &&
                    row.Table.Columns.Contains("UniLon") && row["UniLon"] != DBNull.Value)
                {
                    double uniLat = Convert.ToDouble(row["UniLat"]);
                    double uniLon = Convert.ToDouble(row["UniLon"]);
                    property.DistanceKm = clsUniversities.CalculateDistance(
                        property.Latitude.Value, property.Longitude.Value,
                        uniLat, uniLon);
                }

                list.Add(property);
            }
            return list;
        }
    }
}