using Amazon.S3;
using LctHack;
using LctHack.Api;
using LctHack.Configuration;
using LctHack.Jobs;
using LctHack.Services;
using LctHack.Services.Initialize;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
});
builder.Services.Configure<S3Options>(builder.Configuration.GetSection(nameof(S3Options)));
builder.Services.Configure<UrlGeneratorOptions>(builder.Configuration.GetSection(nameof(UrlGeneratorOptions)));

builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb"));
    o.UseSnakeCaseNamingConvention(); 
});

builder.Services.AddSingleton(s =>
{
    var opts = s.GetRequiredService<IOptions<S3Options>>().Value;
    return new AmazonS3Client(opts.AccessKeyId, opts.SecretAccessKey, new AmazonS3Config()
    {
        ServiceURL = opts.ServiceUrl,
        ForcePathStyle = true
    });
});

builder.Services.AddQuartz(q =>
{
    q.ScheduleJob<RefreshVideoStateJob>(opts => opts
        .WithIdentity(nameof(RefreshVideoStateJob) + "-trigger")
        .StartNow()
        .WithCronSchedule("0/5 * * * * ?"));
});
builder.Services.AddQuartzHostedService();

builder.Services.AddScoped<RefreshVideoStateJob>();

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IUrlGenerator, UrlGenerator>();
builder.Services.AddHttpClient<IMlService, MlService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("MlService")!);
});
builder.Services.AddScoped<IWebSocketService, WebSocketService>();
builder.Services.AddAntiforgery();

builder.Services.AddHostedService<MigrateDb<ApplicationDbContext>>();
builder.Services.AddHostedService<CreateBucketService>();

builder.Services.AddCors();

var app = builder.Build();
app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAntiforgery();
var api = app.MapGroup("api");
var videos = api.MapGroup("videos");
var status = api.MapGroup("status");
status.MapStatusTracking();
videos.MapVideosUploading();
app.Run();