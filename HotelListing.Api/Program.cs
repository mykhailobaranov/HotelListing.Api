using Asp.Versioning;
using HotelListing.Api.CachePolicies;
using HotelListing.Api.Data;
using HotelListing.Api.MappingProfiles;
using HotelListing.Api.Middleware;
using HotelListing.Api.Models.Config;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Implementation;
using HotelListing.Api.Repositories.Interface;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting HotelListing API");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");

    builder.Services.AddDbContext<HotelListingDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);

            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        });
    });

    builder.Services.AddControllers()
        .AddNewtonsoftJson()
        .AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    builder.Services.AddIdentityCore<ApplicationUser>(options => { })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<HotelListingDbContext>();

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<HotelListingDbContext>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            b => b.AllowAnyHeader()
                  .AllowAnyOrigin()
                  .AllowAnyMethod());
    });

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

    if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
    {
        Log.Fatal("JwtSettings configuration is missing or invalid.");
        throw new InvalidOperationException("JwtSettings configuration is missing or invalid.");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthorization();
    builder.Services.AddOpenApi();

    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
    builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
    builder.Services.AddScoped<IBookingsRepository, BookingRepository>();
    builder.Services.AddScoped<IHotelsService, HotelsService>();
    builder.Services.AddScoped<ICountriesService, CountriesService>();
    builder.Services.AddScoped<IBookingsService, BookingService>();
    builder.Services.AddScoped<IUsersService, UsersService>();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    });

    builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy("AuthenticatedUserCachingPolicy", options =>
        {
            options.AddPolicy<AuthenticatedUserCachingPolicy>()
            .SetCacheKeyPrefix("authenticated-user-cache-");
        }, true);
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 50;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });

        options.AddPolicy("perUser", context =>
        {
            var username = context.User?.Identity?.Name ?? "anonymous";

            return RateLimitPartition.GetSlidingWindowLimiter(username, _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 50,
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 3
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = retryAfter.TotalSeconds
            }, cancellationToken: cancellationToken);
        };
    });

    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Hotel Listing API",
            Description = "API for managing hotels, countries, and bookings",
            Contact = new OpenApiContact
            {
                Name = "Support Team",
                Email = "support@hotellisting.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        options.SwaggerDoc("v2", new OpenApiInfo
        {
            Version = "v2",
            Title = "Hotel Listing API V2",
            Description = "Version 2 of the Hotel Listing API with enhanced features"
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        options.EnableAnnotations();

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.OperationFilter<HotelListing.Api.Filters.SecurityRequirementsOperationFilter>();

        options.OrderActionsBy(apiDesc => $"{apiDesc.RelativePath}_{apiDesc.HttpMethod}");
    });

    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? "anonymous");
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                diagnosticContext.Set("UserId", userId ?? "unknown");
            }
        };

        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null || httpContext.Response.StatusCode >= 500)
            {
                return Serilog.Events.LogEventLevel.Error;
            }
            else if (httpContext.Response.StatusCode >= 400)
            {
                return Serilog.Events.LogEventLevel.Warning;
            }
            return Serilog.Events.LogEventLevel.Information;
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Listing API V1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "Hotel Listing API V2");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Hotel Listing API Documentation";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableValidator();
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthentication();

    app.UseRateLimiter();

    app.UseAuthorization();

    app.UseOutputCache();

    app.MapControllers();

    app.MapHealthChecks("/healthz");
    app.MapHealthChecks("/healthz/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    Log.Information("HotelListing API is running");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}