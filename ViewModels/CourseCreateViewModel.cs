using System.ComponentModel.DataAnnotations;

namespace AspNetWeek3.Mvc.ViewModels;

public class CourseCreateViewModel
{
    [Required(ErrorMessage = "Tên khóa học là bắt buộc.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Tên khóa học phải từ 5 đến 100 ký tự.")]
    [Display(Name = "Tên khóa học")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã khóa học là bắt buộc.")]
    [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Mã khóa học chỉ gồm chữ in hoa, số và dấu -. (Ví dụ: CS-101, ASP-MVC)")]
    [Display(Name = "Mã khóa học")]
    public string CourseCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Học phí là bắt buộc.")]
    [Range(0, 50000000, ErrorMessage = "Học phí phải từ 0đ đến 50.000.000đ.")]
    [Display(Name = "Học phí (VND)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Số chỗ trống là bắt buộc.")]
    [Range(1, 1000, ErrorMessage = "Số chỗ trống phải từ 1 đến 1.000.")]
    [Display(Name = "Số chỗ trống")]
    public int AvailableSeats { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục khóa học.")]
    [Display(Name = "Danh mục khóa học")]
    public int CourseCategoryId { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    [Display(Name = "Mô tả chi tiết")]
    public string? Description { get; set; }
}
