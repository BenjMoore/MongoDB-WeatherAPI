using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoNotesAPI.Models;
using MongoNotesAPI.Repositories;

namespace MongoNotesAPI.Middleware
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
    
        public UserRoles[] AllowedRoles { get; set; }

        public ApiKeyAttribute(params UserRoles[] roles)
        {
            AllowedRoles = roles;
        }


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.Query.TryGetValue("apiKey", out var key) == false)
            {
                context.Result = new ContentResult
                {
                    StatusCode = 401,
                    Content = "No apikey was provided"
                };

                return;
            }

            var providedkey = key.ToString().Trim('{', '}');

            
            // Get acsess to repo by requesting it directly 
            var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            // check if the user's role matches the required level
            if (!userRepo.AuthenticateUser(providedkey, AllowedRoles))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Accsess Denied, Api Key Was Invalid / Invalid For The Function Attempted"
                };
                return;
            }
            // If Ok let them through
            await next();
        }
    }
}
