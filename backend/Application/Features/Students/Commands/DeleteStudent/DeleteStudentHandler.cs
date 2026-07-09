using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentHandler : IRequestHandler<DeleteStudentCommand, Result<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteStudentHandler(
            IStudentRepository studentRepository,
            IEnrollmentRepository enrollmentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
            _userManager = userManager;
        }

        public async Task<Result<bool>> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            var student = await _studentRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (student == null)
                return Result<bool>.NotFound($"Student not found.");

            // Soft-delete all active enrollments first
            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.Id);
            foreach (var enrollment in enrollments)
                _enrollmentRepository.Delete(enrollment);

            _studentRepository.Delete(student);

            await _studentRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
