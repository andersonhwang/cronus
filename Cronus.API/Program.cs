/****************************************************
 *         Cronus.API - ESL Gen3 Middleware         *
 **************************************************** 
 * File:    Program
 * Date:    06/06/2022
 * Author:  Huang Hai Peng
 * Summary: This class is the main class of Cronus.API
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/

// Cronus SendServer
using Cronus;
using Serilog;

var config = new CronusConfig { };
var log = LoggerFactory.Create(builder => builder.AddSerilog()).CreateLogger("Cronus");
var result = SendServer.Instance.Start(config, log);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("/test", () => { return "OK"; }).WithName("CronusAPITest");
app.Run();