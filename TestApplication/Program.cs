using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using TestApplication.DataAccessLayer;
using TestApplication.DataAccessLayer.Enums;
using TestApplication.DataAccessLayer.Interfaces;
using TestApplication.MappingProfile;
using TestApplication.Middlewares.ExceptionHandler;
using TestApplication.Services;

var builder = WebApplication.CreateBuilder(args);

// Add builder.Services to the container.

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.SuppressMapClientErrors = true;
});
builder.Services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Authorize",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http
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
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration.GetSection("Jwt").GetValue<string>("Issuer"),
        ValidateIssuer = true,
        ValidAudience = builder.Configuration.GetSection("Jwt").GetValue<string>("Audience"),
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("Jwt").GetValue<string>("Key"))),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Role.None.ToString(), policy => policy.RequireClaim("UserRole", Role.None.ToString()));
    options.AddPolicy(Role.Admin.ToString(), policy => policy.RequireClaim("UserRole", Role.Admin.ToString()));
});

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
