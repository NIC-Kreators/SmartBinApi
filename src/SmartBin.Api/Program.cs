using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options
            .WithTitle("SmartBin API")
            .WithTheme(ScalarTheme.DeepSpace)
            .ShowOperationId()
            .WithDefaultHttpClient(ScalarTarget.Node, ScalarClient.Undici)
            .AddPreferredSecuritySchemes("BearerAuth")
            .AddHttpAuthentication("BearerAuth", auth =>
            {
                auth.Token = "Your jwt token";
            });
    });
}

app.MapGet("/hello", () => "Hello").Stable();
app.Run();
