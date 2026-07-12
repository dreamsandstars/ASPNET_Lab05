using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetWeek3.Mvc.Models;
using AspNetWeek3.Mvc.ViewModels;

namespace AspNetWeek3.Mvc.Services;

public interface IEnrollmentService
{
    Task CreateEnrollmentAsync(EnrollmentCreateViewModel model);
    Task<List<Enrollment>> GetEnrollmentHistoryAsync();
    Task<int> GetEnrollmentCountAsync();
    Task<int> GetStudentCountAsync();
}
