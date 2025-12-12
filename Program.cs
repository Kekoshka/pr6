using Microsoft.EntityFrameworkCore;
using pr6.Common.Extensions;
using pr6.Context;
using pr6.Models.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddDirectoryBrowser();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureJWTAuthentication(builder.Configuration);
builder.Services.RegisterExecutingAsseblyServices();
builder.Services.AddDbContext<ApplicationContext>(config =>
{
    config.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionStringMSSQL"));
});

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(nameof(MailOptions)));
builder.Services.Configure<RandomOptions>(builder.Configuration.GetSection(nameof(RandomOptions)));
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection(nameof(JWTOptions)));
builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection(nameof(CaptchaOptions)));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseExceptionHandling();

app.UseCaptcha();

app.UseHttpsRedirection();

app.UseSwagger().UseSwaggerUI();

app.MapControllers();

app.Run();
