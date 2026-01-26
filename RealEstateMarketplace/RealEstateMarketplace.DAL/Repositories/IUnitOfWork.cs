namespace RealEstateMarketplace.DAL.Repositories;

public interface IUnitOfWork : IDisposable
{
    IPropertyRepository Properties { get; }
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
}
