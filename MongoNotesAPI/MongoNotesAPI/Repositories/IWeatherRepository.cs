using MongoNotesAPI.Models;
using MongoNotesAPI.Models.Filters;

namespace MongoNotesAPI.Repositories
{
    //This interface outlines all the commands that can be asked of it by any other
    //class using it. Any other class implementing this interface must have all these
    //methods withih its code in order to use the interface as its communication system.
    //NOTE: Some of this interfaces methods are inherited form the IGenericRepository
    //interface.
    public interface IWeatherRepository: IGenericRepository<WeatherSensor>
    {
        IEnumerable<WeatherSensor> GetAll(WeatherFilter noteFilter);
        void CreateMany(List<WeatherSensor> noteList);
        OperationResponseDTO<WeatherSensor> DeleteMany(WeatherFilter filter);
        OperationResponseDTO<WeatherSensor> UpdateMany(WeatherPatchDetailsDTO details);
    }
}
