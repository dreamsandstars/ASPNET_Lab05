using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AspNetWeek3.Mvc.Data;
using AspNetWeek3.Mvc.Models;

namespace AspNetWeek3.Mvc.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Course>> GetAllAsync()
        => _context.Courses.Include(p => p.CourseCategory).ToListAsync();

    public Task<List<Course>> GetAllReadOnlyAsync()
        => _context.Courses.Include(p => p.CourseCategory).AsNoTracking().ToListAsync();

    public Task<Course?> GetByIdAsync(int id)
        => _context.Courses.Include(p => p.CourseCategory).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Course course)
        => await _context.Courses.AddAsync(course);

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();

    public Task<List<CourseCategory>> GetAllCategoriesReadOnlyAsync()
        => _context.CourseCategories.Include(c => c.Courses).AsNoTracking().ToListAsync();

    public async Task<List<Course>> GetFilteredReadOnlyAsync(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var query = _context.Courses.Include(c => c.CourseCategory).AsNoTracking().AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CourseCategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(c => c.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= maxPrice.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<List<Course>> SearchActiveReadOnlyAsync(string? keyword, string? seatStatus, int lowSeatThreshold)
    {
        var query = _context.Courses.Include(c => c.CourseCategory).AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            var cleanKeyword = keyword.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(cleanKeyword) || c.CourseCode.ToLower().Contains(cleanKeyword));
        }

        if (!string.IsNullOrEmpty(seatStatus))
        {
            if (seatStatus.Equals("low", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.AvailableSeats <= lowSeatThreshold);
            }
            else if (seatStatus.Equals("available", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.AvailableSeats > lowSeatThreshold);
            }
        }

        return await query.ToListAsync();
    }
}
