using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetWeek3.Mvc.Models;
using AspNetWeek3.Mvc.ViewModels;

namespace AspNetWeek3.Mvc.Services;

public interface ICourseService
{
    Task<List<CourseListItemViewModel>> GetCourseListAsync();
    Task<Course?> GetCourseByIdAsync(int id);
    Task<List<CourseCategoryListItemViewModel>> GetCategoryListAsync();
    Task<List<CourseListItemViewModel>> GetFilteredCoursesAsync(int? categoryId, decimal? minPrice, decimal? maxPrice);
    
    // Lab05 CRUD
    Task<CourseDetailViewModel?> GetCourseDetailAsync(int id);
    Task<CourseEditViewModel?> GetCourseForEditAsync(int id);
    Task<bool> CreateCourseAsync(CourseCreateViewModel model);
    Task<bool> UpdateCourseAsync(CourseEditViewModel model);
    Task<bool> SoftDeleteCourseAsync(int id);
    Task<List<CourseTrashItemViewModel>> GetArchivedCoursesAsync();
    Task<bool> RestoreCourseAsync(int id);
    Task<bool> DeleteCourseForeverAsync(int id);
    Task<bool> IsCourseCodeUniqueAsync(string courseCode, int? excludeId = null);
    Task<List<CourseListItemViewModel>> SearchCoursesAsync(string? keyword, string? seatStatus);
    Task<CourseAdjustSeatsViewModel?> GetCourseForAdjustSeatsAsync(int id);
    Task<bool> AdjustSeatsAsync(CourseAdjustSeatsViewModel model);
    Task<(int importedCount, List<string> errors)> ImportCoursesFromCsvAsync(System.IO.Stream csvStream);
}
