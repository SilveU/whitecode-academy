using Domain.Entites.Core;

namespace Application.Interfaces.Repositories
{
    public interface ISectionRepository : IRepository<Section>
    {
        Task<Section?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<IEnumerable<Section>> GetByCourseIdAsync(Guid courseId);
        void Update(Section section);
        void Delete(Section section);
    }
}