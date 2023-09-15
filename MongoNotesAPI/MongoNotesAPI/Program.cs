using Microsoft.OpenApi.Models;
using MongoNotesAPI.Middleware;
using MongoNotesAPI.Repositories;
using MongoNotesAPI.Services;
using MongoNotesAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => 
{
    options.AddSecurityDefinition("apiKey", new OpenApiSecurityScheme
    {
        Description = "Enter Your Api Key Here!",
        Name = "apiKey",
        In = ParameterLocation.Query,
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement 
    {
        { 
            new OpenApiSecurityScheme
            {
             Reference = new OpenApiReference
             {
                 Type = ReferenceType.SecurityScheme,
                 Id = "apiKey"
             },

             Name = "apiKey",
             In = ParameterLocation.Query

            },
            new List<String>()
        }
    }
    );
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ICTPRG553.xml"));
});

//Adds the MongoConnectionSettings to the saervices container and sets it up to hold 
//the details of our conneciton settings in the appsettings.json file
builder.Services.Configure<MongoConnectionSettings>(builder.Configuration.GetSection("MongoConnectionSettings"));
//Adds our connection builder to the services container
builder.Services.AddScoped<MongoConnectionBuilder>();

builder.Services.AddScoped<IWeatherRepository,WeatherRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//Add new cors policies to the system in our services.
builder.Services.AddCors(options => 
{
    //Add a new policy to the system called "GooglePolicy"
    options.AddPolicy("GooglePolicy", p => 
    {
        //Set the policy rules to allow communication from google.com or google.com.au
        p.WithOrigins("https://www.google.com", "https://www.google.com.au");
        p.AllowAnyHeader();
        //Set the HTTP methods that these origins are allowed to send. 
        p.WithMethods("GET","PUT","POST","DELETE");
    });
    options.AddPolicy("YouTubePolicy", p =>
    {
        //Set the policy rules to allow communication from google.com or google.com.au
        p.WithOrigins("https://www.youtube.com");
        p.AllowAnyHeader();
        //Set the HTTP methods that these origins are allowed to send. 
        p.WithMethods("GET");
    });
});

builder.Services.AddScoped<ExampleMidlewareClass>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
   
}

app.UseHttpsRedirection();

app.UseCors("YouTubePolicy");

app.UseAuthorization();

app.Use(async(context,next) =>
{
    //into the api
    Console.WriteLine("First custom midelware on the way in");

    await next();

    //out of the api, to the client
    Console.WriteLine("First custom midelware on the way out");
});

app.MapControllers();

app.UseMiddleware<ExampleMidlewareClass>();

app.Run();
