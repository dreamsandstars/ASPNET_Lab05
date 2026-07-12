using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AspNetWeek3.Mvc.Data;
using AspNetWeek3.Mvc.Models;
using AspNetWeek3.Mvc.Options;
using AspNetWeek3.Mvc.Repositories;
using AspNetWeek3.Mvc.ViewModels;

namespace AspNetWeek3.Mvc.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;
    private readonly ICourseRepository _courseRepository;
    private readonly AppSettings _settings;

    public CourseService(AppDbContext context, ICourseRepository courseRepository, IOptions<AppSettings> options)
    {
        _context = context;
        _courseRepository = courseRepository;
        _settings = options.Value;
    }

    public async Task<List<CourseListItemViewModel>> GetCourseListAsync()
    {
        var courses = await _context.Courses
            .Include(c => c.CourseCategory)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return courses.Select(MapToViewModel).ToList();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _courseRepository.GetByIdAsync(id);
    }

    public async Task<List<CourseCategoryListItemViewModel>> GetCategoryListAsync()
    {
        var categories = await _courseRepository.GetAllCategoriesReadOnlyAsync();
        return categories.Select(c => new CourseCategoryListItemViewModel
        {
            Id = c.Id,
            Name = c.Name,
            CourseCount = c.Courses.Count
        }).ToList();
    }

    public async Task<List<CourseListItemViewModel>> GetFilteredCoursesAsync(int? categoryId, decimal? minPrice, decimal? maxPrice)
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

        var courses = await query.ToListAsync();
        return courses.Select(MapToViewModel).ToList();
    }

    // Lab05 CRUD
    public async Task<CourseDetailViewModel?> GetCourseDetailAsync(int id)
    {
        var course = await _context.Courses
            .IgnoreQueryFilters()
            .Include(c => c.CourseCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return null;

        return new CourseDetailViewModel
        {
            Id = course.Id,
            Name = course.Name,
            CourseCode = course.CourseCode,
            Price = course.Price,
            AvailableSeats = course.AvailableSeats,
            CategoryName = course.CourseCategory != null ? course.CourseCategory.Name : "N/A",
            Description = course.Description,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            IsDeleted = course.IsDeleted,
            DeletedAt = course.DeletedAt,
            RowVersion = course.RowVersion != null ? Convert.ToBase64String(course.RowVersion) : string.Empty
        };
    }

    public async Task<CourseEditViewModel?> GetCourseForEditAsync(int id)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return null;

        return new CourseEditViewModel
        {
            Id = course.Id,
            Name = course.Name,
            CourseCode = course.CourseCode,
            Price = course.Price,
            AvailableSeats = course.AvailableSeats,
            CourseCategoryId = course.CourseCategoryId,
            Description = course.Description,
            RowVersion = course.RowVersion != null ? Convert.ToBase64String(course.RowVersion) : string.Empty
        };
    }

    public async Task<bool> CreateCourseAsync(CourseCreateViewModel model)
    {
        var course = new Course
        {
            Name = model.Name,
            CourseCode = model.CourseCode,
            Price = model.Price,
            AvailableSeats = model.AvailableSeats,
            CourseCategoryId = model.CourseCategoryId,
            Description = model.Description,
            IsDeleted = false
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCourseAsync(CourseEditViewModel model)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.Id);
        if (course == null) return false;

        course.Name = model.Name;
        course.CourseCode = model.CourseCode;
        course.Price = model.Price;
        course.AvailableSeats = model.AvailableSeats;
        course.CourseCategoryId = model.CourseCategoryId;
        course.Description = model.Description;

        if (!string.IsNullOrEmpty(model.RowVersion))
        {
            _context.Entry(course).Property("RowVersion").OriginalValue =
                Convert.FromBase64String(model.RowVersion);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteCourseAsync(int id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return false;

        course.IsDeleted = true;
        course.DeletedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CourseTrashItemViewModel>> GetArchivedCoursesAsync()
    {
        return await _context.Courses
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(c => c.DeletedAt)
            .Select(c => new CourseTrashItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                CourseCode = c.CourseCode,
                DeletedAt = c.DeletedAt
            })
            .ToListAsync();
    }

    public async Task<bool> RestoreCourseAsync(int id)
    {
        var course = await _context.Courses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);

        if (course == null) return false;

        course.IsDeleted = false;
        course.DeletedAt = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCourseForeverAsync(int id)
    {
        var course = await _context.Courses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsCourseCodeUniqueAsync(string courseCode, int? excludeId = null)
    {
        return !await _context.Courses
            .IgnoreQueryFilters()
            .AnyAsync(c => c.CourseCode == courseCode && (excludeId == null || c.Id != excludeId));
    }

    public async Task<List<CourseListItemViewModel>> SearchCoursesAsync(string? keyword, string? seatStatus)
    {
        var courses = await _courseRepository.SearchActiveReadOnlyAsync(keyword, seatStatus, _settings.LowSeatThreshold);
        return courses.Select(MapToViewModel).ToList();
    }

    public async Task<CourseAdjustSeatsViewModel?> GetCourseForAdjustSeatsAsync(int id)
    {
        var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return null;

        return new CourseAdjustSeatsViewModel
        {
            Id = course.Id,
            Name = course.Name,
            CurrentSeats = course.AvailableSeats,
            RowVersion = course.RowVersion != null ? Convert.ToBase64String(course.RowVersion) : string.Empty
        };
    }

    public async Task<bool> AdjustSeatsAsync(CourseAdjustSeatsViewModel model)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.Id);
        if (course == null) return false;

        int newSeats = course.AvailableSeats + model.SeatChange.GetValueOrDefault();
        if (newSeats < 0) return false;

        course.AvailableSeats = newSeats;
        course.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrEmpty(model.RowVersion))
        {
            _context.Entry(course).Property("RowVersion").OriginalValue = Convert.FromBase64String(model.RowVersion);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private CourseListItemViewModel MapToViewModel(Course c)
    {
        return new CourseListItemViewModel
        {
            Id = c.Id,
            CourseCode = c.CourseCode,
            Name = c.Name,
            Price = c.Price,
            AvailableSeats = c.AvailableSeats,
            CategoryName = c.CourseCategory != null ? c.CourseCategory.Name : "N/A",
            IsLowSeats = c.AvailableSeats <= _settings.LowSeatThreshold
        };
    }

    public async Task<(int importedCount, List<string> errors)> ImportCoursesFromCsvAsync(System.IO.Stream csvStream)
    {
        var errors = new List<string>();
        var newCourses = new List<Course>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new System.IO.StreamReader(csvStream);
        string? header = await reader.ReadLineAsync(); // Skip header
        if (header == null)
        {
            errors.Add("Tệp tin CSV trống.");
            return (0, errors);
        }

        int lineNum = 1;
        while (!reader.EndOfStream)
        {
            lineNum++;
            string? line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Simple CSV split (comma separated)
            var parts = line.Split(',')
                            .Select(p => p.Trim().Trim('"'))
                            .ToArray();

            if (parts.Length < 5)
            {
                errors.Add($"Dòng {lineNum}: Không đủ cột dữ liệu (Yêu cầu ít nhất 5 cột: Code, Tên, Giá, Ghế, CategoryId).");
                continue;
            }

            var code = parts[0];
            var name = parts[1];
            var priceStr = parts[2];
            var seatsStr = parts[3];
            var categoryIdStr = parts[4];
            var description = parts.Length > 5 ? parts[5] : null;

            if (string.IsNullOrWhiteSpace(code))
            {
                errors.Add($"Dòng {lineNum}: Mã khóa học không được trống.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add($"Dòng {lineNum}: Tên khóa học không được trống.");
                continue;
            }

            if (!decimal.TryParse(priceStr, out var price) || price < 0)
            {
                errors.Add($"Dòng {lineNum}: Học phí '{priceStr}' không hợp lệ hoặc nhỏ hơn 0.");
                continue;
            }

            if (!int.TryParse(seatsStr, out var seats) || seats < 0)
            {
                errors.Add($"Dòng {lineNum}: Số chỗ trống '{seatsStr}' không hợp lệ hoặc nhỏ hơn 0.");
                continue;
            }

            if (!int.TryParse(categoryIdStr, out var categoryId))
            {
                errors.Add($"Dòng {lineNum}: ID danh mục '{categoryIdStr}' không phải là số hợp lệ.");
                continue;
            }

            // Check category presence
            var categoryExists = await _context.CourseCategories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                errors.Add($"Dòng {lineNum}: Danh mục ID {categoryId} không tồn tại.");
                continue;
            }

            // Check code uniqueness in batch
            if (seenCodes.Contains(code))
            {
                errors.Add($"Dòng {lineNum}: Mã khóa học '{code}' bị trùng lặp trong tệp CSV.");
                continue;
            }
            seenCodes.Add(code);

            // Check code uniqueness in DB
            var dbCodeExists = await _context.Courses.AnyAsync(c => c.CourseCode == code);
            if (dbCodeExists)
            {
                errors.Add($"Dòng {lineNum}: Mã khóa học '{code}' đã tồn tại trong cơ sở dữ liệu.");
                continue;
            }

            var course = new Course
            {
                CourseCode = code,
                Name = name,
                Price = price,
                AvailableSeats = seats,
                CourseCategoryId = categoryId,
                Description = description,
                IsDeleted = false
            };

            newCourses.Add(course);
        }

        if (errors.Any())
        {
            return (0, errors);
        }

        if (!newCourses.Any())
        {
            errors.Add("Không tìm thấy dòng dữ liệu nào để nhập.");
            return (0, errors);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var course in newCourses)
            {
                _context.Courses.Add(course);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (newCourses.Count, errors);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            errors.Add($"Lỗi lưu cơ sở dữ liệu: {ex.Message}");
            return (0, errors);
        }
    }
}
