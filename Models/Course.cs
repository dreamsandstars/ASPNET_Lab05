using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetWeek3.Mvc.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string CourseCode { get; set; } = string.Empty; // Added in Feature 3

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int CourseCategoryId { get; set; }
    public CourseCategory? CourseCategory { get; set; }

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public byte[]? RowVersion { get; set; }
}
