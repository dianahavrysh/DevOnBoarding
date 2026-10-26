using API.Middleware;
using Common.Auth;
using Common.Auth.Jwt;
using Common.Contexts;
using Common.Enums;
using Common.Interfaces;
using DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Services;
using Services.Auth;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
if (jwtOptions == null) {
    throw new InvalidOperationException("JWT configuration is missing from appsettings.json");
}

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateOnStart();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5002", "https://localhost:7002")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddJwtTokenService();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddAuthServices();

builder.Services.AddScoped<ConnectionContext>(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var dbTypeString = configuration.GetValue<string>("DbType") ?? "MSSQL";
    var dbType = Enum.TryParse<DbType>(dbTypeString, true, out var parsed) ? parsed : DbType.MSSQL;
    var connectionString = dbType == DbType.MSSQL
        ? configuration.GetConnectionString("MSSQL")
        : configuration.GetConnectionString("MySQL");
    return new ConnectionContext { DbType = dbType, ConnectionString = connectionString! };
});

builder.Services.AddScoped<IDatabaseFactory, DatabaseFactory>();

builder.Services.AddScoped<IUsersDbManager, DAL.UsersDbManager>();
builder.Services.AddUserServices(builder.Configuration);

builder.Services.AddScoped<CurrentUserContext>();
builder.Services.AddScoped<ICurrentUserContext>(sp => sp.GetRequiredService<CurrentUserContext>());

builder.Services.AddAutoMapper(cfg => { }, typeof(Services.Mappers.UserMappingProfile).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else {
    app.UseMiddleware<ExceptionLoggingMiddleware>();
}

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseMiddleware<CurrentUserMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
