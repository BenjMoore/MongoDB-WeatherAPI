namespace ICTPRG553.Models.DTOs
{
    public class FilteredDataDTO
    {
        public string deviceName { get; set; }
        public DateTime Time { get; set; }
        public Double Temperature { get; set; }
        public Double atmosphericPressure { get; set; }
        public Double solarRadiation { get; set; }
        public Double Precipitation { get; set; }

    }
}
