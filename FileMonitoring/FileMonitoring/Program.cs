using FileMonitoring.API.Middlewares;
using FileMonitoring.Application.Interfaces;
using FileMonitoring.Application.Services;
using FileMonitoring.Domain.Interfaces;
using FileMonitoring.Infrastructure.Data;
using FileMonitoring.Infrastructure.Data.Repositories;
using FileMonitoring.Infrastructure.FileStorage;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IArquivoRepository, ArquivoRepository>();
builder.Services.AddScoped<ITransacaoArquivoRepository, TransacaoArquivoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.AddScoped<IParsingService, ParsingService>();
builder.Services.AddScoped<IArquivoService, ArquivoService>();

builder.Services.AddAutoMapper(typeof(FileMonitoring.Application.Mappings.MappingProfile));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<FileMonitoring.Application.Validators.ArquivoUploadValidator>();

builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseGlobalExceptionMiddleware();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();