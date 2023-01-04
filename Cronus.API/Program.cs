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
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var config = new CronusConfig { };
var log = LoggerFactory.Create(builder => builder.AddSerilog()).CreateLogger("Cronus");
var builder = WebApplication.CreateBuilder(args);
var regTagID = new Regex("^[0-9A-F]{12}$");

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

app.Urls.Add("http://*:5000");
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

#region API #5. Switch Page Specific
app.MapGet("/switchPageSpecific", ([FromQuery(Name = "store")] string store, [FromQuery(Name = "page")] int page, [FromQuery(Name = "id")] string id) =>
{
    try
    {
        if (string.IsNullOrEmpty(store)) return Results.BadRequest("NULL_DATA");
        if (page < 0 || page > 7) return Results.BadRequest("INVALID_PAGE");
        if (!regTagID.IsMatch(id)) return Results.BadRequest("INVALID_TAG_ID");

        var result = SendServer.Instance.SwitchPage(store, id, page);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushLedError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #6. [Broadcast] Switch Page
app.MapGet("/switchPage", ([FromQuery(Name = "store")] string store, [FromQuery(Name = "page")] int page) =>
{
    try
    {
        if (string.IsNullOrEmpty(store)) return Results.BadRequest("NULL_DATA");
        if (page < 0 || page > 7) return Results.BadRequest("NULL_DATA");

        var result = SendServer.Instance.SwitchPageAll(store, page);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushLedError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #7. [Broadcast] Display Barcode
app.MapGet("/displayBarcode", ([FromQuery(Name = "store")] string store) =>
{
    try
    {
        if (string.IsNullOrEmpty(store)) return Results.BadRequest("NULL_DATA");

        var result = SendServer.Instance.DisplayBarcodeAll(store);

        return result == Result.OK ? Results.Ok() : Results.BadRequest(result.ToString());
    }
    catch (Exception ex)
    {
        log.LogError("PushLedError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #8. [Query] Query AP
app.MapGet("/queryAP", ([FromQuery(Name = "store")] string store, [FromQuery(Name = "ap")] string ap) =>
{
    try
    {
        if (string.IsNullOrEmpty(store) || string.IsNullOrEmpty(ap)) return Results.BadRequest("NULL_DATA");

        var result = SendServer.Instance.GetAPByID(store, ap);

        return result == null ? Results.NotFound() : Results.Ok(result);
    }
    catch (Exception ex)
    {
        log.LogError("PushLedError", ex);
        return Results.StatusCode(500);
    }
});
#endregion

#region API #9. [Query] Query Tag
app.MapGet("/queryTag", ([FromQuery(Name = "id")] string id) =>
{
    try
    {
        if (string.IsNullOrEmpty(id)) return Results.BadRequest("NULL_DATA");

        var tag = SendServer.Instance.GetTagByID(id);

        return tag == null ? Results.NotFound() : Results.Ok(tag);
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
    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}][APEventHandler]Store Code:{e.StoreCode},AP ID:{e.APID},Status:{e.Status}");
}

// Task event handler
void Instance_TaskEventHandler(object? sender, TaskResultEventArgs e)
{
    // Here you can do DB update, API feedback and etc.
    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]AP {e.TaskResults} Recieved:({e.TaskResults.Count})");
    foreach (var result in e.TaskResults)
    {
        Console.WriteLine(
            $"[{DateTime.Now.ToShortTimeString()}][TaskEventHandler]Tag ID:{result.TagID},Task ID:{result.TaskID},Last Send:{result.LastSendTime}," +
            $"Last Recieve:{result.LastRecvTime},Send Count:{result.SendCount},Status:{result.Status}," +
            $"RF Power:{result.RfPower},Battery:{result.Battery},Temperature:{result.Temperature}.");
    }
}