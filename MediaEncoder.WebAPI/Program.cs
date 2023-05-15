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

///���ݿ�
builder.Services.AddDbContext<MEDbContext>(ctx =>
{
    //�����ַ�������ŵ�appsettings.json�У�����й�ܵķ���
    //����ŵ�UserSecrets�У�ÿ����Ŀ��Ҫ���ã����鷳
    //��������Ƽ��ŵ����������С�
    string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
    ctx.UseSqlServer(connStr);
});

//���ö�ȡfrom���ݿ�
string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
builder.Configuration.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<FileServiceOptions>(builder.Configuration.GetSection("FileService:Endpoint"));
builder.Services//.AddOptions() //asp.net core��Ŀ��AddOptions()��дҲ�У���Ϊ���һ���Զ�ִ����
    .Configure<SMBStorageOptions>(builder.Configuration.GetSection("FileService:SMB"));

//��־��Ϣ���
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
builder.Services.AddHostedService<EncodingBgService>();//��̨ת�����

//��ʼ���ÿ���
builder.Services.AddCors(options =>
{
    //���õ���Program.cs���ð󶨷�ʽ��ȡ���õķ�����https://github.com/dotnet/aspnetcore/issues/21491
    //�����Ƚ��鷳��
    var corsOpt = builder.Configuration.GetSection("Cors").Get<CorsSettings>();

    string[] urls = corsOpt.Origins;
    options.AddDefaultPolicy(builder => builder.WithOrigins(urls)
            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
}
);
//�������ÿ���

//�����¼�
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));


//�����¼�
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
