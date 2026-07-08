using Application.DTOs.Authentication;
using Application.DTOs.Core;
using Application.Features.Courses.Commands.CreateCourse;
using Application.Features.Courses.Commands.UpdateCourse;
using Application.Features.Departments.Commands.CreateDepartment;
using Application.Features.Departments.Commands.UpdateDepartment;
using Application.Features.Sections.Commands.CreateSection;
using Application.Features.Sections.Commands.UpdateSection;
using AutoMapper;
using Domain.Entites.Core;
using Domain.Entites.Users;

namespace API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── User ────────────────────────────────────────────────────
            CreateMap<ApplicationUser, AuthResponse>();
            CreateMap<LoginRequest, ApplicationUser>();
            CreateMap<RegisterRequest, ApplicationUser>();

            // ── Course ──────────────────────────────────────────────────
            CreateMap<Course, CourseResponse>();
            CreateMap<CreateCourseCommand, Course>()
                .ForMember(dest => dest.InstructorId, opt => opt.Ignore()) // Set manually in the handler
                .ForSourceMember(src => src.CurrentUserId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsInstructor,  opt => opt.DoNotValidate());
            CreateMap<UpdateCourseCommand, Course>()
                .ForSourceMember(src => src.CurrentUserId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsInstructor,  opt => opt.DoNotValidate());

            // ── Section ─────────────────────────────────────────────────
            CreateMap<Section, SectionResponse>();
            CreateMap<CreateSectionCommand, Section>()
                .ForSourceMember(src => src.CurrentUserId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsInstructor,  opt => opt.DoNotValidate());
            CreateMap<UpdateSectionCommand, Section>()
                .ForSourceMember(src => src.CurrentUserId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsInstructor,  opt => opt.DoNotValidate());

            // ── Department ──────────────────────────────────────────────
            CreateMap<Department, DepartmentResponse>();
            CreateMap<CreateDepartmentCommand, Department>();
            CreateMap<UpdateDepartmentCommand, Department>();


            CreateMap<Enrollment, EnrollmentResponse>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty));

            // ── Instructor ──────────────────────────────────────────────
            CreateMap<Instructor, InstructorResponse>()
                .ForMember(dest => dest.FirstName,       opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName,        opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email,           opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.DepartmentName,  opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));

            // ── Student ─────────────────────────────────────────────────
            CreateMap<Student, StudentResponse>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName,  opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email,     opt => opt.MapFrom(src => src.User.Email));
        }
    }
}
