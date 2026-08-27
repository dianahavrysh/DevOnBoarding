using Common;
using Common.Enums;
using Common.Interfaces;
using DataLayer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IUsersManager, BusinessLogic.UsersManager>();
builder.Services.AddScoped<IUsersService, Services.UsersService>();

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

app.UseRouting();
app.MapControllers();

app.Run();
