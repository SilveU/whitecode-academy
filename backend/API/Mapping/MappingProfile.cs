using Application.DTOs.Authentication;
using Application.DTOs.Core;
using Application.DTOs.Core.Requests;
using Application.DTOs.Profile;
using Application.Features.Courses.Commands.CreateCourse;
using Application.Features.Courses.Commands.UpdateCourse;
using Application.Features.Departments.Commands.CreateDepartment;
using Application.Features.Departments.Commands.UpdateDepartment;
using Application.Features.Instructors.Commands.AssignInstructor;
using Application.Features.Instructors.Commands.UpdateInstructor;
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
            // ── Auth ─────────────────────────────────────────────────────
            CreateMap<ApplicationUser, AuthResponse>();
            CreateMap<LoginRequest, ApplicationUser>();
            CreateMap<RegisterRequest, ApplicationUser>();

            // ── Course ───────────────────────────────────────────────────
            CreateMap<Course, CourseResponse>();

            // Request → Command  (controller maps this; JWT fields are injected after)
            CreateMap<CreateCourseRequest, CreateCourseCommand>()
                .ForMember(dest => dest.CurrentUserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsInstructor,  opt => opt.Ignore());

            CreateMap<UpdateCourseRequest, UpdateCourseCommand>()
                .ForMember(dest => dest.Id,            opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsInstructor,  opt => opt.Ignore());

            // Command → Entity  (handler maps this)
            CreateMap<CreateCourseCommand, Course>()
                .ForMember(dest => dest.InstructorId,  opt => opt.Ignore())
                .ForSourceMember(src => src.CurrentUserId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsInstructor,  opt => opt.DoNotValidate());

            CreateMap<UpdateCourseCommand, Course>()
                .ForSourceMember(src => src.CurrentUserId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsInstructor,  opt => opt.DoNotValidate());

            // ── Department ───────────────────────────────────────────────
            CreateMap<Department, DepartmentResponse>();

            // Request → Command
            CreateMap<CreateDepartmentRequest, CreateDepartmentCommand>()
                .ForMember(dest => dest.ImageFile, opt => opt.MapFrom(src => src.ImageFile));

            CreateMap<UpdateDepartmentRequest, UpdateDepartmentCommand>()
                .ForMember(dest => dest.Id,        opt => opt.Ignore())
                .ForMember(dest => dest.ImageFile, opt => opt.MapFrom(src => src.ImageFile));

            // Command → Entity  (ImageUrl is set manually in the handler after upload)
            CreateMap<CreateDepartmentCommand, Department>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForSourceMember(src => src.ImageFile, opt => opt.DoNotValidate());

            CreateMap<UpdateDepartmentCommand, Department>()
                .ForSourceMember(src => src.ImageFile, opt => opt.DoNotValidate());

            // ── Section ──────────────────────────────────────────────────
            CreateMap<Section, SectionResponse>()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(x => x.DayOfWeek.ToString()));

            // Request → Command  (IFormFile fields map by name: VideoFile, PdfFile)
            CreateMap<CreateSectionRequest, CreateSectionCommand>()
                .ForMember(dest => dest.CurrentUserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsInstructor,  opt => opt.Ignore());

            CreateMap<UpdateSectionRequest, UpdateSectionCommand>()
                .ForMember(dest => dest.Id,            opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsInstructor,  opt => opt.Ignore());

            // Command → Entity  (VideoUrl/PdfUrl are set manually in the handler after upload)
            CreateMap<CreateSectionCommand, Section>()
                .ForMember(dest => dest.VideoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.PdfUrl,   opt => opt.Ignore())
                .ForSourceMember(src => src.VideoFile, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.PdfFile,   opt => opt.DoNotValidate())
                .ForSourceMember(src => src.CurrentUserId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsInstructor,  opt => opt.DoNotValidate());

            // ── Instructor ───────────────────────────────────────────────
            CreateMap<Instructor, InstructorResponse>()
                .ForMember(dest => dest.FirstName,      opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName,       opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email,          opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));

            // Request → Command
            CreateMap<AssignInstructorRequest, AssignInstructorCommand>();

            CreateMap<UpdateInstructorRequest, UpdateInstructorCommand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // ── Enrollment ───────────────────────────────────────────────
            CreateMap<Enrollment, EnrollmentResponse>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty));

            // ── Student ──────────────────────────────────────────────────
            CreateMap<Student, StudentResponse>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName,  opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email,     opt => opt.MapFrom(src => src.User.Email));

            // ── Profile ──────────────────────────────────────────────────
            CreateMap<ApplicationUser, ProfileResponse>();
        }
    }
}
