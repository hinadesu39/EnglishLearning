using CommonHelper;
using IdentityServiceDomain;
using IdentityServiceDomain.Entities;
using IdentityServiceInfrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Zack.Commons.JsonConverters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
{
    //设置时间格式。而非“2008-08-08T08:08:08”这样的格式
    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
});

///数据库
builder.Services.AddDbContext<IdDBContext>(ctx =>
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
//日志信息输出
builder.Services.AddLogging(builder =>
{
    Log.Logger = new LoggerConfiguration()
       .WriteTo.Console()
       .WriteTo.File("f:/temp/IdentityService.log")
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
//结束配置跨域

//开始基础设施层注入
builder.Services.AddScoped<IdDomainService>();
builder.Services.AddScoped<IIdRepository, IdRepository>();
builder.Services.AddScoped<IEmailSender, MockEmailSender>();
builder.Services.AddScoped<ISmsSender, MockSmsSender>();

//领域事件
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));


//开始:Authentication,Authorization
//只要需要校验Authentication报文头的地方（非IdentityService.WebAPI项目）也需要启用这些
//IdentityService项目还需要启用AddIdentityCore
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
JWTOptions jwtOpt = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
builder.Services.AddJWTAuthentication(jwtOpt);
//启用Swagger中的【Authorize】按钮。
builder.Services.Configure<SwaggerGenOptions>(c =>
{
    c.AddAuthenticationHeader();
});
//结束:Authentication,Authorization

builder.Services.AddDataProtection();
//登录、注册的项目除了要启用WebApplicationBuilderExtensions中的初始化之外，还要如下的初始化
//不要用AddIdentity，而是用AddIdentityCore
//因为用AddIdentity会导致JWT机制不起作用，AddJwtBearer中回调不会被执行，因此总是Authentication校验失败
//https://github.com/aspnet/Identity/issues/1376
IdentityBuilder idBuilder = builder.Services.AddIdentityCore<User>(options =>
{
    //三次登录失败锁定用户5分钟
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    //不能设定RequireUniqueEmail，否则不允许邮箱为空
    //options.User.RequireUniqueEmail = true;
    //以下两行，把GenerateEmailConfirmationTokenAsync验证码缩短

    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
}
);
idBuilder = new IdentityBuilder(idBuilder.UserType, typeof(IdentityServiceDomain.Entities.Role), builder.Services);
idBuilder.AddEntityFrameworkStores<IdDBContext>().AddDefaultTokenProviders()
    //.AddRoleValidator<RoleValidator<Role>>()
    .AddRoleManager<RoleManager<IdentityServiceDomain.Entities.Role>>()
    .AddUserManager<IdUserManager>();

// Configure the HTTP request pipeline.


//Redis的配置
string redisConnStr = builder.Configuration.GetValue<string>("Redis:ConnStr");
IConnectionMultiplexer redisConnMultiplexer = ConnectionMultiplexer.Connect(redisConnStr);
builder.Services.AddSingleton(typeof(IConnectionMultiplexer), redisConnMultiplexer);




builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders();
app.MapControllers();
//`app.UseStaticFiles()` 是 ASP.NET 中的一个配置，它将 `wwwroot` 文件夹映射到 `/` 路径，
//并在处理其他应用程序中间件（如 Razor Pages、Minimal APIs、MVC 等）之前将该文件夹中的任何内容作为静态内容提供¹。
//这意味着，它可以让 ASP.NET Core 应用程序直接向客户端提供静态文件，例如 HTML、CSS、图像和 JavaScript²。
app.UseStaticFiles();

app.Run();
