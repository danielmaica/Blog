using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
ConfiguraAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

var app = builder.Build();
LoadConfiguration(app);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCompression();
app.UseStaticFiles();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
  Console.WriteLine("Estou no ambiente de desenvolvimento!");
}

app.Run();


void LoadConfiguration(WebApplication wa)
{
  Configuration.JwtKey = wa.Configuration.GetValue<string>("JwtKey")!;
  Configuration.ApiKeyName = wa.Configuration.GetValue<string>("ApiKeyName")!;
  Configuration.ApiKey = wa.Configuration.GetValue<string>("ApiKey")!;

  var smtp = new Configuration.SmtpConfiguration();
  wa.Configuration.GetSection("Smtp").Bind(smtp);
  Configuration.Smtp = smtp;
}

void ConfiguraAuthentication(WebApplicationBuilder wab)
{
  var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
  wab.Services.AddAuthentication(x =>
  {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  }).AddJwtBearer(x =>
  {
    x.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidateIssuer = false,
      ValidateAudience = false
    };
  });
}

void ConfigureMvc(WebApplicationBuilder wab)
{
  wab.Services.AddMemoryCache();
  wab.Services.AddResponseCompression(options =>
  {
    options.Providers.Add<GzipCompressionProvider>();
  });
  wab.Services.Configure<GzipCompressionProviderOptions>(options =>
  {
    options.Level = CompressionLevel.Optimal;
  });
  wab.Services
  .AddControllers()
  .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
  .AddJsonOptions(x =>
  {
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // ignorar listas dentro de outros listas
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault; // quando o objeto for nulo ele não deixa renderizar na tela
  });
}

void ConfigureServices(WebApplicationBuilder wab)
{
  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
  wab.Services.AddDbContext<BlogDataContext>(options => options.UseSqlServer(connectionString));
  wab.Services.AddTransient<TokenService>();
  wab.Services.AddTransient<EmailService>();
}
