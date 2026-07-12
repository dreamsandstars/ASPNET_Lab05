using Microsoft.EntityFrameworkCore;
using AspNetWeek3.Mvc.Data;
using AspNetWeek3.Mvc.Options;
using AspNetWeek3.Mvc.Repositories;
using AspNetWeek3.Mvc.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/lab05-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Options Pattern Configuration
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// EF Core DbContext with SQLite provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection Registration
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running."), tags: new[] { "live" })
    .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready" });

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] =
            DateTimeOffset.UtcNow;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Custom exception handling middleware to format API error response as ProblemDetails
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred during request processing.");

        if (context.Request.Path.StartsWithSegments("/api") || 
            context.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            string title = "An error occurred while processing your request.";
            string detail = app.Environment.IsDevelopment() ? ex.ToString() : "Đã xảy ra lỗi hệ thống. Vui lòng liên hệ quản trị viên.";
            string type = "https://example.com/problems/internal-server-error";
            string instance = context.Request.Path;
            string traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
            string timestamp = DateTimeOffset.UtcNow.ToString("o");

            bool prefersHtml = context.Request.Headers.Accept.Any(a => a != null && a.Contains("text/html")) 
                               && context.Request.Query["format"] != "json";

            if (prefersHtml)
            {
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "text/html; charset=utf-8";
                var html = AspNetWeek3.Mvc.Services.ProblemDetailsHtmlGenerator.Generate(
                    statusCode, title, detail, type, instance, traceId, timestamp);
                await context.Response.WriteAsync(html);
            }
            else
            {
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                
                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = instance,
                    Type = type
                };
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = timestamp;

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
        else
        {
            throw;
        }
    }
});

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseRouting();
app.UseAuthorization();

// Health check mappings
var optionsLive = new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        bool prefersHtml = context.Request.Headers["Accept"].ToString().Contains("text/html");
        if (prefersHtml)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            var html = AspNetWeek3.Mvc.Services.HealthCheckHtmlGenerator.Generate(report, "System Liveness");
            await context.Response.WriteAsync(html);
        }
        else
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description ?? "No description available."
                })
            };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
};

var optionsReady = new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        bool prefersHtml = context.Request.Headers["Accept"].ToString().Contains("text/html");
        if (prefersHtml)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            var html = AspNetWeek3.Mvc.Services.HealthCheckHtmlGenerator.Generate(report, "Database Readiness");
            await context.Response.WriteAsync(html);
        }
        else
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description ?? "No description available."
                })
            };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
};

app.MapHealthChecks("/health/live", optionsLive);
app.MapHealthChecks("/health/ready", optionsReady);

app.MapGet("/api/courses/{id:int}", async (int id, AppDbContext db, HttpContext http) =>
{
    var course = await db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    if (course == null)
    {
        string type = "https://example.com/problems/course-not-found";
        string title = "Course not found";
        string detail = $"The course with id {id} was not found.";
        int statusCode = StatusCodes.Status404NotFound;
        string instance = http.Request.Path;
        string traceId = System.Diagnostics.Activity.Current?.Id ?? http.TraceIdentifier;
        string timestamp = DateTimeOffset.UtcNow.ToString("o");

        bool prefersHtml = http.Request.Headers.Accept.Any(a => a != null && a.Contains("text/html")) 
                           && http.Request.Query["format"] != "json";

        if (prefersHtml)
        {
            var html = AspNetWeek3.Mvc.Services.ProblemDetailsHtmlGenerator.Generate(
                statusCode, title, detail, type, instance, traceId, timestamp, "COURSE_NOT_FOUND");
            return Results.Content(html, "text/html", System.Text.Encoding.UTF8, statusCode);
        }

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = type,
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = instance
        };
        problemDetails.Extensions["errorCode"] = "COURSE_NOT_FOUND";
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = timestamp;

        return Results.Json(problemDetails, statusCode: statusCode, contentType: "application/problem+json");
    }

    return Results.Ok(course);
});

// Test error endpoint for ProblemDetails verification
app.MapGet("/api/test-error", () => { throw new Exception("Đây là lỗi thử nghiệm!"); });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
