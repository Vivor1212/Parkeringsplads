using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;
using Parkeringsplads.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register DbContext
builder.Services.AddDbContext<TestParkeringspladsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Simply")));

// Register the CityService (ICityService) with dependency injection
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ITripService, EFTripService>();

// Add logging services (this is where you add logging)
builder.Services.AddLogging();  // Adds default logging services

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<ITripService, EFTripService>();
builder.Services.AddDbContext<ParkeringspladsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Simply")));

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
