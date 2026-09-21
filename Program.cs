using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pasteleria.Data;
using Pasteleria.Interfaces;
using Pasteleria.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => //Le enseñamos a swagger a comprobar tokens, es decir, utilizar autenticación 
{
    // Le decimos a Swagger que vamos a usar Tokens Bearer (JWT)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Escribe 'Bearer' [espacio] y luego tu token JWT. Ejemplo: 'Bearer eyJhbG...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(opciones => //CORS (CROSS ORIGIN RESOURCE SHARING) es una medida de seguridad de los navegadores para evitar peticiones de un puerto distinto al de la pagina
{
    opciones.AddPolicy("PermitirReact", app =>
    {
        app.AllowAnyOrigin() //Cuaalquierorigen permite que cualquier página web se conecte 
        .AllowAnyHeader() //Permite cualquier tipo de dato
        .AllowAnyMethod(); //Permite cualquier método (GET, POST, PUT, DELETE)
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //JWT Se utiliza para asignar credenciales a cada cliente
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            // Le decimos de dónde sacar la llave secreta que pusimos en el appsettings
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddDbContext<PasteleriaContext>(opciones =>
{
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionPasteleria")); //"ConexionPasteleria es la variable de entorno de appsettings.json
});

// Registrar el servicio en el contenedor de Inyección de Dependencias
builder.Services.AddScoped<IPostreService, PostreService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirReact"); //Añadir/Cambiar Cors es un paso importante para poder conectar puertos distintos, es decir,
//conectar nuestro Backend(ASP.NET) con nuestro Frontend(React)

app.UseAuthentication(); //Valida la credencial de JWT

app.UseAuthorization();

app.MapControllers();

app.Run();
