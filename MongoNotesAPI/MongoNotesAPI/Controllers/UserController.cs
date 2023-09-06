﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoNotesAPI.Middleware;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.DTOs;
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

      
    }
}