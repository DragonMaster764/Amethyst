using Amethyst.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Amethyst.Services;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Google;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();

// Register MongoDB client from configuration so IMongoClient can be injected
//var mongoSettings = builder.Configuration.GetSection("MongoDBSettings");
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? throw new InvalidOperationException("MongoDBSettings:ConnectionString is not configured.");
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

builder.Services.AddSingleton<MongoDBServices>();

builder.Services.AddHttpClient<AIService>();

builder.Services.AddSession();
builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();

//Youtube music stuff
app.UseSession();
app.MapControllers();

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

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}

app.Run();
