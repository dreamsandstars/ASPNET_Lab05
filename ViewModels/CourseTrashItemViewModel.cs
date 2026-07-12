using System;

namespace AspNetWeek3.Mvc.ViewModels;

public class CourseTrashItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
