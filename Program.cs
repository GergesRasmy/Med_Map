using Med_Map.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

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
        #endregion
        builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<Mm_Context>();
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
        });
        builder.Services.AddAuthorization();
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
        app.MapControllers();
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in RoleConstants.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            if (app.Environment.IsDevelopment())
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