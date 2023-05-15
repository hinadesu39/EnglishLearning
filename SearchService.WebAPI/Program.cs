using CommonHelper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nest;
using SearchServiceDomain;
using SearchServiceInfrastructure;
using Serilog;
using System.Reflection;
using Zack.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//配置读取from数据库
string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
builder.Configuration.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));

//日志信息输出
builder.Services.AddLogging(builder =>
{
    Log.Logger = new LoggerConfiguration()
       .WriteTo.Console()
       .WriteTo.File("f:/temp/SearchService.log")
       .CreateLogger();
    builder.AddSerilog();
});
Log.Logger.Information("hello");

//开始配置跨域
builder.Services.AddCors(options =>
{
    //更好的在Program.cs中用绑定方式读取配置的方法：https://github.com/dotnet/aspnetcore/issues/21491
    //不过比较麻烦。
    var corsOpt = builder.Configuration.GetSection("Cors").Get<CorsSettings>();

    string[] urls = corsOpt.Origins;
    options.AddDefaultPolicy(builder => builder.WithOrigins(urls)
            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
}
);

//领域事件
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
//集成事件
builder.Services.Configure<IntegrationEventRabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddEventBus("SearchService.WebAPI", Assembly.GetExecutingAssembly());


builder.Services.AddHttpClient();

builder.Services.Configure<ElasticSearchOptions>(builder.Configuration.GetSection("ElasticSearch"));

builder.Services.AddScoped<IElasticClient>(sp =>
{
    var option = sp.GetRequiredService<IOptions<ElasticSearchOptions>>();
    var settings = new ConnectionSettings(option.Value.Url);
    return new ElasticClient(settings);
});
builder.Services.AddScoped<ISearchRepository, SearchRepository>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//`app.UseStaticFiles()` 是 ASP.NET 中的一个配置，它将 `wwwroot` 文件夹映射到 `/` 路径，
//并在处理其他应用程序中间件（如 Razor Pages、Minimal APIs、MVC 等）之前将该文件夹中的任何内容作为静态内容提供¹。
//这意味着，它可以让 ASP.NET Core 应用程序直接向客户端提供静态文件，例如 HTML、CSS、图像和 JavaScript²。
app.UseStaticFiles();
app.UseEventBus();
app.UseCors();
app.Run();
