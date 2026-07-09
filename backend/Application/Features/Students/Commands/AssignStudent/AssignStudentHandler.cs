using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Students.Commands.AssignStudent
{
    public class AssignStudentHandler : IRequestHandler<AssignStudentCommand, Result<StudentResponse>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AssignStudentHandler(
            IStudentRepository studentRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<StudentResponse>> Handle(AssignStudentCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<StudentResponse>.NotFound($"User found.");

            var existingStudent = await _studentRepository.GetByUserIdAsync(request.UserId);
            if (existingStudent != null)
                return Result<StudentResponse>.Failure("This user is already registered as a student.", 409);

            var student = new Student
            {
                UserId    = request.UserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _studentRepository.CreateAsync(student);
            await _studentRepository.SaveChangesAsync();

            var created = await _studentRepository.GetByIdWithNavigationPropertiesAsync(student.Id);
            return Result<StudentResponse>.Success(_mapper.Map<StudentResponse>(created!), 201);
        }
    }
}
