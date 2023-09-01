using MongoNotesAPI.Models.Filters;

namespace MongoNotesAPI.Models
{
    public class NotePatchDetailsDTO
    {
        public NoteFilter Filter { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
    }
}
