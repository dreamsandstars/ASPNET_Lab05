using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetWeek3.Mvc.Data;
using AspNetWeek3.Mvc.ViewModels;

namespace AspNetWeek3.Mvc.Controllers;

public class DataHealthController : Controller
{
    private readonly AppDbContext _context;

    public DataHealthController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DataHealthViewModel();

        try
        {
            model.IsDatabaseOnline = await _context.Database.CanConnectAsync();
            model.ConnectionString = _context.Database.GetDbConnection().ConnectionString;
            model.DatabaseProvider = _context.Database.ProviderName ?? "N/A";

            if (model.IsDatabaseOnline)
            {
                model.CategoryCount = await _context.CourseCategories.CountAsync();
                model.CourseCount = await _context.Courses.CountAsync();
                model.EnrollmentCount = await _context.Enrollments.CountAsync();
                model.StudentCount = await _context.Students.CountAsync();

                model.AppliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();
                model.PendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();
            }
        }
        catch (Exception ex)
        {
            model.IsDatabaseOnline = false;
            model.ConnectionString = $"Connection error: {ex.Message}";
        }

        return View(model);
    }
}
