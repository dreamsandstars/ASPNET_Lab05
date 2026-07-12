using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetWeek3.Mvc.Models;

namespace AspNetWeek3.Mvc.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync();
    Task<List<Course>> GetAllReadOnlyAsync();
    Task<Course?> GetByIdAsync(int id);
    Task AddAsync(Course course);
    Task SaveChangesAsync();
    Task<List<CourseCategory>> GetAllCategoriesReadOnlyAsync();
    Task<List<Course>> GetFilteredReadOnlyAsync(int? categoryId, decimal? minPrice, decimal? maxPrice);
    Task<List<Course>> SearchActiveReadOnlyAsync(string? keyword, string? seatStatus, int lowSeatThreshold);
}
