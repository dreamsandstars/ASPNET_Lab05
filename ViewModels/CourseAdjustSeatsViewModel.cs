using System.ComponentModel.DataAnnotations;

namespace AspNetWeek3.Mvc.ViewModels;

public class CourseAdjustSeatsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int CurrentSeats { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số chỗ thay đổi.")]
    public int? SeatChange { get; set; }

    public string? RowVersion { get; set; }
}
