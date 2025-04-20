using Microsoft.EntityFrameworkCore;
using Querim.Data;
using Querim.Services;
using Querim.Data;
using Querim.Services;
using Querim.Middleware;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB for PDF files
});
builder.Services.AddScoped<IChapterService, ChapterService>();
var app = builder.Build();
var uploadsDir = Path.Combine(app.Environment.WebRootPath, "uploads", "chapters");
Directory.CreateDirectory(uploadsDir);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseRouting();
app.UseRequestLogging();
app.UseAuthorization();

app.MapControllers();
app.Run();