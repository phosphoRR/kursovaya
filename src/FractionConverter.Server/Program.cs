using FractionConverter.Server;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Учебный прототип: авторизация и сохранение истории будут добавлены позже.
app.MapPost("/api/convert", (ConvertRequest request) =>
{
    try
    {
        string result = NumberConverter.Convert(
            request.Number, request.SourceBase, request.TargetBase, request.Precision);
        return Results.Ok(new ConvertResponse(result));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

public record ConvertRequest(string Number, int SourceBase, int TargetBase, int Precision);
public record ConvertResponse(string Result);
