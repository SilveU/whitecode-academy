using Application.Common;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Departments.Commands.DeleteDepartment
{
    public class DeleteDepartmentHandler : IRequestHandler<DeleteDepartmentCommand, Result<bool>>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DeleteDepartmentHandler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<Result<bool>> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _departmentRepository.GetByIdAsync(request.Id);
            if (department == null)
                return Result<bool>.NotFound($"Department with ID {request.Id} not found.");

            var hasActiveDependencies = await _departmentRepository.HasActiveCoursesOrInstructorsAsync(request.Id);
            if (hasActiveDependencies)
                return Result<bool>.Failure(
                    "Cannot delete this department because it has active courses or instructors assigned to it.", 409);

            _departmentRepository.Delete(department);
            await _departmentRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
