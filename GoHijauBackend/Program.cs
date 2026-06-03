using FluentValidation;
using FluentValidation.AspNetCore;
using GoHijauBackend.Application.Hubs;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Interfaces.Services.ActivityLogs;
using GoHijauBackend.Application.MappingProfile;
using GoHijauBackend.Application.Services;
using GoHijauBackend.Application.Services.ActivityLogs;
using GoHijauBackend.Application.Validators;
using GoHijauBackend.Background;
using GoHijauBackend.Infrastructure;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure();
builder.Services.AddScoped<RegisterUserService>();
builder.Services.AddScoped<LoginUserService>();
builder.Services.AddScoped<ManageUserRoleService>();
builder.Services.AddScoped<TransactionService>(); 
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IMachineAuditService, MachineAuditService>();
builder.Services.AddScoped<IManMachineService, ManMachineService>();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IRateService, RateService>();
builder.Services.AddScoped<QrTokenService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IWithdrawalHistoryService, WithdrawalHistoryService>();
builder.Services.AddScoped<IWithdrawalNotifierService, WithdrawalNotifierService>();
builder.Services.AddScoped<IPayoutService, PayoutService>();
builder.Services.AddScoped<IRazorPayService, RazorPayService>();
builder.Services.AddScoped<IMachineUCOTrackingService, MachineUCOTrackingService>();
builder.Services.AddScoped<ICollectorBillingService, CollectorBillingService>();
builder.Services.AddScoped<ISecretService, SecretService>();
builder.Services.AddScoped<IMachineOwnerPayoutService, MachineOwnerPayoutService>();
builder.Services.AddScoped<IOwnerWithdrawawalHistoryService, MachineOwnerWithdrawalHistoryService>();
builder.Services.AddScoped<IDashboardAnalyticsService, DashboardAnalyticsService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>(); 
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<ICollectorUCOReminder, CollectorUCOReminderService>();
builder.Services.AddScoped<IMachineHistoryService, MachineHistoryService>();
builder.Services.AddScoped<IMachineOwnerProfitDebtLogService, MachineOwnerProfitDebtLogService>();
builder.Services.AddScoped<ICollectionPostProcessService, CollectionPostProcessService>();
builder.Services.AddSingleton<LiveMachineCache>();
builder.Services.AddScoped<IPhysicalCheckReportRepository, MongoPhysicalCheckReportRepository>();
builder.Services.AddSingleton<ICollectionPostProcessQueue, CollectionPostProcessQueue>();
builder.Services.AddHostedService<CollectionPostProcessWorker>();
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.AddHttpClient();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
var mongoConn = builder.Configuration["MongoDB:ConnectionString"];
var mongoDb = builder.Configuration["MongoDB:Database"];
var options = new MongoStorageOptions
{
    MigrationOptions = new MongoMigrationOptions
    {
        MigrationStrategy = new MigrateMongoMigrationStrategy(),
        BackupStrategy = new CollectionMongoBackupStrategy()
    }
};

builder.Services.AddHangfire(config =>
    config.UseMongoStorage(mongoConn, mongoDb, options)
);

builder.Services.AddHangfireServer();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MachineCommandValidator>();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    // Add Application/Templates folder to Razor search paths
    options.ViewLocationFormats.Add("/Templates/{0}.cshtml");
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UcoMappingProfile>();
});
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(options =>
    {
        // Add GoHijauBackend.Application/Templates to Razor view search paths
        var templatePath = Path.Combine(
            builder.Environment.ContentRootPath,
            "..",
            "GoHijauBackend.Application",
            "Templates"
        );

        options.FileProviders.Add(new PhysicalFileProvider(templatePath));
    });
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(7192);
    });
}
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

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        ),
        ClockSkew = TimeSpan.Zero
        //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Skip the default behavior
            context.HandleResponse();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\": \"Authentication failed. Please provide a valid token.\"}");
            }

            return Task.CompletedTask;
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\": \"User not authorized to access this resource.\"}");
        },

        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\": \"Token has expired.\"}");
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\": \"Authentication failed.\"}");
        }
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<QRHub>("/qrHub");
app.MapHub<MachineHub>("/machineHub");
app.MapHub<WithdrawalHub>("/withdrawalHub");
app.UseHangfireDashboard("/hangfire");

app.Run();
