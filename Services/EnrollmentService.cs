using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AspNetWeek3.Mvc.Data;
using AspNetWeek3.Mvc.Models;
using AspNetWeek3.Mvc.Repositories;
using AspNetWeek3.Mvc.ViewModels;

namespace AspNetWeek3.Mvc.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly AppDbContext _context;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository, AppDbContext context)
    {
        _enrollmentRepository = enrollmentRepository;
        _context = context;
    }

    public async Task CreateEnrollmentAsync(EnrollmentCreateViewModel model)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Find or create Student
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.StudentEmail);
            if (student == null)
            {
                student = new Student
                {
                    Name = model.StudentName,
                    Email = model.StudentEmail
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            // Find Course and verify available seats
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.CourseId);
            if (course == null)
            {
                throw new Exception("Selected course not found.");
            }

            if (course.AvailableSeats < model.Quantity)
            {
                throw new Exception($"Not enough seats available. Remaining seats: {course.AvailableSeats}, requested: {model.Quantity}");
            }

            // Calculate Promo Code Discount
            decimal discountPercentage = 0;
            if (!string.IsNullOrWhiteSpace(model.PromoCode))
            {
                var cleanPromo = model.PromoCode.Trim().ToUpper();
                if (cleanPromo == "KM10")
                {
                    discountPercentage = 0.10M;
                }
                else if (cleanPromo == "KM20")
                {
                    discountPercentage = 0.20M;
                }
                else if (cleanPromo == "WELCOME")
                {
                    discountPercentage = 0.15M;
                }
                else
                {
                    throw new Exception("Mã giảm giá không hợp lệ. Vui lòng kiểm tra lại.");
                }
            }

            decimal finalUnitPrice = course.Price * (1 - discountPercentage);

            // Create Enrollment
            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CreatedAt = DateTime.Now,
                TotalAmount = finalUnitPrice * model.Quantity
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Create EnrollmentDetail
            var detail = new EnrollmentDetail
            {
                EnrollmentId = enrollment.Id,
                CourseId = course.Id,
                Quantity = model.Quantity,
                UnitPrice = finalUnitPrice
            };
            _context.EnrollmentDetails.Add(detail);

            // Deduct available seats
            course.AvailableSeats -= model.Quantity;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<List<Enrollment>> GetEnrollmentHistoryAsync()
    {
        return _enrollmentRepository.GetAllReadOnlyAsync();
    }

    public Task<int> GetEnrollmentCountAsync()
    {
        return _enrollmentRepository.GetCountAsync();
    }

    public Task<int> GetStudentCountAsync()
    {
        return _enrollmentRepository.GetStudentCountAsync();
    }
}
