/****************************************************
 *         Cronus.API - ESL Gen3 Middleware         *
 **************************************************** 
 * File:    Program
 * Date:    08/13/2022
 * Author:  Huang Hai Peng
 * Summary: This class is the main class of Cronus.API
 * (C) Suzhou ETAG Electronic Technology Co., Ltd
****************************************************/

// Cronus SendServer
using Cronus;
using Cronus.API;
using Cronus.Enum;
using Cronus.Events;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SkiaSharp;

var config = new CronusConfig { };
var log = LoggerFactory.Create(builder => builder.AddSerilog()).CreateLogger("Cronus");
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Middleware
SendServer.Instance.APEventHandler += Instance_APEventHandler;
SendServer.Instance.TaskEventHandler += Instance_TaskEventHandler;
var result = SendServer.Instance.Start(config, log);
log.LogInformation("Try to start send server: " + result);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
#region API #0. Test
app.MapGet("/test", () => { return "OK"; }).WithName("CronusAPITest");
#endregion

#region API #1. Push Image
app.MapPost("/pushImage", ([FromBody] ImageTask task) =>
{
    try
    {
        if (task is null) return Results.BadRequest("NULL_DATA");

        var bytes = Convert.FromBase64String(task.ImageBase64);
        using var memoryStream = new MemoryStream(bytes);
        var image = SKImage.FromEncodedData(memoryStream);
        var result = string.IsNullOrEmpty(task.StoreCode)
            ? SendServer.Instance.Push(task.TagID, image)
            : SendServer.Instance.Push(task.StoreCode, task.TagID, image);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushImageError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #2. Push Task
app.MapPost("/pushTask", ([FromBody] BasicTask task) =>
{
    try
    {
        if (task is null) return Results.BadRequest("NULL_DATA");

        var result = string.IsNullOrEmpty(task.StoreCode)
            ? SendServer.Instance.Push(task.TaskDatas)
            : SendServer.Instance.Push(task.StoreCode, task.TaskDatas);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushTaskError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #3. Push Task List
app.MapPost("/pushLed", ([FromBody] LedTask task) =>
{
    try
    {
        if (task is null) return Results.BadRequest("NULL_DATA");

        var result = string.IsNullOrEmpty(task.StoreCode)
            ? SendServer.Instance.LED(task.Red, task.Green, task.Blue, task.Times, task.TagIDList)
            : SendServer.Instance.LED(task.StoreCode, task.Red, task.Green, task.Blue, task.Times, task.TagIDList);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushLedError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #5. [Broadcast] Switch Page
app.MapGet("/switchPage", ([FromQuery(Name = "store")] string store, [FromQuery(Name = "page")] int page) =>
{
    try
    {
        if (string.IsNullOrEmpty(store)) return Results.BadRequest("NULL_DATA");
        if (page < 0 || page > 7) return Results.BadRequest("NULL_DATA");

        var result = SendServer.Instance.SwitchPage(store, page);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushLedError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #6. [Broadcast] Display Barcode
app.MapGet("/displayBarcode", ([FromQuery(Name="store")]string store) =>
{
    try
    {
        if (string.IsNullOrEmpty(store)) return Results.BadRequest("NULL_DATA");

        var result = SendServer.Instance.DisplayBarcode(store);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushLedError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

app.Run();


// AP event handler
void Instance_APEventHandler(object? sender, APStatusEventArgs e)
{
    // Here you can do DB update, API feedback and etc.
    log.LogInformation($"[APEventHandler]Store Code:{e.StoreCode},AP ID:{e.APID},Status:{e.Status}");
}

// Task event handler
void Instance_TaskEventHandler(object? sender, TaskResultEventArgs e)
{
    // Here you can do DB update, API feedback and etc.
    foreach(var result in e.TaskResults)
    {
        log.LogInformation(
            $"[TaskEventHandler]Tag ID:{result.TagID},Task ID:{result.TaskID},Last Send:{result.LastSendTime}," +
            $"Last Recieve:{result.LastRecvTime},Send Count:{result.SendCount},Status:{result.Status}," +
            $"RF Power:{result.RfPower},Battery:{result.Battery},Temperature:{result.Temperature}.");
    }
}