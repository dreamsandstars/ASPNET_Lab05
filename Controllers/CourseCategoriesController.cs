using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspNetWeek3.Mvc.Services;

namespace AspNetWeek3.Mvc.Controllers;

public class CourseCategoriesController : Controller
{
    private readonly ICourseService _courseService;

    public CourseCategoriesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _courseService.GetCategoryListAsync();
        ViewBag.Courses = await _courseService.GetCourseListAsync();
        return View(categories);
    }
}
