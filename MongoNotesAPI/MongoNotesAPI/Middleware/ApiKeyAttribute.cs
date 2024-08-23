using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoNotesAPI.Models;
using MongoNotesAPI.Repositories;

namespace MongoNotesAPI.Middleware
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private string requiredRole;

        public string RequiredRole
        {
            get { return requiredRole; }
        }

        public ApiKeyAttribute(string role = "ADMIN")
        {
            requiredRole = role;
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

            if (Enum.TryParse(requiredRole.ToUpper(), out UserRoles neededRole) == false)
            {
                context.Result = new ContentResult
                {
                    StatusCode = 500,
                    Content = "Bad Request!"
                };
                return;

            }
            // Get acsess to repo by requesting it directly 
            var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            // check if the user's role matches the required level
            if (userRepo.AuthenticateUser(providedkey, neededRole) == null) 
            {
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Access Denied, Api Key Was Invalid / Invalid For The Function Attempted"
                };
                return;
            }
            // If Ok let them through
            await next();
        }
    }
}
