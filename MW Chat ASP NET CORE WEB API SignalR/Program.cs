using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Data;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Hubs;
using FluentValidation.AspNetCore;
using System.Text;
using FluentValidation;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Validators;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

#region Services

var services = builder.Services;

services.AddScoped<IValidator<User>, UserValidator>();

services.AddCors();
services.AddSignalR();
//services.AddFluentValidation();

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<UserContext>(options => options.UseSqlServer(configuration.GetConnectionString("Default")));
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = configuration.GetSection("Token").GetValue<bool>("CheckIssuer"),
                        ValidIssuers = configuration.GetSection("Token").GetValue<IEnumerable<string>>("Issuers"),
                        ValidateAudience = configuration.GetSection("Token").GetValue<bool>("CheckAudience"),
                        ValidAudiences = configuration.GetSection("Token").GetValue<IEnumerable<string>>("Audiences"),
                        ValidateIssuerSigningKey = configuration.GetSection("Token").GetValue<bool>("CheckSigningKey"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Token").GetValue<string>("SuperSecretKey")))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // если запрос направлен хабу
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/chat")))
                            {
                                // получаем токен из строки запроса
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

#endregion

var app = builder.Build();

#region Middlewares

app.UseCors((options) =>
{
    options.WithOrigins()
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed((x) => true)
    .AllowCredentials();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chat");
});

#endregion

app.Run();
