using MongoNotesAPI.Models;
using MongoNotesAPI.Models.Filters;

namespace MongoNotesAPI.Repositories
{
    //This interface outlines all the commands that can be asked of it by any other
    //class using it. Any other class implementing this interface must have all these
    //methods withih its code in order to use the interface as its communication system.
    //NOTE: Some of this interfaces methods are inherited form the IGenericRepository
    //interface.
    public interface INoteRepository: IGenericRepository<Note>
    {
        IEnumerable<Note> GetAll(NoteFilter noteFilter);
        void CreateMany(List<Note> noteList);
        OperationResponseDTO<Note> DeleteMany(NoteFilter filter);
        OperationResponseDTO<Note> UpdateMany(NotePatchDetailsDTO details);
    }
}
