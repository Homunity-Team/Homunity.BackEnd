using Homunity_Data_Access;
using System.Data;
using System.Text.Json.Serialization;

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
        public DateTime CreatedAt { get; private set; }

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

        // دالة لتحميل البيانات من DataRow
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
            Mode = enMode.Update;
        }

        // ================= Add New Property =================
        private bool _AddNewProperty()
        {
            if (!_Validate())
                return false;

            int newId = clsPropertiesData.AddNewProperty(
                OwnerID,
                Title,
                Description,
                Price,
                Rooms,
                PropertyType,
                LocationID,
                PropertyStatusID,
                RejectReason
            );

            if (newId == -1)
                return false;

            PropertyID = newId;
            Mode = enMode.Update;
            return true;
        }

        // ================= Update Property =================
        private bool _UpdateProperty()
        {
            return clsPropertiesData.UpdateProperty(
                PropertyID,
                OwnerID,
                Title,
                Description,
                Price,
                Rooms,
                PropertyType,
                LocationID,
                PropertyStatusID,
                RejectReason
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
        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewProperty())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case enMode.Update:
                    return _UpdateProperty();
            }
            return false;
        }

        // ================= Delete =================
        public static bool Delete(int PropertyID)
        {
            return clsPropertiesData.DeleteProperty(PropertyID);
        }

        // ================= Find By ID =================
        public static clsProperties FindByID(int PropertyID)
        {
            int OwnerID;
            string Title;
            string Description;
            decimal Price;
            int Rooms;
            string PropertyType;
            int LocationID;
            int StatusID;
            string RejectReason;
            DateTime CreatedAt;

            if (clsPropertiesData.GetPropertyByID(
                PropertyID,
                out OwnerID,
                out Title,
                out Description,
                out Price,
                out Rooms,
                out PropertyType,
                out LocationID,
                out StatusID,
                out RejectReason,
                out CreatedAt))
            {
                return new clsProperties
                {
                    PropertyID = PropertyID,
                    OwnerID = OwnerID,
                    Title = Title,
                    Description = Description,
                    Price = Price,
                    Rooms = Rooms,
                    PropertyType = PropertyType,
                    LocationID = LocationID,
                    PropertyStatusID = StatusID,
                    RejectReason = RejectReason,
                    CreatedAt = CreatedAt,
                    Mode = enMode.Update
                };
            }

            return null;
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
    }
}