//using Amethyst.Data;
using Amethyst.Data;
using Amethyst.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Identity route mappings
    options.Conventions.AddPageRoute("/Identity/Account/Login", "/Account/Login");
    options.Conventions.AddPageRoute("/Identity/Account/Register", "/Account/Register");
    options.Conventions.AddPageRoute("/Identity/Account/Logout", "/Account/Logout");

    options.Conventions.AddPageRoute("/Identity/Account/ConfirmEmail", "/Account/ConfirmEmail");
    options.Conventions.AddPageRoute("/Identity/Account/RegisterConfirmation", "/Account/RegisterConfirmation");

    options.Conventions.AddPageRoute("/Identity/Account/ForgotPassword", "/Account/ForgotPassword");
    options.Conventions.AddPageRoute("/Identity/Account/ForgotPasswordConfirmation", "/Account/ForgotPasswordConfirmation");
    options.Conventions.AddPageRoute("/Identity/Account/ResetPassword", "/Account/ResetPassword");
    options.Conventions.AddPageRoute("/Identity/Account/ResetPasswordConfirmation", "/Account/ResetPasswordConfirmation");

    options.Conventions.AddPageRoute("/Identity/Account/AccessDenied", "/Account/AccessDenied");

    options.Conventions.AddPageRoute("/Identity/Account/Manage/Index", "/Account/Manage");
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContext")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoDBServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}

app.Run();

