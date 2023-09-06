﻿namespace MongoNotesAPI.Models.Filters
{
    public class WeatherFilter
    {
        public string? TitleMatch { get; set; }
        public string? BodyMatch { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
    }
}