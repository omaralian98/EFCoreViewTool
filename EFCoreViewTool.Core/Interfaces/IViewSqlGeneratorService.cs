using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Core.Interfaces;

public interface IViewSqlGeneratorService
{
    public string GenerateViewSql(ViewConfiguratorInfo info, DbContext dbContext);
}