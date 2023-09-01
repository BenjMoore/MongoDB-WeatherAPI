namespace MongoNotesAPI.Middleware
{
    public class ExampleMidlewareClass : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            //into the api
            Console.WriteLine("Second custom midelware on the way in");

            await next(context);

            //out of the api, to the client
            Console.WriteLine("Second custom midelware on the way out");
        }
    }
}
