using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoCanvas.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the repository service
builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddScoped<IRepositorioRoles, RepositorioRoles>();
builder.Services.AddScoped<IRepositorioCursos, RepositorioCursos>();
builder.Services.AddScoped<IRepositorioAsignaciones, RepositorioAsignaciones>();
builder.Services.AddScoped<IRepositorioAsistencias, RepositorioAsistencia>();
builder.Services.AddScoped<IRepositorioAnuncios, RepositorioAnuncios>();
builder.Services.AddScoped<IRepositorioPersonas, RepositorioPersona>();
builder.Services.AddScoped<IRepositorioGrupos, RepositorioGrupos>();

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
