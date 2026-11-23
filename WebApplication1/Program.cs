using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebApplication1.Data;



Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
// ---- Secret management ----
// Order of precedence (later wins): appsettings -> user-secrets (dev) -> env vars -> Azure Key Vault.
// Never put SMTP/DB/admin passwords in appsettings*.json.  Key Vault secret names use "--" (e.g. Email--Password).
var kvUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(kvUri))
    builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());

// ---- Serilog ----
builder.Host.UseSerilog((ctx, services, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("App", "AuthSystem"));


// ---- Database ----
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
