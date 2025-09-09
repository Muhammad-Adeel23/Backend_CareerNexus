using CareerNexus.AppConfiguration;
using CareerNexus.Models;
using CareerNexus.Services;
using CareerNexus.Services.Authenticate;
using CareerNexus.Services.EmailSender;
using CareerNexus.Services.EmailTemplate;
using CareerNexus.Services.OtpService;
using CareerNexus.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = AppConfiguration.LoadConfiguration();
//var appskey = new AppSettingModel
//{
//    Domain = AppSettingModel.,

//}
// Add services to the container.
builder.Services.AddSingleton<IConfiguration>(AppConfiguration.LoadConfiguration());
builder.Services.AddSingleton<IAuthenticate,AuthenticateService>();
builder.Services.AddSingleton<IUserService,UserService>();
builder.Services.AddSingleton<IOTP,OtpService>();
builder.Services.AddSingleton<IEmailSenderService,EmailSenderService>();
builder.Services.AddSingleton<IEmailTemplateService,EmailTemplateService>();
//builder.Services.AddSingleton<IAuthenticate,AuthenticateService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CareerNexus API", Version = "v1" });
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secret = AppConfiguration._secret;

    if (string.IsNullOrWhiteSpace(secret))
        throw new Exception("JWT Secret is null or empty. Check appsettings.json and AppConfiguration loading.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = AppConfiguration._validIssuer,
        ValidAudience = AppConfiguration._validAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
   // c.SwaggerDoc("v1", new OpenApiInfo { Title = "CareerNexus API", Version = "v1" });

    // ?? Add JWT Bearer support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
 app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CareerNexus API v1");
    });app.MapControllers();

app.Run();
