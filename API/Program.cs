using Common;
using Common.Auth;
using Common.Enums;
using Common.Interfaces;
using DataLayer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Auth;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT options from appsettings
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Add JWT token service and password hasher (from Common layer)
builder.Services.AddJwtTokenService();

// Add authentication services (from Services layer)
builder.Services.AddAuthServices();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var dbTypeString = configuration.GetValue<string>("DbType") ?? "MSSQL";
    var dbType = Enum.TryParse<DbType>(dbTypeString, true, out var parsed) ? parsed : DbType.MSSQL;
    var connectionString = dbType == DbType.MSSQL
        ? configuration.GetConnectionString("MSSQL")
        : configuration.GetConnectionString("MySQL");
    return new ConnectionContext { DbType = dbType, ConnectionString = connectionString! };
});

builder.Services.AddScoped<IDatabaseFactory, DatabaseFactory>();

builder.Services.AddScoped<IUsersManager, DAL.UsersDbManager>();
builder.Services.AddScoped<IUsersService, Services.UsersService>();

builder.Services.AddAutoMapper(cfg => { }, typeof(Services.Mappers.UserMappingProfile).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Register middleware for JWT authentication (extracts and validates tokens)
app.UseMiddleware<API.Middleware.JwtAuthenticationMiddleware>();

app.MapControllers();

app.Run();
