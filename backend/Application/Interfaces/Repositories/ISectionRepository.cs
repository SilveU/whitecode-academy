using Domain.Entites.Core;

namespace Application.Interfaces.Repositories
{
    public interface ISectionRepository : IRepository<Section>
    {
        Task<Section?> GetByIdWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Section>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
        void Update(Section section);
        void Delete(Section section);
    }
}