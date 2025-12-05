using pr6.Common.Extensions;
using pr6.Models.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(nameof(MailOptions)));
builder.Services.Configure<RandomOptions>(builder.Configuration.GetSection(nameof(RandomOptions)));

builder.Services.ConfigureJWTAuthentication();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
