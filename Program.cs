using Med_Map.Filters;
using Med_Map.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers(options =>{ options.Filters.Add<ValidateModelAttribute>();})
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
        builder.Services.AddDbContext<Mm_Context>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), x => x.UseNetTopologySuite());
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => {
            c.SupportNonNullableReferenceTypes();
            c.UseAllOfToExtendReferenceSchemas();
            c.OperationFilter<Med_Map.Filters.MultipleResponseTypesOperationFilter>();
        });
       
        #region repos registration
        builder.Services.AddScoped<IOtpRepository, OtpRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IPharmacyRepository, PharmacyRepository>();
        builder.Services.AddScoped<IPharmacyInventoryRepository, PharmacyInventoryRepository>();
        builder.Services.AddScoped<IPharmacyServiceRepository, PharmacyServiceRepository>();
        builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IWalletRepository, WalletRepository>();
        builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        #endregion
        #region service registration
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IOtpService, OtpService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IKashierService, KashierService>();
        builder.Services.AddHostedService<PendingOrderExpiryService>();
        builder.Services.AddHttpClient<IAiService, AiService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["AiService:BaseUrl"]!);
        });
        #endregion
        builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<Mm_Context>()
          .AddDefaultTokenProviders();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidAudience = builder.Configuration["JWT:ValidAudience"],
                ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecurityKey"]))
            };
            // SignalR WebSocket handshakes can't send headers — read the token from the query string instead
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(token) &&
                        context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        context.Token = token;
                    return Task.CompletedTask;
                },
                // A validly-signed, unexpired token can still have been logged out — check the
                // server-side UserSession record (set inactive by AccountController.logout) so
                // logout actually revokes access instead of only being a no-op DB write.
                OnTokenValidated = async context =>
                {
                    var sidClaim = context.Principal?.FindFirstValue("sid");
                    if (!Guid.TryParse(sidClaim, out var sessionId))
                    {
                        context.Fail("Token missing a valid session id.");
                        return;
                    }
                    var sessionRepository = context.HttpContext.RequestServices.GetRequiredService<ISessionRepository>();
                    var session = await sessionRepository.FindByIdAsync(sessionId);
                    if (session == null || !session.IsActive)
                        context.Fail("Session has been logged out. Please log in again.");
                }
            };
        });
        builder.Services.AddAuthorization();
        // "auth" policy: per-client-IP fixed window, applied to AccountController (login/register/OTP/
        // password-reset) — deliberately generous for now, see Constants/Constant.cs to tighten later.
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Constant.AuthRateLimitPermits,
                    Window = TimeSpan.FromSeconds(Constant.AuthRateLimitWindowSeconds),
                    QueueLimit = Constant.AuthRateLimitQueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));
        });
        builder.Services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options => options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json");
        }
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();
        app.MapHub<NotificationHub>("/hubs/notifications");
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in RoleConstants.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            if (app.Environment.IsDevelopment() && Constant.IncludeSeeders)
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext   = scope.ServiceProvider.GetRequiredService<Mm_Context>();
                var env         = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                await AdminSeeder.SeedAsync(userManager);
                await MedicineSeeder.SeedAsync(dbContext, env);
                await CustomerSeeder.SeedAsync(dbContext, userManager);
                await PharmacySeeder.SeedAsync(dbContext, userManager, env);
            }
        }
        app.Run();
    }
}