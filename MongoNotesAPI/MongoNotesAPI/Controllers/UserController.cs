using ICTPRG553.Models.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoNotesAPI.Middleware;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.DTOs;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Repositories;

namespace MongoNotesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  

    public class UserController : ControllerBase
    {
        //Stores a reference to our repository so we can request database actions
        private readonly IUserRepository _userRepository;
        //Requests our repository from the dependency injection and stores it in our 
        //readonly field.
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        [ApiKey("ADMIN")]

        public ActionResult CreateUser(string apiKey, UserCreateDTO userDTO)
        {
            //Ckeck if the user apiKey meets the required level(Admin Access) to add a
            //new user to the system.

            var user = new ApiUser
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Role = userDTO.Role,
                Active = true
                
            };

            var result = _userRepository.CreateUser(user);
            return Ok();
        }

        [HttpDelete("DeleteUser")]
        [ApiKey("ADMIN")]
        public ActionResult DeleteUser(ApiUser user, string apiKey, String id)
        {
            //Ckeck if the user apiKey meets the required level(Admin Access) to add a
            //new user to the system.

            var result = _userRepository.DeleteUser(user,id);
            return Ok();
        }
        // public ActionResult DeleteUser(string apiKey, UserDeleteDTO userDTO)
        // {
        //Ckeck if the user apiKey meets the required level(Admin Access) to add a
        //new user to the system.
        // find by id 
        //var result = _userRepository.CreateUser(user);
        //
        //  return Ok();

        [HttpDelete("DeleteOlderThanGivenDays")]
        public ActionResult DeleteOlderThanDays([FromQuery] int? days)
        {
            //Check if a days value is provided and that it complies with our business rules 
            if (days == null || days <= 30)
            {
                return BadRequest();
            }

            UserFilter filter = new UserFilter
            {
                //Add a created before filter to our filter details to be used for building our
                //filter definitions later. The calculation in the add days section ensures the value
                //will result in a past date, not a future one by accident.
                CreatedBefore = DateTime.Now.AddDays(Math.Abs((int)days) * -1)
            };

            //Process the reauest and store the details regarding the success/failure of the 
            //request
            var result = _userRepository.DeleteMany(filter);
            //If the request show a failure, inform the user.
            if (result.WasSuccessful == false)
            {
                result.Message = "No records found within that range";
                return Ok(result);
            }
            //Otherwise, send an Ok(200) message
            return Ok(result);
        }
    }
}
 