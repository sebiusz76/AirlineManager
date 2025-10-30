using AirlineManager.DataAccess.Data;
using AirlineManager.Middleware;
using AirlineManager.Models.Domain;
using AirlineManager.Services.Implementations;
using AirlineManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.IO.Compression;

// Configure Serilog before building the application
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

var columnOptions = new ColumnOptions
{
    // Disable all default columns
    AdditionalColumns = new List<Serilog.Sinks.MSSqlServer.SqlColumn>
    {
        new Serilog.Sinks.MSSqlServer.SqlColumn { ColumnName = "LogEvent", DataType = System.Data.SqlDbType.NVarChar, DataLength = -1, AllowNull = true }
  }
};

// Configure column mappings to match our ApplicationLog model
columnOptions.Store.Remove(StandardColumn.Properties);
columnOptions.Store.Remove(StandardColumn.MessageTemplate);
columnOptions.Store.Remove(StandardColumn.LogEvent);

// Map Serilog columns to our model columns
columnOptions.Id.ColumnName = "Id";
columnOptions.TimeStamp.ColumnName = "Timestamp";
columnOptions.Level.ColumnName = "Level";
columnOptions.Message.ColumnName = "Message";
columnOptions.Exception.ColumnName = "Exception";

// Set column data types
columnOptions.Level.StoreAsEnum = false;
columnOptions.TimeStamp.ConvertToUtc = true;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.MSSqlServer(
    connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "ApplicationLogs",
            SchemaName = "dbo",
            AutoCreateSqlTable = false,
            BatchPostingLimit = 50,
            BatchPeriod = TimeSpan.FromSeconds(5)
        },
        columnOptions: columnOptions)
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add database connection
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
      )
    );

    // Add Identity services
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password options will be dynamically configured from database
        options.Password.RequireDigit = true; // Default fallback
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        options.SignIn.RequireConfirmedEmail = true; // Require email confirmation
        options.SignIn.RequireConfirmedAccount = true; // Require confirmed account

        // Lockout settings - will be dynamically configured from database
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
      .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Configure cookie settings
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

    // Register role hierarchy and authorization policies
    var roleOrder = new[] { "User", "Moderator", "Admin", "SuperAdmin" };

    builder.Services.AddAuthorization(options =>
    {
        // helper to add policy for minimum role
        void AddMinimumRolePolicy(string policyName, string minimumRole)
        {
            options.AddPolicy(policyName, policy =>
                  {
                      policy.RequireAssertion(context =>
           {
               var user = context.User;
               var minIndex = Array.IndexOf(roleOrder, minimumRole);
               if (minIndex < 0) return false;
               for (var i = minIndex; i < roleOrder.Length; i++)
               {
                   if (user.IsInRole(roleOrder[i])) return true;
               }
               return false;
           });
                  });
        }

        AddMinimumRolePolicy("AtLeastUser", "User");
        AddMinimumRolePolicy("AtLeastModerator", "Moderator");
        AddMinimumRolePolicy("AtLeastAdmin", "Admin");
        AddMinimumRolePolicy("AtLeastSuperAdmin", "SuperAdmin");
    });

    // Compression service registration
    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        // Compress common text-based MIME types
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
    });

    // Configuring provider settings
    builder.Services.Configure<BrotliCompressionProviderOptions>(o =>
    {
        o.Level = CompressionLevel.Fastest;
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(o =>
    {
        o.Level = CompressionLevel.Fastest;
    });

    // Add services to the container.
    builder.Services.AddControllersWithViews()
#if DEBUG
        .AddRazorRuntimeCompilation()
#endif
      ;

    // Add session support
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

    // Register Configuration Service
    builder.Services.AddScoped<AirlineManager.Services.IConfigurationService, AirlineManager.Services.ConfigurationService>();

    // Register Email Service
    builder.Services.AddScoped<AirlineManager.Services.IEmailService, AirlineManager.Services.EmailService>();

    // Register Login History Service
    builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();

    // Register Session Management Service
    builder.Services.AddScoped<ISessionManagementService, SessionManagementService>();

    // Register Password Expiration Service
    builder.Services.AddScoped<IPasswordExpirationService, PasswordExpirationService>();

    // Register Account Lockout Service
    builder.Services.AddScoped<IAccountLockoutService, AccountLockoutService>();

    // Register Password Policy Service
    builder.Services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();

    // Register Data Retention Service
    builder.Services.AddScoped<IDataRetentionService, DataRetentionService>();

    // Register Theme Service
    builder.Services.AddScoped<IThemeService, ThemeService>();

    // Register Lockout Options Updater
    builder.Services.AddHostedService<AirlineManager.Middleware.LockoutOptionsUpdater>();

    // Register Password Options Updater
    builder.Services.AddHostedService<AirlineManager.Middleware.PasswordOptionsUpdater>();

    // Register Session Cleanup Background Service
    builder.Services.AddHostedService<AirlineManager.Services.Background.SessionCleanupService>();

    // Register Data Retention Cleanup Background Service
    builder.Services.AddHostedService<AirlineManager.Services.Background.DataRetentionCleanupService>();

    var app = builder.Build();

    // Apply any pending migrations at startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        // Test log after migration
        Log.Information("Database migrations applied successfully");
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Add Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
              {
                  diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anonymous");
                  diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
              };
    });

    app.UseHttpsRedirection();

    app.UseResponseCompression();

    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
        }
    });

    app.UseRouting();

    // Add session middleware
    app.UseSession();

    // Add authentication and authorization
    app.UseAuthentication();

    // Update session activity
    app.UseMiddleware<SessionActivityMiddleware>();

    // Maintenance mode middleware (before authorization)
    app.UseMiddleware<MaintenanceModeMiddleware>();

    // Middleware to force password change
    app.UseMiddleware<RequirePasswordChangeMiddleware>();

    app.UseAuthorization();

    // Initialize roles and SuperAdmin
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Create roles if they don't exist
        string[] roles = { "User", "Moderator", "Admin", "SuperAdmin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Log.Information("Created role: {Role}", role);
            }
        }

        // Create a SuperAdmin if it doesn't exist (you'll want to secure this in production)
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Admin"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                Log.Information("Created SuperAdmin user: {Email}", adminEmail);
            }
            else
            {
                Log.Error("Failed to create SuperAdmin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    app.MapControllerRoute(
        name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    Log.Information("Application started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}