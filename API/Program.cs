using System;
using Common.Enums;
using Common.Interfaces;
using DataLayer.MsSql;
using DataLayer.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var dbTypeString = configuration.GetValue<string>("DbType") ?? "MSSQL";
var dbType = Enum.TryParse<DbType>(dbTypeString, true, out var parsed) ? parsed : DbType.MSSQL;

switch (dbType)
{
    case DbType.MSSQL:
        var mssqlConn = configuration.GetConnectionString("MSSQL");
        builder.Services.AddSingleton<IDatabaseFactory>(new MsSqlDatabaseFactory(mssqlConn!));
        break;
    case DbType.MySQL:
        var mysqlConn = configuration.GetConnectionString("MySQL");
        builder.Services.AddSingleton<IDatabaseFactory>(new MySqlDatabaseFactory(mysqlConn!));
        break;
}

builder.Services.AddScoped<IUsersManager, BusinessLogic.UsersManager>();
builder.Services.AddScoped<IUsersService, Services.UsersService>();

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
app.MapControllers();

app.Run();
