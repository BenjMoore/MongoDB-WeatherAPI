using MongoDB.Bson.Serialization.Attributes;

namespace ICTPRG553.Models.DTOs
{
    public class FilteredDataDTO
    {
        [BsonElement("Device Name")]
        public string deviceName { get; set; }
        [BsonElement("Time")]
        public DateTime Time { get; set; }
        [BsonElement("Temperature (°C)")]
        public Double Temperature { get; set; }
        [BsonElement("Atmospheric Pressure (kPa)")]
        public Double atmosphericPressure { get; set; }
        [BsonElement("Solar Radiation (W/m2)")]
        public Double solarRadiation { get; set; }
        [BsonElement("Precipitation mm/h")]
        public Double Precipitation { get; set; }

    }
}
