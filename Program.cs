using pr6.Common.Extensions;
using pr6.Models.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(nameof(MailOptions)));
builder.Services.Configure<RandomOptions>(builder.Configuration.GetSection(nameof(RandomOptions)));
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection(nameof(RandomOptions)));
builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection(nameof(CaptchaOptions)));

builder.Services.ConfigureJWTAuthentication();

builder.Services.RegisterExecutingAsseblyServices();

var app = builder.Build();

app.UseExceptionHandling();

app.UseCaptcha();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
