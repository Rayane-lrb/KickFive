using Microsoft.EntityFrameworkCore;
using KickFive.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("KickFiveContext") ?? throw new InvalidOperationException("Connection string 'KickFiveContext' not found.");

builder.Services.AddDbContext<KickFiveContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<KickFiveContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
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

    Console.WriteLine("SEEDING BLOCK STARTED");
    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    Console.WriteLine("Got roleManager");

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            Console.WriteLine($"Creating role: {role}");
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    var adminEmail = builder.Configuration["AdminUser:Email"];
    var adminPassword = builder.Configuration["AdminUser:Password"];

    Console.WriteLine($"Admin email from config: '{adminEmail}'");
    Console.WriteLine($"Admin password from config: '{adminPassword}'");

    if (adminEmail != null && adminPassword != null)
    {
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        Console.WriteLine($"Existing admin found: {existingAdmin != null}");
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
            Console.WriteLine($"User creation succeeded: {result.Succeeded}");
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
}
app.Run();
