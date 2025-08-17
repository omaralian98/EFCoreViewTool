using System;
using System.Linq;
using System.Reflection;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFCoreViewTool.Core.Services
{
    public class DbContextFactoryService(ILogger<DbContextFactoryService> logger) : IDbContextFactoryService
    {
        public DbContext CreateDbContext(Type dbContextType, ProjectInfo context, string? dbContextName = null)
        {
            var dbContext = TryCreateWithServiceProvider2(dbContextType, Assembly.LoadFrom(context.StartupAssembly), context.StartupAssembly);
            if (dbContext != null)
            {
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

/*
 using System.Reflection;
   using System.Runtime.Loader;
   using EFCoreViewTool.Core.Interfaces;
   using EFCoreViewTool.Core.Models;
   using Microsoft.EntityFrameworkCore;
   using Microsoft.EntityFrameworkCore.Design;
   using Microsoft.EntityFrameworkCore.Design.Internal;
   using Microsoft.Extensions.Logging;
   
   namespace EFCoreViewTool.Core.Services
   {
       public class DbContextFactoryService(ILogger<DbContextFactoryService> logger) : IDbContextFactoryService
       {
           public DbContext CreateDbContext(Type dbContextType, ProjectInfo context, string? dbContextName = null)
           {
               //var startupAssembly = Assembly.LoadFrom(context.StartupAssembly);
               //var projectAssembly = Assembly.LoadFrom(context.Assembly);
               
               // 1. Try IDesignTimeDbContextFactory in project assembly
               // var dbContext = TryCreateWithDesignTimeFactory(dbContextType, [startupAssembly, projectAssembly]);
               // if (dbContext is not null)
               // {
               //     return dbContext;
               // }
   
               var loader = new CustomLoadContext(context.StartupAssembly);
               var startupAssembly = loader.LoadFromAssemblyPath(context.StartupAssembly);
               
               // 2. Try DI from startup assembly
               var dbContext = TryCreateWithServiceProvider(dbContextType, startupAssembly, context.StartupAssembly);
               if (dbContext is not null)
               {
                   return dbContext;
               }
               
               // 3. Try parameterless constructor With OnConfiguration
               dbContext = TryCreateWithParameterlessConstructor(dbContextType);
               if (dbContext is not null)
               {
                   return dbContext;
               }
   
               throw new InvalidOperationException(
                   $"Could not create an instance of DbContext type '{dbContextType.FullName}'. " +
                   $"Ensure there is a parameterless constructor, a DI setup, or an IDesignTimeDbContextFactory<{dbContextType.Name}> implementation.");
           }
   
           private static DbContext? TryCreateWithDesignTimeFactory(Type dbContextType, List<Assembly> assemblies)
           {
               var factoryType = assemblies.Distinct().SelectMany(x => x.GetTypes())
                   .FirstOrDefault(t =>
                       t.GetInterfaces().Any(i =>
                           i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>) &&
                           i.GenericTypeArguments[0] == dbContextType));
   
               if (factoryType is null)
               {
                   return null;
               }
               
               var factoryInstance = Activator.CreateInstance(factoryType);
               var createMethod = factoryType.GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext));
   
               var dbContext = createMethod?.Invoke(factoryInstance, [Array.Empty<string>()]) as DbContext;
               return dbContext;
           }
           
           private DbContext? TryCreateWithServiceProvider(Type dbContextType, Assembly startupAssembly, string pathtoDllFile)
           {
               var originalDirectory = Directory.GetCurrentDirectory();
               try
               {
                   //Directory.SetCurrentDirectory(Path.GetDirectoryName(pathtoDllFile)??originalDirectory);
                   // var dllfiles = Directory.GetFiles(Directory.GetCurrentDirectory(),  "*.dll");
                   // foreach (var dllfile in dllfiles)
                   // {
                   //     Assembly.LoadFrom(dllfile);
                   // }
   #pragma warning disable EF1001
                   var reporter = new OperationReporter(null);
                   var factory = new AppServiceProviderFactory(startupAssembly, reporter);
                   var appServices = factory.Create([]);
   #pragma warning restore EF1001
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
               var ctor = dbContextType.GetConstructor(
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                   binder: null,
                   Type.EmptyTypes,
                   modifiers: null
               );
                   
               if (ctor is null)
               {
                   return null;
               }
               
               var dbContext = Activator.CreateInstance(dbContextType) as DbContext;
               return dbContext;
           }
       }
   }
   
   
   public class CustomLoadContext : AssemblyLoadContext
   {
       private readonly AssemblyDependencyResolver _resolver;
       
       public CustomLoadContext(string mainAssemblyPath) : base(isCollectible: true)
       {
           Directory.SetCurrentDirectory(Path.GetDirectoryName(mainAssemblyPath));
           _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
       }
       protected override Assembly? Load(AssemblyName assemblyName)
       {
           
           // resolve where the assembly physically lives
           string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
           if (assemblyPath != null)
           {
               return LoadFromAssemblyPath(assemblyPath);
           }
   
           return null;
       }
       
       protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
       {
           var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
           if (path != null)
           {
               return LoadUnmanagedDllFromPath(path);
           }
           return IntPtr.Zero;
       }
   }
   
 */