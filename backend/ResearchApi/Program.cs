using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(Options =>
{
    Options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapPost("/api/research/analisar", async (RelatorioDto relatorio) =>
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
    

        await channel.QueueDeclareAsync(queue: "relatorios_fila",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var jsonMessage = JsonSerializer.Serialize(relatorio);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        await channel.BasicPublishAsync(exchange: "",
                             routingKey: "relatorios_fila",
                             body: body);

       return Results.Ok(new { status = "Relatório enfsileirado com sucesso!", dados = relatorio });
    });

app.MapGet("/", () => "Olá Mundo!");



app.Run();

record RelatorioDto(int id, string texto);