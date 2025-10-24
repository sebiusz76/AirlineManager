using AirlineManager.DataAccess.Data;
using AirlineManager.Middleware;
using AirlineManager.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlServer(
 builder.Configuration.GetConnectionString("DefaultConnection")
 )
);

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
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

var app = builder.Build();

// Apply any pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
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

// Add authentication and authorization
app.UseAuthentication();

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

app.Run();