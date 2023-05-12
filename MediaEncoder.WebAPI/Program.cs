using CommonHelper;
using FileServiceInfrastrucure.Services;
using MediaEncoder.WebAPI;
using MediaEncoder.WebAPI.BgServices;
using MediaEncoderDomain;
using MediaEncoderInfrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using Zack.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddScoped<MediaEncoderFactory>();

builder.Services.AddScoped<IMediaEncoderRepository, MediaEncoderRepository>();
builder.Services.AddScoped<IMediaEncoder, ToM4AEncoder>();

///数据库
builder.Services.AddDbContext<MEDbContext>(ctx =>
{
    //连接字符串如果放到appsettings.json中，会有泄密的风险
    //如果放到UserSecrets中，每个项目都要配置，很麻烦
    //因此这里推荐放到环境变量中。
    string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
    ctx.UseSqlServer(connStr);
});

//配置读取from数据库
string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
builder.Configuration.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<FileServiceOptions>(builder.Configuration.GetSection("FileService:Endpoint"));
builder.Services//.AddOptions() //asp.net core项目中AddOptions()不写也行，因为框架一定自动执行了
    .Configure<SMBStorageOptions>(builder.Configuration.GetSection("FileService:SMB"));

//日志信息输出
builder.Services.AddLogging(builder =>
{
    Log.Logger = new LoggerConfiguration()
       .WriteTo.Console()
       .WriteTo.File("f:/temp/MediaEncoder.log")
       .CreateLogger();
    builder.AddSerilog();
});
Log.Logger.Information("hello");

builder.Services.AddHttpClient();
builder.Services.AddHostedService<EncodingBgService>();//后台转码服务

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
//结束配置跨域

//领域事件
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));


//集成事件
builder.Services.Configure<IntegrationEventRabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddEventBus("MediaEncoder.WebAPI", Assembly.GetExecutingAssembly());


//builder.Services.AddSignalR().AddStackExchangeRedis("127.0.0.1", opt => opt.Configuration.ChannelPrefix = "SignalR");
string redisConnStr = builder.Configuration.GetValue<string>("Redis:ConnStr");
IConnectionMultiplexer redisConnMultiplexer = ConnectionMultiplexer.Connect(redisConnStr);
builder.Services.AddSingleton(typeof(IConnectionMultiplexer), redisConnMultiplexer);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.MapControllers();
app.UseEventBus();
app.UseCors();
app.Run();
