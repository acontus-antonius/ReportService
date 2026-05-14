using ReportService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS für Angular-Entwicklung
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:50946")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Report Services registrieren
builder.Services.AddSingleton<ITenantService, TenantService>();
builder.Services.AddSingleton<ITemplateService, ScribanTemplateService>();
builder.Services.AddSingleton<IPdfService, PuppeteerPdfService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
