using Parkeringsplads.Models;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Interfaces;
using Parkeringsplads.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register DbContext
builder.Services.AddDbContext<TestParkeringspladsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Simply")));

// Register the CityService (ICityService) with dependency injection
builder.Services.AddScoped<ICityService, CityService>();

// Add logging services (this is where you add logging)
builder.Services.AddLogging();  // Adds default logging services

// Add services to the container.
builder.Services.AddRazorPages();

// Add session service with automatic idle logout
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Session lasts 30 minutes of inactivity
    options.Cookie.HttpOnly = true;                  // Increases security (not accessible via JavaScript)
    options.Cookie.IsEssential = true;               // Required if user hasn't consented to cookies
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Add session middleware support
app.UseSession();

app.MapRazorPages();

app.Run();
