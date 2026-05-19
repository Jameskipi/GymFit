using GymFit.Data;
using GymFit.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SQLite database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddControllersWithViews();

builder.Services.AddSession();

var app = builder.Build();


// TEMP - delete
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.MembershipOffers.Any())
    {
        context.MembershipOffers.AddRange(
            new MembershipOffer
            {
                Name = "Basic",
                Price = 99,
                ValidityDays = 30,
                Description = "Access to gym equipment."
            },
            new MembershipOffer
            {
                Name = "Premium",
                Price = 179,
                ValidityDays = 30,
                Description = "Gym + group classes."
            },
            new MembershipOffer
            {
                Name = "VIP",
                Price = 299,
                ValidityDays = 60,
                Description = "Full access + trainer support."
            }
        );

        context.SaveChanges();
    }
}






// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
