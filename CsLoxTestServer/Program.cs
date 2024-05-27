var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

/*
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});
*/

app.MapPost("/getGlobal", () =>
{
    return "{ \"script\":\"var urlGlobal = true;\"}";
});

app.MapPost("/getScript", () =>
{
    return "{ \"script\":\"urlGlobal = false;\nprint urlGlobal;\"}";
});

app.MapGet("/getGlobal", () =>
{
    return "{ \"script\":\"var urlGlobal = true;\"}";
});

app.MapGet("/getScript", () =>
{
    return "{ \"script\":\"urlGlobal = false;\nprint urlGlobal;\"}";
});

app.MapGet("/setAnswer", (HttpRequest request) =>
{
    return "{ \"msg\":\"Found answer: '" + request.Query["answer"] + ", " + request.Query["variableName"] + "'\" }";
});

app.MapPost("/setAnswer", (HttpRequest request) =>
{
    return "{ \"msg\":\"Found answer: '" + request.Form["answer"] + ", " + request.Form["variableName"] + "'\" }";
});

app.Run();

/*
internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
*/
