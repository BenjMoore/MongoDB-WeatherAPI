using MongoNotesAPI.Models;

namespace MongoNotesAPI.Repositories
{
    public interface IGenericRepository<T>
    {
        IEnumerable<T> GetAll();
        T GetById(string id);
        void Create(T item);
        OperationResponseDTO<T> Update(string id, T item);
        OperationResponseDTO<T> Delete(string id);
    }
}
