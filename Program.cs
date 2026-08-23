using Microsoft.EntityFrameworkCore;
using KickFive.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using KickFive.Services;
using KickFive.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("KickFiveContext") ?? throw new InvalidOperationException("Connection string 'KickFiveContext' not found.");

builder.Services.AddDbContext<KickFiveContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<KickFiveContext>().AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
// Add services to the container.
builder.Services.AddControllersWithViews().AddViewLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[] { "en", "fr", "nl" };
        options.SetDefaultCulture(supportedCultures[0]).AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures);
    }
);

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.AllowedForNewUsers = true;
});


var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var localizationOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{

    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }


    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    var adminEmail = builder.Configuration["AdminUser:Email"];
    var adminPassword = builder.Configuration["AdminUser:Password"];


    if (adminEmail != null && adminPassword != null)
    {
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {

            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");

            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"User creation error: {error.Code} - {error.Description}");
                }
            }

        }
    }
    var context = scope.ServiceProvider.GetRequiredService<KickFiveContext>();
    await context.Database.MigrateAsync();

    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var seeder = new DbSeeder(userManager, configuration);
    await seeder.SeedAsync(context);
}
app.Run();
