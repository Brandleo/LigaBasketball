using Microsoft.EntityFrameworkCore;
using BasketballLeagueApp.Models;
using Microsoft.OpenApi.Models;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
// Configuración de Entity Framework
builder.Services.AddDbContext<LigaBaloncestoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LigaConnection")));

// Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Liga Baloncesto API", Version = "v1" });
});

// Configuración de controladores y vistas
builder.Services.AddControllersWithViews();

//Login
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".BasketballLeagueApp.Session";
});

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Liga Baloncesto API v1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
