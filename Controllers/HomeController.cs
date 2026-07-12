using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetWeek3.Mvc.Data;
using AspNetWeek3.Mvc.Models;

namespace AspNetWeek3.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.CategoryCount = await _context.CourseCategories.AsNoTracking().CountAsync();
            ViewBag.CourseCount = await _context.Courses.AsNoTracking().CountAsync();
            ViewBag.StudentCount = await _context.Students.AsNoTracking().CountAsync();
            ViewBag.EnrollmentCount = await _context.Enrollments.AsNoTracking().CountAsync();
            ViewBag.LowSeatsCount = await _context.Courses.AsNoTracking().CountAsync(c => c.AvailableSeats <= 3);
            ViewBag.IsDbConnected = true;

            // Lab05 Course Metrics
            ViewBag.TotalCourseCount = await _context.Courses.IgnoreQueryFilters().AsNoTracking().CountAsync();
            ViewBag.ActiveCourseCount = await _context.Courses.AsNoTracking().CountAsync();
            ViewBag.DeletedCourseCount = await _context.Courses.IgnoreQueryFilters().AsNoTracking().CountAsync(c => c.IsDeleted);

            // Lab05 Created or Updated Today
            var today = DateTime.Today;
            ViewBag.CreatedOrUpdatedToday = await _context.Courses
                .IgnoreQueryFilters()
                .AsNoTracking()
                .CountAsync(c => c.CreatedAt >= today || (c.UpdatedAt.HasValue && c.UpdatedAt.Value >= today));

            // Lab05 Logs Today Counting
            int logsToday = 0;
            var todayStr = DateTime.Now.ToString("yyyyMMdd");
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", $"lab05-{todayStr}.txt");
            if (System.IO.File.Exists(logPath))
            {
                try
                {
                    using var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("[INF]") || line.Contains("[WRN]") || line.Contains("[ERR]") || line.Contains("[DBG]"))
                        {
                            logsToday++;
                        }
                    }
                }
                catch
                {
                    // Ignore
                }
            }
            ViewBag.LogsToday = logsToday;

            ViewBag.RecentEnrollments = await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.EnrollmentDetails)
                .ThenInclude(ed => ed.Course)
                .OrderByDescending(e => e.CreatedAt)
                .Take(2)
                .ToListAsync();
        }
        catch
        {
            ViewBag.CategoryCount = 0;
            ViewBag.CourseCount = 0;
            ViewBag.StudentCount = 0;
            ViewBag.EnrollmentCount = 0;
            ViewBag.LowSeatsCount = 0;
            ViewBag.IsDbConnected = false;
            
            ViewBag.TotalCourseCount = 0;
            ViewBag.ActiveCourseCount = 0;
            ViewBag.DeletedCourseCount = 0;
            ViewBag.CreatedOrUpdatedToday = 0;
            ViewBag.LogsToday = 0;

            ViewBag.RecentEnrollments = new List<Enrollment>();
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? id = null)
    {
        Console.WriteLine($"[DEBUG] Error action called with id = '{id}'");
        if (id == 404)
        {
            return View("NotFound");
        }
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
