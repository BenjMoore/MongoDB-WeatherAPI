using ICTPRG553.Models;
using ICTPRG553.Models.DTOs;
using ICTPRG553.Models.Filters;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoNotesAPI.Middleware;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MongoNotesAPI.Controllers
{
    [EnableCors("GooglePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherRepository _repository;
        private readonly IUserRepository _userRepository;

        public WeatherController(IWeatherRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Retrieves all weather sensor records from the database. 
        /// This endpoint returns a list of all weather sensor data recorded by the system, 
        /// providing a comprehensive overview of sensor readings.
        /// </summary>
        /// <returns>A list of all weather sensor records currently available in the system.</returns>
        /// <response code="200">Returns a list of weather sensor records.</response>
       
        [ApiKey("USER")]
        [HttpGet("GetAll")]
        public IEnumerable<WeatherSensor> Get()
        {
            return _repository.GetAll();
        }

        /// <summary>
        /// Retrieves weather sensor records filtered by the provided criteria.
        /// This allows for fetching sensor data based on specific conditions like date range, 
        /// device type, or location to narrow down the results to relevant records.
        /// </summary>
        /// <param name="filter">An object containing filter criteria such as dates, device name, or sensor location. 
        /// This helps customize the data retrieval based on specific parameters.</param>
        /// <returns>A list of weather sensor records that match the filter criteria.</returns>
        /// <response code="200">Returns a list of weather sensor records matching the filter criteria.</response>
        [ApiKey("USER")]
        [HttpGet("Filtered")]
        public IEnumerable<WeatherSensor> Get([FromQuery] WeatherFilter filter)
        {
            return _repository.GetAll(filter);
        }

        /// <summary>
        /// Retrieves a weather sensor record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the weather sensor. This should be a valid ID string.</param>
        /// <returns>A weather sensor record if found, otherwise null.</returns>
        /// <response code="200">Returns the weather sensor record if found.</response>
        /// <response code="404">If no weather sensor is found with the provided ID.</response>
        [ApiKey("USER")]
        [HttpGet("ID")]
        public WeatherSensor GetByID([FromQuery] string id)
        {
            return _repository.GetById(id);
        }

        /// <summary>
        /// Retrieves weather sensor data based on a specific date and device name. 
        /// If no date is provided, it defaults to null.
        /// This helps in fetching sensor data specific to a certain device or time period.
        /// </summary>
        /// <param name="selectedDateTime">Optional. The specific date and time to filter the sensor data by. 
        /// If left null, all records regardless of date are returned.</param>
        /// <param name="deviceName">The name of the weather sensor device. If left as an empty string, 
        /// all device data will be included in the response.</param>
        /// <returns>A filtered set of weather sensor records based on the given date and device name.</returns>
        /// <response code="200">Returns filtered sensor data based on the provided parameters.</response>
        [ApiKey("USER")]
        [HttpGet("GetFilteredData")]
        public FilteredDataDTO GetFiltered(DateTime? selectedDateTime, string deviceName)
        {
            
            if (!selectedDateTime.HasValue)
            {
                selectedDateTime = null;
            }
            var result = _repository.GetFilteredData(selectedDateTime, deviceName);
            return result;
        }

        /// <summary>
        /// Retrieves the weather sensor record that contains the highest precipitation measurement. 
        /// This endpoint is useful for finding extreme weather conditions recorded by the sensors.
        /// </summary>
        /// <returns>An object containing details of the record with the highest precipitation measurement.</returns>
        /// <response code="200">Returns the weather sensor record with the highest precipitation measurement.</response>
        [ApiKey("USER")]
        [HttpGet("MaxPrecipitation")]
        public PrecipitationDTO MaxPrecipitation(string deviceName)
        {
            var result = _repository.GetMaxPrecipitation(deviceName);
            return result;
        }

        /// <summary>
        /// Retrieves the weather sensor record for each sensor that contains the highest temperature reading. 
        /// This can be useful for analyzing extreme temperature conditions in the sensor's recorded history.
        /// </summary> 
        /// <returns>An object with details of the weather sensor record with the highest temperature.</returns>
        /// <response code="200">Returns the weather sensor record with the highest temperature reading.</response>
        [ApiKey("USER")]
        [HttpGet("MaxTemp")]
        public List<TempFilter> MaxTemp()
        {
            var result = _repository.GetMaxTemp();
            return result;
        }

        /// <summary>
        /// Creates a new weather sensor record and stores it in the database. 
        /// If the request body is invalid or empty, the operation will return a BadRequest response.
        /// </summary>
        /// <param name="createdNote">The new weather sensor record to be added to the database. 
        /// This should contain all relevant sensor details like date, time, temperature, and precipitation data.</param>
        /// <returns>A CreatedAtAction response indicating successful creation or a BadRequest/Problem response in case of errors.</returns>
        /// <response code="201">Indicates that the weather sensor record was successfully created.</response>
        /// <response code="400">If the request body is null or invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [ApiKey("SENSOR")]
        [HttpPost("New")]
        public ActionResult Post([FromBody] WeatherSensor? createdNote)
        {
            if (createdNote == null)
            {
                return BadRequest();
            }

            try
            {
                _repository.Create(createdNote);
                return CreatedAtAction(nameof(Post), createdNote);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        /// <summary>
        /// Creates multiple new weather sensor records and adds them to the database. 
        /// This is useful for batch operations where many sensor readings are recorded at once.
        /// </summary>
        /// <param name="createdNotes">A list of new weather sensor records to be added to the database. 
        /// Each record should include relevant sensor information such as temperature, precipitation, and timestamps.</param>
        /// <returns>An Ok response for successful creation or a BadRequest response if the input is invalid.</returns>
        /// <response code="200">Indicates that the weather sensor records were successfully created.</response>
        /// <response code="400">If the request body is null or empty.</response>
        
        [HttpPost("PostMany")]
        [ApiKey("SENSOR")]
        public ActionResult PostMany([FromBody] List<WeatherSensor>? createdNotes)
        {
            if (createdNotes == null || createdNotes.Count == 0)
            {
                return BadRequest();
            }

            _repository.CreateMany(createdNotes);
            return Ok();
        }

        /// <summary>
        /// Deletes weather sensor records that are older than a specified number of days. 
        /// This is used for maintaining the database by removing outdated records.
        /// </summary>
        /// <param name="days">The number of days used as the cutoff for deletion. Records older than this number will be deleted. 
        /// A minimum of 30 days is required for this operation to avoid accidental deletions.</param>
        /// <returns>An Ok response indicating success, or a BadRequest response if the input is invalid.</returns>
        /// <response code="200">Indicates that the records older than the specified number of days were successfully deleted.</response>
        /// <response code="400">If the number of days is not specified or is less than 30.</response>
        
        [HttpDelete("DeleteOlderThanGivenDays")]
        [ApiKey("TEACHER")]
        public ActionResult DeleteOlderThanDays([FromQuery] int? days)
        {
            if (days == null || days <= 30)
            {
                return BadRequest();
            }

            WeatherFilter filter = new WeatherFilter
            {
                CreatedBefore = DateTime.Now.AddDays(-days.Value)
            };

            var result = _repository.DeleteMany(filter);
            if (!result.WasSuccessful)
            {
                result.Message = "No records found within that range";
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Deletes a specific weather sensor record identified by its ID. 
        /// This operation requires an admin-level API key to perform the deletion.
        /// </summary>
        /// <param name="id">The unique identifier (_id) of the weather sensor record to delete. 
        /// If the ID is invalid or empty, the operation will return a BadRequest response.</param>
        /// <returns>An Ok response for successful deletion or a BadRequest response for invalid input.</returns>
        /// <response code="200">Indicates that the weather sensor record was successfully deleted.</response>
        /// <response code="400">If the ID is invalid or empty.</response>
        [HttpDelete("Delete/{id}")]
        [ApiKey("TEACHER")]
        public ActionResult Delete(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return BadRequest("A valid _id is required to perform this operation");
            }

            var result = _repository.Delete(id);
            if (!result.WasSuccessful)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Updates a specific weather sensor record identified by its ID with new data. 
        /// This allows for modifying existing records in the database to reflect changes in the recorded weather data.
        /// </summary>
        /// <param name="id">The unique identifier (_id) of the record to update.</param>
        /// <param name="updatedNote">An object containing the updated weather sensor data such as temperature, precipitation, etc.</param>
        /// <returns>An Ok response for successful update or a BadRequest response for invalid input.</returns>
        /// <response code="200">Indicates that the weather sensor record was successfully updated.</response>
        /// <response code="400">If the ID is invalid or the updated data is null.</response>
        [ApiKey("TEACHER")]
        [HttpPut("Update/{id}")]
        public ActionResult Put(string id, [FromBody] WeatherSensor updatedNote)
        {
            if (String.IsNullOrWhiteSpace(id) || updatedNote == null)
            {
                return BadRequest();
            }

            _repository.Update(id, updatedNote);
            return Ok();
        }

        /// <summary>
        /// Updates the precipitation data for a specific weather sensor record by its ID. 
        /// This is a focused update operation where only the precipitation value is modified.
        /// </summary>
        /// <param name="id">The unique identifier (_id) of the record to update.</param>
        /// <param name="updatedSensor">An object containing the new precipitation value to be updated in the record.</param>
        /// <returns>An Ok response for successful update or a BadRequest response for invalid input.</returns>
        /// <response code="200">Indicates that the precipitation data was successfully updated.</response>
        /// <response code="400">If the ID is invalid or the updated data is null.</response>
        [ApiKey("TEACHER")]
        [HttpPut("Precipitation/{id}")]
        public ActionResult Precipitation(string id, double precipitation)
        {
            PrecipitationDTO dto = new PrecipitationDTO();
            {
                dto.Precipitation = precipitation;
            };
            if (String.IsNullOrWhiteSpace(id) || dto == null)
            {
                return BadRequest();
            }

            _repository.UpdatePrecipitation(id, dto);
            return Ok();
        }

        /// <summary>
        /// Updates multiple weather sensor records based on a filter and the provided update details. 
        /// This is useful for bulk updates where a set of records matching the filter criteria are modified at once.
        /// </summary>
        /// <param name="details">An object containing the filter criteria and the details of the updates to be applied. 
        /// This should include both the fields to be updated and the conditions for selecting records.</param>
        /// <returns>An Ok response indicating success, or a BadRequest response for invalid input.</returns>
        /// <response code="200">Indicates that the weather sensor records were successfully updated.</response>
        /// <response code="400">If the filter criteria or update details are invalid.</response>
        [ApiKey("TEACHER")]
        [HttpPut("UpdateMany")]
        public ActionResult UpdateMany([FromBody] WeatherPatchDetailsDTO? details)
        {
            if (details == null)
            {
                return BadRequest();
            }
            if (String.IsNullOrWhiteSpace(details.deviceName))
            {
                return BadRequest();
            }
            if (details.Filter.CreatedBefore == null && details.Filter.CreatedAfter == null)
            {
                return BadRequest();
            }

            _repository.UpdateMany(details);
            return Ok();
        }
    }
}
