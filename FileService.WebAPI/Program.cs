using FileServiceDomain;
using FileServiceInfrastrucure.Services;
using FileServiceInfrastrucure;
using Microsoft.Extensions.Configuration;
using CommonHelper;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using UserMgrWebApi;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//日志信息输出
services.AddLogging(builder =>
{
    Log.Logger = new LoggerConfiguration()
       .WriteTo.Console()
       .WriteTo.File("f:/temp/FileService.log")
       .CreateLogger();
    //builder.AddSerilog();
});
Log.Logger.Information("hello");


//配置读取from数据库
string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
builder.Configuration.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
builder.Services//.AddOptions() //asp.net core项目中AddOptions()不写也行，因为框架一定自动执行了
    .Configure<SMBStorageOptions>(builder.Configuration.GetSection("FileService:SMB"));
//结束

//开始基础设施层注入
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IStorageClient, SMBStorageClient>();
//保存到云存储中，本项目暂时不用
//使用虚拟的云存储
builder.Services.AddScoped<IStorageClient, MockCloudStorageClient>();
builder.Services.AddScoped<IFSRepository, FSRepository>();
builder.Services.AddScoped<FSDomainService>();
builder.Services.AddHttpClient();
//结束基础设施层注入

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

//开始:Authentication,Authorization
//只要需要校验Authentication报文头的地方（非IdentityService.WebAPI项目）也需要启用这些
//IdentityService项目还需要启用AddIdentityCore
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
JWTOptions jwtOpt = configuration.GetSection("JWT").Get<JWTOptions>();
builder.Services.AddJWTAuthentication(jwtOpt);
//启用Swagger中的【Authorize】按钮。
builder.Services.Configure<SwaggerGenOptions>(c =>
{
    c.AddAuthenticationHeader();
});
//结束:Authentication,Authorization

///数据库
builder.Services.AddDbContext<FSDBContext>(ctx =>
{
    //连接字符串如果放到appsettings.json中，会有泄密的风险
    //如果放到UserSecrets中，每个项目都要配置，很麻烦
    //因此这里推荐放到环境变量中。
    string connStr = configuration.GetValue<string>("DefaultDB:ConnStr");
    ctx.UseSqlServer(connStr);
});

//开始配置跨域
services.AddCors(options =>
{
    //更好的在Program.cs中用绑定方式读取配置的方法：https://github.com/dotnet/aspnetcore/issues/21491
    //不过比较麻烦。
    var corsOpt = configuration.GetSection("Cors").Get<CorsSettings>();

    string[] urls = corsOpt.Origins;
    options.AddDefaultPolicy(builder => builder.WithOrigins(urls)
            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
}
);
//结束配置跨域


builder.Services.Configure<MvcOptions>(o =>
{
    //注册全局的filter
    o.Filters.Add<UnitOfWorkFilter>();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders();
app.MapControllers();
//`app.UseStaticFiles()` 是 ASP.NET 中的一个配置，它将 `wwwroot` 文件夹映射到 `/` 路径，
//并在处理其他应用程序中间件（如 Razor Pages、Minimal APIs、MVC 等）之前将该文件夹中的任何内容作为静态内容提供¹。
//这意味着，它可以让 ASP.NET Core 应用程序直接向客户端提供静态文件，例如 HTML、CSS、图像和 JavaScript²。
app.UseStaticFiles();
app.UseCors();
app.Run();
