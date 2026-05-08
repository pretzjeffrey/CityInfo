using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Mvc.Formatters; // Add this using directive
using Microsoft.Extensions.DependencyInjection; // Add this using directive
using Newtonsoft.Json; // Add this using directive
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Serilog;
using CityInfo.Services;
using CityInfo.Models;
using CityInfo.DbContexts;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.ApplicationInsights.Extensibility; // Add this using directive

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == Environments.Development)
{
    builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
        .MinimumLevel.Debug()
        .WriteTo.Console());
}
else
{
    builder.Host.UseSerilog((context, LoggerConfiguration) =>
    LoggerConfiguration.MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(
        new TelemetryConfiguration()
        {
            InstrumentationKey = builder.Configuration["ApplicationInsightsInstrumentationKey"]
        },
        TelemetryConverter.Traces));
}

    // Add services to the container.
    builder.Host.UseSerilog();
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson() // Fix: Correct method name and ensure using directive is present
.AddXmlDataContractSerializerFormatters();  // Add support for XML format (in addition to JSON which is supported by default)

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// customize the problem details object
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {  // Customize mapping for specific exceptions (use controller based exception filters for more control)
        ctx.ProblemDetails.Extensions.Add("additionalInfo", "This is some additional information about the error.");
        ctx.ProblemDetails.Extensions.Add("server", Environment.MachineName);   
    };
});

//builder.Services.AddSingleton<FileExtensionContentTypeProvider>(); // Singleton lifetime is appropriate for services that are thread-safe and can be shared across the entire application. FileExtensionContentTypeProvider is a service that provides content type information based on file extensions, and it doesn't maintain any state or require any resources that would necessitate creating multiple instances.
                                                                   // Using a singleton lifetime allows for better performance and resource utilization by sharing a single instance of the service throughout the application.

builder.Services.AddTransient<IMailService, LocalMailService>(); // Transient lifetime is appropriate for lightweight, stateless services. LocalMailService is a simple service that doesn't maintain any state and is not resource-intensive,
                                                                 // so using a transient lifetime allows for better performance and scalability by creating a new instance each time it's requested.

// builder.Services.AddScoped<Services.LocalMailService>(); // Scoped lifetime is appropriate for services that need to maintain state within a single request but should be shared across multiple components during that request.
// LocalMailService doesn't maintain any state and is not resource-intensive,
// so using a scoped lifetime would not provide any benefits in this case and could lead to unnecessary overhead by creating a new instance for each request.
// Once we add Entity Framework Core and a database context, we will need to use a scoped lifetime for the database context,
// as it needs to maintain state within a single request and should be shared across multiple components during that request.

#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<CitiesDataStore>();

builder.Services.AddDbContext<CityInfoContext>(DbContextOptions
        => DbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"])
        ); // scoped lifetime is appropriate for database contexts in Entity Framework Core, as they need to maintain state within a single request and should be shared across multiple components during that request.
           // Using a scoped lifetime ensures that the same instance of the database context is used throughout the request, which allows for efficient management of database connections and transactions.
           // It also helps to prevent issues related to concurrent access and ensures that changes made to the context are properly tracked and saved at the end of the request.

builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>(); // Scoped lifetime is appropriate for services that need to maintain state within a single request but should be shared across multiple components during that request.
                                                                       // CityInfoRepository is a service that interacts with the database context and may need to maintain state during a request,
                                                                       // so using a scoped lifetime allows for better performance and resource management by sharing a single instance of the service throughout the request.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); // AutoMapper is a library that helps to map objects of one type to another.
                                                                         // It can be used to simplify the process of mapping between data transfer objects (DTOs) and domain models in an application.

builder.Services.AddAuthentication("Bearer") // This adds authentication services to the application and specifies that the default authentication scheme is "Bearer".
                                             // This means that the application will expect incoming requests to include a bearer token for authentication.
    .AddJwtBearer(options => // This adds JWT bearer authentication to the application and allows you to configure the options for JWT authentication.
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters // This sets the token validation parameters for JWT authentication.
                                                                                                         // These parameters specify how the JWT tokens should be validated when they are received in incoming requests.
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromAntwerp", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Antwerp");
    });
});

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
}).AddMvc()
  .AddApiExplorer(setupAction =>
    {
        setupAction.GroupNameFormat = "'v'VVV";
        setupAction.SubstituteApiVersionInUrl = true;
    });

var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
    .GetRequiredService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(setupAction =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        setupAction.SwaggerDoc(
            $"{description.GroupName}",
            new()
            {
                Title = "City Info API",
                Version = description.ApiVersion.ToString(),
                Description = "Through this API you can access cities and their points of interest. You can also add points of interest and upload files."
            });
    }

    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var CommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(CommentsFullPath);

    setupAction.AddSecurityDefinition("CityInfoApiBearerAuth", new()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access this API"
    });

    setupAction.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CityInfoApiBearerAuth"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options => // for Azure proxies
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
    | ForwardedHeaders.XForwardedProto;
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseForwardedHeaders(); 

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(setupAction =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            setupAction.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
//}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication(); // This adds authentication middleware to the application, which enables the authentication process for incoming requests.
                         // It should be placed before the authorization middleware to ensure that the user is authenticated before any authorization checks are performed.

app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
