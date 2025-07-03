using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Core.Contracts;

public interface IViewConfigurator<in TDbContext, out TView> where TDbContext : DbContext  where TView : class
{
    public IQueryable<TView> ConfigureView(TDbContext dbContext);
}