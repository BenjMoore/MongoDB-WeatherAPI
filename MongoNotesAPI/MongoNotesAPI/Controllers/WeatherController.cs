using ICTPRG553.Models;
using ICTPRG553.Models.DTOs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoNotesAPI.Middleware;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Repositories;
using System.Security.Cryptography;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MongoNotesAPI.Controllers
{
    [EnableCors("GooglePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey("Guest")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherRepository _repository;
        private readonly IUserRepository _userRepository;

        public WeatherController(IWeatherRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        // GET: api/Notes
        [HttpGet]
        [HttpHead]
        public IEnumerable<WeatherSensor> Get()
        {
            //Sends a message to the repository ot request it to retrieve all
            //entries from the database
            return _repository.GetAll(); ;
        }

        // GET: api/Notes/Filtered
        [HttpGet("Filtered")]
        public IEnumerable<WeatherSensor> Get([FromQuery] WeatherFilter filter)
        {
            //Sends a message to the repository ot request it to retrieve all
            //entries from the database
            return _repository.GetAll(filter);
        }

        [HttpHead("Filtered")]
        public ActionResult GetHeaders([FromQuery] WeatherFilter filter)
        {
            //Gets all records mathing the filter and finds the most recent one.
            var record = _repository.GetAll(filter).OrderByDescending(n => n.Time).FirstOrDefault();
            //Add a custom header to output the created date of the most recent record.
            HttpContext.Response.Headers.Add("last-created", record.Time.ToLocalTime().ToString());

            //Get the count of how many entries match the filter
            int count = _repository.GetAll(filter).Count();
            //Add a custom header to output the number of records that would be returned.
            HttpContext.Response.Headers.Add("record-count", count.ToString());

            return Ok();
        }

        // GET api/<NotesController>/5
        [HttpGet("{id}")]
        public ActionResult Get(string id, [FromQuery]string apiKey)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            return Ok(_repository.GetById(id));
        }

        [HttpPut("TrimWeather")]
        public ActionResult TrimWeather(string id, [FromBody] WeatherTrim updatedSensor)
        {
            if (String.IsNullOrWhiteSpace(id) || updatedSensor == null)
            {
                return BadRequest();
            }

            _TrimData.GetWeatherFiltered(updatedSensor);
            return Ok();
        }


        // POST api/<NotesController>
        [HttpPost]
        public ActionResult Post([FromBody] WeatherSensor? createdNote)
        {
            if (createdNote == null)
            {
                return BadRequest();
            }

            //Use a try catch to catch anny erors if the MOngo Db requests
            //causes an exceptrion
            try
            {
                _repository.Create(createdNote);
                return Ok("New note added");
            }
            catch (Exception ex)
            {
                //Send a problem error code if any issues occur.
                return Problem(detail: ex.Message, statusCode: 500);
            }

         
        }

        // POST api/Notes/PostMany
        [HttpPost("PostMany")]
        public ActionResult PostMany([FromBody] List<WeatherSensor>? createdNotes)
        {
            if (createdNotes == null || createdNotes.Count == 0)
            {
                return BadRequest();
            }

            _repository.CreateMany(createdNotes);
            return Ok();
        }

        // PUT api/<NotesController>/5
        [HttpPut("{id}")]
        public ActionResult Put(string id, [FromBody] WeatherSensor updatedNote)
        {
            if (String.IsNullOrWhiteSpace(id) || updatedNote == null)
            {
                return BadRequest();
            }

            _repository.Update(id, updatedNote);
            return Ok();
        }

        // PATCH api/Notes/UpdateMany
        [HttpPatch("UpdateMany")]
        public ActionResult UpdateMany([FromBody] WeatherPatchDetailsDTO? details)
        {
            //Check if a valid set of update details was provided.
            if (details == null)
            { 
                return BadRequest(); 
            }
            //Check if at least one of the update fiields has detilas to send to the database
            if (String.IsNullOrWhiteSpace(details.deviceName) )
            {
                return BadRequest();
            }
            //Check that we have at least 1 valid filter optoin set, otherwise the update will
            //target every document in the collection.
            if (details.Filter.CreatedBefore == null && details.Filter.CreatedAfter == null)
            {
                return BadRequest();
            }

            _repository.UpdateMany(details);
            return Ok();
        }

        // DELETE api/<NotesController>/5
        [HttpDelete("{id}")]
        [ApiKey("ADMIN")]
        public ActionResult Delete(string id, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return BadRequest("A valid _id is required to perform this operation");
            }

       

            //Process the reauest and store the details regarding the success/failure of the 
            //request
            var result = _repository.Delete(id);
            //If the request show a failure, inform the user.
            if (result.WasSuccessful == false) 
            { 
                return BadRequest(result);
            }
            //Otherwise, send an Ok(200) message
            return Ok(result);
        }

        /*
        [HttpGet("GetHighestTemp")]
        public List<HighestTempDTO> GetHighestTemp()
        {
            var result = _repository.getHighestTemp();

            return result;
        }
        */




        [HttpPut("Precipitation/{id}")]
        public ActionResult Precipitation(string id, [FromBody] PrecipitationDTO updatedSensor)
        {
            if (String.IsNullOrWhiteSpace(id) || updatedSensor == null)
            {
                return BadRequest();
            }

            _repository.UpdatePrecipitation(id, updatedSensor);
            return Ok();
        }







        //DELETE: api/Notes/DeleteOlderThanGivenDays
        [HttpDelete("DeleteOlderThanGivenDays")]
        public ActionResult DeleteOlderThanDays([FromQuery]int? days) 
        {
            //Check if a days value is provided and that it complies with our business rules 
            if (days == null || days <= 30)
            {
                return BadRequest();
            }

            WeatherFilter filter = new WeatherFilter
            {
                //Add a created before filter to our filter details to be used for building our
                //filter definitions later. The calculation in the add days section ensures the value
                //will result in a past date, not a future one by accident.
                CreatedBefore = DateTime.Now.AddDays(Math.Abs((int)days) * -1)
            };

            //Process the reauest and store the details regarding the success/failure of the 
            //request
            var result = _repository.DeleteMany(filter);
            //If the request show a failure, inform the user.
            if (result.WasSuccessful == false)
            {
                result.Message = "No records found within that range";
                return Ok(result);
            }
            //Otherwise, send an Ok(200) message
            return Ok(result);
        }

        [HttpDelete("DeleteByTitleMatch")]
        public ActionResult DeleteByTitleMatch([FromQuery] string? searchTerm)
        {
            //Validate our user input to ensure it meets our busines rules.
            if (searchTerm == null || searchTerm.Length <=3 )
            {
                return BadRequest("This endpoint requires a search parameter provided " +
                                                                "of more then 3 Characters ");
            }

            WeatherFilter filter = new WeatherFilter
            {
                //Create a new note filter object which takes the search term and will later be
                //passed to the delete method to delete based upon thr term provided.
               // id = searchTerm
            };

            //Process the reauest and store the details regarding the success/failure of the 
            //request
            var result = _repository.DeleteMany(filter);
            //If the request show a failure, inform the user.
            if (result.WasSuccessful == false)
            {
                return BadRequest(result);
            }
            //Otherwise, send an Ok(200) message
            return Ok(result);
        }

       

    }
}
