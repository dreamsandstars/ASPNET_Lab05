using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AspNetWeek3.Mvc.Services;
using AspNetWeek3.Mvc.ViewModels;

namespace AspNetWeek3.Mvc.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    // GET /Courses
    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetCourseListAsync();
        return View(courses);
    }

    // GET /Courses/Detail/{id}
    public async Task<IActionResult> Detail(int id)
    {
        var course = await _courseService.GetCourseDetailAsync(id);
        if (course == null)
        {
            _logger.LogWarning("Accessing non-existing course details. CourseId={CourseId}", id);
            return NotFound();
        }
        return View(course);
    }

    // GET /Courses/Create
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new CourseCreateViewModel());
    }

    // POST /Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(model);
        }

        var isUnique = await _courseService.IsCourseCodeUniqueAsync(model.CourseCode);
        if (!isUnique)
        {
            ModelState.AddModelError(nameof(model.CourseCode), "Mã khóa học này đã tồn tại.");
            await PopulateCategoriesAsync();
            return View(model);
        }

        await _courseService.CreateCourseAsync(model);
        _logger.LogInformation("Course created. CourseCode={CourseCode}", model.CourseCode);

        TempData["Success"] = "Đã thêm khóa học thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Courses/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _courseService.GetCourseForEditAsync(id);
        if (model == null)
        {
            _logger.LogWarning("Attempted to edit non-existing course. CourseId={CourseId}", id);
            return NotFound();
        }
        await PopulateCategoriesAsync();
        return View(model);
    }

    // POST /Courses/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseEditViewModel model)
    {
        _logger.LogInformation("POST Edit action called. id={id}, model.Id={modelId}", id, model.Id);
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning("ModelState Validation Error in '{Field}': {Error}", state.Key, error.ErrorMessage);
                }
            }
            await PopulateCategoriesAsync();
            return View(model);
        }

        var isUnique = await _courseService.IsCourseCodeUniqueAsync(model.CourseCode, id);
        if (!isUnique)
        {
            ModelState.AddModelError(nameof(model.CourseCode), "Mã khóa học này đã tồn tại.");
            await PopulateCategoriesAsync();
            return View(model);
        }

        try
        {
            var success = await _courseService.UpdateCourseAsync(model);
            if (!success)
            {
                _logger.LogWarning("Course to update not found. CourseId={CourseId}", id);
                return NotFound();
            }

            _logger.LogInformation("Course updated. CourseId={CourseId}", id);
            TempData["Success"] = "Đã cập nhật khóa học thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred when updating course. CourseId={CourseId}", id);
            ModelState.AddModelError(string.Empty, "Dữ liệu đã được người khác cập nhật. Vui lòng tải lại trang và thử lại.");
            await PopulateCategoriesAsync();
            return View(model);
        }
    }

    // GET /Courses/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _courseService.GetCourseDetailAsync(id);
        if (course == null)
        {
            _logger.LogWarning("Attempted to soft delete non-existing course. CourseId={CourseId}", id);
            return NotFound();
        }
        return View(course);
    }

    // POST /Courses/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _courseService.SoftDeleteCourseAsync(id);
        if (!success)
        {
            _logger.LogWarning("Failed to soft delete course. CourseId={CourseId}", id);
            return NotFound();
        }

        _logger.LogWarning("Course soft deleted. CourseId={CourseId}", id);
        TempData["Success"] = "Đã khóa/xóa mềm khóa học.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Courses/Trash
    public async Task<IActionResult> Trash()
    {
        var archivedCourses = await _courseService.GetArchivedCoursesAsync();
        return View(archivedCourses);
    }

    // POST /Courses/Restore/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var success = await _courseService.RestoreCourseAsync(id);
        if (!success)
        {
            _logger.LogWarning("Attempted to restore non-existing or active course. CourseId={CourseId}", id);
            return NotFound();
        }

        _logger.LogInformation("Course restored. CourseId={CourseId}", id);
        TempData["Success"] = "Đã khôi phục khóa học thành công.";
        return RedirectToAction(nameof(Trash));
    }

    // POST /Courses/DeleteForever/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteForever(int id)
    {
        var success = await _courseService.DeleteCourseForeverAsync(id);
        if (!success)
        {
            _logger.LogWarning("Attempted to delete forever non-existing course. CourseId={CourseId}", id);
            return NotFound();
        }

        _logger.LogWarning("Course deleted permanently. CourseId={CourseId}", id);
        TempData["Success"] = "Đã xóa vĩnh viễn khóa học.";
        return RedirectToAction(nameof(Trash));
    }

    public async Task<IActionResult> Filter(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var filteredCourses = await _courseService.GetFilteredCoursesAsync(categoryId, minPrice, maxPrice);
        var categories = await _courseService.GetCategoryListAsync();

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        return View(filteredCourses);
    }

    // GET /Courses/Search
    public async Task<IActionResult> Search(string? keyword, string? seatStatus)
    {
        var courses = await _courseService.SearchCoursesAsync(keyword, seatStatus);
        ViewBag.Keyword = keyword;
        ViewBag.SeatStatus = seatStatus;
        return View(courses);
    }

    // GET /Courses/AdjustSeats/{id}
    public async Task<IActionResult> AdjustSeats(int id)
    {
        var model = await _courseService.GetCourseForAdjustSeatsAsync(id);
        if (model == null)
        {
            _logger.LogWarning("Attempted to adjust seats for non-existing course. CourseId={CourseId}", id);
            return NotFound();
        }
        return View(model);
    }

    // POST /Courses/AdjustSeats/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustSeats(int id, CourseAdjustSeatsViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            var original = await _courseService.GetCourseForAdjustSeatsAsync(id);
            if (original != null)
            {
                model.Name = original.Name;
                model.CurrentSeats = original.CurrentSeats;
            }
            return View(model);
        }

        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null)
        {
            _logger.LogWarning("Course not found when adjusting seats. CourseId={CourseId}", id);
            return NotFound();
        }

        int newSeats = course.AvailableSeats + model.SeatChange.GetValueOrDefault();
        if (newSeats < 0)
        {
            ModelState.AddModelError(nameof(model.SeatChange), "Số chỗ sau khi điều chỉnh không được nhỏ hơn 0.");
            model.Name = course.Name;
            model.CurrentSeats = course.AvailableSeats;
            return View(model);
        }

        try
        {
            var success = await _courseService.AdjustSeatsAsync(model);
            if (!success)
            {
                _logger.LogWarning("Failed to adjust seats. CourseId={CourseId}", id);
                return NotFound();
            }

            _logger.LogInformation("Adjusted seats successfully. CourseId={CourseId}, SeatChange={SeatChange}, NewSeats={NewSeats}", 
                id, model.SeatChange, newSeats);
            TempData["Success"] = $"Đã điều chỉnh số chỗ thành công ({model.SeatChange:+#;-#;0} chỗ, còn lại {newSeats}).";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred when adjusting seats. CourseId={CourseId}", id);
            ModelState.AddModelError(string.Empty, "Dữ liệu đã bị người dùng khác thay đổi. Vui lòng tải lại trang và thử lại.");
            model.Name = course.Name;
            model.CurrentSeats = course.AvailableSeats;
            return View(model);
        }
    }

    private async Task PopulateCategoriesAsync()
    {
        var categories = await _courseService.GetCategoryListAsync();
        ViewBag.Categories = categories;
    }

    // GET /Courses/ExportToCsv
    [HttpGet]
    public async Task<IActionResult> ExportToCsv()
    {
        var courses = await _courseService.GetCourseListAsync();
        var csvBuilder = new System.Text.StringBuilder();
        csvBuilder.Append('\uFEFF');
        csvBuilder.AppendLine("ID,Mã Khóa Học,Tên Khóa Học,Học Phí,Số Chỗ Trống,Danh Mục");

        foreach (var course in courses)
        {
            var escapedName = course.Name.Contains(",") ? $"\"{course.Name}\"" : course.Name;
            var escapedCategory = course.CategoryName.Contains(",") ? $"\"{course.CategoryName}\"" : course.CategoryName;
            csvBuilder.AppendLine($"{course.Id},{course.CourseCode},{escapedName},{course.Price},{course.AvailableSeats},{escapedCategory}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
        return File(bytes, "text/csv", "danh_sach_khoa_hoc.csv");
    }

    // GET /Courses/Import
    [HttpGet]
    public IActionResult Import()
    {
        return View();
    }

    // POST /Courses/Import
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn tệp tin CSV để nhập.");
            return View();
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Định dạng tệp không hợp lệ. Vui lòng tải lên tệp .csv.");
            return View();
        }

        using var stream = file.OpenReadStream();
        var (importedCount, errors) = await _courseService.ImportCoursesFromCsvAsync(stream);

        if (errors.Any())
        {
            foreach (var err in errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
            return View();
        }

        TempData["Success"] = $"Nhập hàng loạt thành công {importedCount} khóa học mới.";
        return RedirectToAction(nameof(Index));
    }
}
