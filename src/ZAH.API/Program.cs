using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ZAH.API.Middleware;
using ZAH.Application.Interfaces;
using ZAH.Infrastructure.MongoDB;
using ZAH.Infrastructure.Payments;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ZAH.API")
    .CreateLogger();

try
{
    Log.Information("Starting ZAH E-Commerce API");

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = Directory.GetCurrentDirectory(),
        // Disable file watching in production
        EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
    });

    // Disable file watching for configuration files
    builder.Configuration.Sources.Clear();
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>(optional: true);
    }

    builder.Configuration
        .AddEnvironmentVariables();

    // Add Serilog
    builder.Host.UseSerilog();

    // MongoDB Configuration
    var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
    if (string.IsNullOrWhiteSpace(mongoConnectionString) || mongoConnectionString.StartsWith("${"))
    {
        mongoConnectionString = builder.Configuration["MONGODB_CONNECTION_STRING"]
                             ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                             ?? Environment.GetEnvironmentVariable("MongoDB__ConnectionString")
                             ?? "mongodb+srv://zahgo:zahgo*51228@cluster0.fpiizwe.mongodb.net/?appName=Cluster0";
    }

    builder.Services.Configure<MongoDbSettings>(options =>
    {
        options.ConnectionString = mongoConnectionString ?? string.Empty;
        options.DatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "zahgo";
    });
    builder.Services.AddSingleton<MongoDbContext>();

    // Cashfree Gateway Configuration - Reads Render Env Variables in Production, appsettings in Development
    var cashfreeEnv = Environment.GetEnvironmentVariable("CASHFREE_ENVIRONMENT")
                   ?? Environment.GetEnvironmentVariable("Cashfree__Environment")
                   ?? builder.Configuration["Cashfree:Environment"]
                   ?? "sandbox";

    var isProductionEnv = cashfreeEnv.Equals("production", StringComparison.OrdinalIgnoreCase);

    var cashfreeBaseUrl = isProductionEnv
        ? "https://api.cashfree.com/pg"
        : (builder.Configuration["Cashfree:ApiBaseUrl"] ?? "https://sandbox.cashfree.com/pg");

    var cashfreeClientId = Environment.GetEnvironmentVariable("CASHFREE_CLIENT_ID")
                        ?? Environment.GetEnvironmentVariable("Cashfree__ClientId")
                        ?? builder.Configuration["Cashfree:ClientId"]
                        ?? string.Empty;

    var cashfreeClientSecret = Environment.GetEnvironmentVariable("CASHFREE_CLIENT_SECRET")
                            ?? Environment.GetEnvironmentVariable("Cashfree__ClientSecret")
                            ?? builder.Configuration["Cashfree:ClientSecret"]
                            ?? string.Empty;

    builder.Services.Configure<CashfreeOptions>(options =>
    {
        options.Environment = cashfreeEnv;
        options.ApiBaseUrl = cashfreeBaseUrl;
        options.ClientId = cashfreeClientId;
        options.ClientSecret = cashfreeClientSecret;
        options.ReturnUrl = builder.Configuration["Cashfree:ReturnUrl"] ?? "https://zahgo.com/payment/callback";
        options.WebhookUrl = builder.Configuration["Cashfree:WebhookUrl"] ?? "";
        options.ApiVersion = builder.Configuration["Cashfree:ApiVersion"] ?? "2023-08-01";
    });

    builder.Services.AddHttpClient<ICashfreePaymentClient, CashfreePaymentClient>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.IPaymentService, ZAH.Application.Services.PaymentService>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.ICouponRepository, ZAH.Infrastructure.Repositories.CouponRepository>();

    // JWT Configuration
    var jwtSecret = builder.Configuration["JWT:Secret"];
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.StartsWith("${"))
    {
        jwtSecret = builder.Configuration["JWT_SECRET"]
                 ?? Environment.GetEnvironmentVariable("JWT_SECRET")
                 ?? Environment.GetEnvironmentVariable("JWT__Secret");
    }

    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.StartsWith("${"))
    {
        jwtSecret = "ZAH_Ecommerce_Super_Secret_JWT_Signing_Key_2026_Minimum_32_Chars!";
    }

    var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "ZAH.API";
    var jwtAudience = builder.Configuration["JWT:Audience"] ?? "ZAH.Client";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // CORS Configuration
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ZAHCorsPolicy", policy =>
        {
            if (builder.Environment.IsDevelopment() || allowedOrigins.Length == 0)
            {
                // Allow all origins in development or when no origins configured
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                // Restrict to specific origins in production
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        });
    });

    // Add Controllers
    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    // Add Response Compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Health Checks
    builder.Services.AddHealthChecks();

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Register Application Services
    builder.Services.AddScoped<ZAH.Application.Interfaces.IProductService, ZAH.Application.Services.ProductService>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.ICategoryService, ZAH.Application.Services.CategoryService>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.IAuthService, ZAH.Application.Services.AuthService>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.IOtpService, ZAH.Application.Services.OtpService>();
    
    // Register SMS Service
    builder.Services.AddScoped<ZAH.Application.Services.ISmsService, ZAH.Application.Services.SmsService>();
    builder.Services.AddHttpClient<ZAH.Application.Services.ISmsService, ZAH.Application.Services.SmsService>();

    // Register Repositories
    builder.Services.AddScoped<ZAH.Application.Interfaces.IProductRepository, ZAH.Infrastructure.Repositories.ProductRepository>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.ICategoryRepository, ZAH.Infrastructure.Repositories.CategoryRepository>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.IUserRepository, ZAH.Infrastructure.Repositories.UserRepository>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.ICartRepository, ZAH.Infrastructure.Repositories.CartRepository>();
    builder.Services.AddScoped<ZAH.Application.Interfaces.IOrderRepository, ZAH.Infrastructure.Repositories.OrderRepository>();
    builder.Services.AddScoped<ZAH.Domain.Interfaces.IOtpRepository, ZAH.Infrastructure.Repositories.OtpRepository>();
    builder.Services.AddScoped<ZAH.Domain.Interfaces.IPasswordResetRepository, ZAH.Infrastructure.Repositories.PasswordResetRepository>();

    var app = builder.Build();

    // Configure HTTP Request Pipeline
    // Enable Swagger in all environments
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZAH E-Commerce API v1");
        c.RoutePrefix = "swagger";
    });

    // Global Exception Handling Middleware
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Serilog Request Logging
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseResponseCompression();

    // CORS
    app.UseCors("ZAHCorsPolicy");

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health");

    app.MapControllers();

    // Seed Database
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var seeder = new ZAH.Infrastructure.Seeding.DatabaseSeeder(
                scope.ServiceProvider.GetRequiredService<MongoDbContext>());
            await seeder.SeedAsync();
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database seeding failed on startup. Application will continue starting.");
    }

    Log.Information("ZAH API Started Successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
