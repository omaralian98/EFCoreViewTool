using System;
using System.Linq;
using System.Reflection;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreViewTool.Core.Services
{
    public class DbContextFactoryService : IDbContextFactoryService
    {
        public DbContext CreateDbContext(Type dbContextType, ProjectContextInfo context, string? dbContextName = null)
        {
            var dbContext = TryCreateWithServiceProvider2(dbContextType, context.StartupAssembly, context.StartupDllPath);
            if (dbContext != null)
            {
                Console.WriteLine("Here MOther fuckler");
                return dbContext;
            }
            // 1. Try IDesignTimeDbContextFactory in project assembly
            // var dbContext = TryCreateWithDesignTimeFactory(dbContextType, context.ProjectAssembly);
            // if (dbContext != null)
            //     return dbContext;
            //
            // // 3. Try DI from startup assembly
            // dbContext = TryCreateWithServiceProvider(dbContextType, context.StartupAssembly);
            // if (dbContext != null)
            //     return dbContext;
            
            // // 4. Try parameterless constructor
            // dbContext = TryCreateWithParameterlessConstructor(dbContextType);
            // if (dbContext != null)
            //     return dbContext;

            throw new InvalidOperationException(
                $"Could not create an instance of DbContext type '{dbContextType.FullName}'. " +
                $"Ensure there is a public parameterless constructor, a DI setup, or an IDesignTimeDbContextFactory<{dbContextType.Name}> implementation.");
        }

        private DbContext? TryCreateWithDesignTimeFactory(Type dbContextType, Assembly assembly)
        {
            var factoryType = assembly
                .GetTypes()
                .FirstOrDefault(t =>
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>) &&
                        i.GenericTypeArguments[0] == dbContextType));

            if (factoryType != null)
            {
                var factoryInstance = Activator.CreateInstance(factoryType);
                var createMethod = factoryType.GetMethod("CreateDbContext");
                if (createMethod != null)
                {
                    var dbContext = createMethod.Invoke(factoryInstance, new object[] { Array.Empty<string>() }) as DbContext;
                    if (dbContext != null)
                        return dbContext;
                }
            }
            return null;
        }
        
        private DbContext? TryCreateWithServiceProvider2(Type dbContextType, Assembly startupAssembly, string pathtoDllFile)
        {
            var originalDirectory = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(pathtoDllFile)??originalDirectory);
                var dllfiles = Directory.GetFiles(Directory.GetCurrentDirectory(),  "*.dll");
                foreach (var dllfile in dllfiles)
                {
                    Assembly.LoadFrom(dllfile);
                }
                var reporter = new OperationReporter(null); // You can inject or pass a real reporter if you want logs
                var factory = new AppServiceProviderFactory(startupAssembly, reporter);
                var appServices = factory.Create([]);
                var dbContext = appServices.GetService(dbContextType) as DbContext;
                return dbContext;
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }
        
        private DbContext? TryCreateWithParameterlessConstructor(Type dbContextType)
        {
            var ctor = dbContextType.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
            {
                var dbContext = Activator.CreateInstance(dbContextType) as DbContext;
                if (dbContext != null)
                    return dbContext;
            }
            return null;
        }
    }
}