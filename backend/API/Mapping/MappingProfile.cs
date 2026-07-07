using Application.DTOs.Authentication;
using Application.DTOs.Core;
using Application.Features.Courses.Commands.CreateCourse;
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
            CreateMap<CreateCourseCommand, Course>();
        }   
    }
}