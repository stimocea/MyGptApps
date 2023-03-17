using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var basicAuthOptions = builder.Configuration.GetSection("BasicAuthenticationOptions").Get<BasicAuthenticationOptions>() 
    ?? throw new Exception("Could not get the BasicAuthenticationOptions from the configuration");

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("BasicAuthentication", options =>
    {
        options.Username = basicAuthOptions.Username;
        options.Password = basicAuthOptions.Password;
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOutputCache();


string[] Summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapGet("/weatherforecast", () =>
{
    var rng = new Random();
    var weatherForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = rng.Next(-20, 55),
        Summary = Summaries[rng.Next(Summaries.Length)]
    })
    .ToArray();

    return weatherForecast;
}).CacheOutput(x => x.Expire(TimeSpan.FromSeconds(10)))
.RequireAuthorization();

app.MapGet("/health", () => "OK").AllowAnonymous();

app.UseOutputCache();
app.Run();

internal class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string Summary { get; set; } = string.Empty;
}