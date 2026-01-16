using ReservationSystem.Infrastructure.Persistence;
using ReservationSystem.Infrastructure.Services;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;

var builder = WebApplication.CreateBuilder(args);

// 1. MongoDB ve Context Kaydı
var mongoUri = "mongodb+srv://aytaccamurlu26_db_user:3HwWLyyOSY1Stvaj@cluster0.vg96nxd.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
builder.Services.AddSingleton(new MongoContext(mongoUri));

// 2. Hangfire Konfigürasyonu
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMongoStorage(mongoUri, "ReservationSystemDB", new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        },
        CheckConnection = true,
        Prefix = "hangfire"
    })
);
builder.Services.AddHangfireServer();

// 3. Servis Kayıtları
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Swashbuckle kullanımı
builder.Services.AddScoped<ReservationService>();

var app = builder.Build();

// 4. Middleware Pipeline - Render ve Her Ortamda Swagger Açık
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reservation API V1");
    c.RoutePrefix = string.Empty; // Ana dizinde Swagger başlar
});

// Render/Docker ortamında HTTPS bazen sorun çıkarabilir, 
// eğer linke ulaşamazsan bu satırı yorum satırı yapabilirsin.
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.Run();
