using FFMP.BlobStorageServices;
using FFMP.Data;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = "Server=project-3-mysql-server.mysql.database.azure.com;User ID=stremberg_s;Password=1Database1;Database=project_3;";
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<project_3Context>(options =>
options.UseMySql(connectionString, serverVersion));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache(); // For session
builder.Services.AddSession(); // For session
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // For session

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();  // For session

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Login}/{id?}");

app.Run();
