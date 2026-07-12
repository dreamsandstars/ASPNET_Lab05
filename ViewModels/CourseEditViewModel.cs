namespace AspNetWeek3.Mvc.ViewModels;

public class CourseEditViewModel : CourseCreateViewModel
{
    public int Id { get; set; }
    public string? RowVersion { get; set; }
}
