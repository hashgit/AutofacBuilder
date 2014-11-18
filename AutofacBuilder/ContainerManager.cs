using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using AutofacBuilder.Utils;

namespace AutofacBuilder
{
    public static class ContainerManager
    {
        private static Assembly SafeLoadAssembly(string name)
        {
            try
            {
                name = string.IsNullOrWhiteSpace(_executingPath) ? name + ".dll" : _executingPath + "\\" + name + ".dll";
                return Assembly.LoadFrom(name);
            }
            catch (FileNotFoundException)
            {
            }

            return null;
        }

        private static void RegisterFromAssembly(this ContainerBuilder builder, string assemblyName, string baseTypeName, Func<Type, bool> qualifier = null)
        {
            var assembly = SafeLoadAssembly(assemblyName);
            if (assembly == null) return;

            var types = builder.RegisterAssemblyTypes(assembly);
            if (!string.IsNullOrWhiteSpace(baseTypeName))
            {
                var baseType = assembly.GetType(baseTypeName, true);
                types = types.Where(baseType.IsAssignableFrom);
            }

            if (qualifier != null) types.Where(qualifier);
            types.Where(IsSingleton).AsImplementedInterfaces().SingleInstance();
            types.Where(type => !IsSingleton(type)).AsImplementedInterfaces().ScopeCustom();
        }

        private static bool IsSingleton(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(SingleInstance)) != null;
        }

        public static IContainer BuildContainerWithApi(Assembly assembly)
        {
            var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _executingPath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            _containerType = "API";

            var builder = BuildContainerInternal();
            builder.RegisterApiControllers(assembly);
            return BuildAndSet(builder);
        }

        public static IContainer BuildContainerWithWeb(Assembly assembly)
        {
            var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _executingPath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            _containerType = "WEB";

            var builder = BuildContainerInternal();
            builder.RegisterControllers(assembly);
            builder.RegisterSource(new ViewRegistrationSource());
            builder.RegisterFilterProvider();
            return BuildAndSet(builder);
        }

        private static IContainer BuildAndSet(ContainerBuilder builder)
        {
            Current = builder.Build();
            return Current;
        }

        public static IContainer Current { get; set; }

        private static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> ScopeCustom<T>(this IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> builder)
        {
            switch (_containerType)
            {
                case "API":
                    return builder.InstancePerRequest();
                case "WEB":
                    return builder.InstancePerRequest();
                default:
                    return builder.InstancePerDependency();
            }
        }

        private static IRegistrationBuilder<T, ScanningActivatorData, DynamicRegistrationStyle> ScopeCustom<T>(this IRegistrationBuilder<T, ScanningActivatorData, DynamicRegistrationStyle> builder)
        {
            switch (_containerType)
            {
                case "API":
                    return builder.InstancePerRequest();
                case "WEB":
                    return builder.InstancePerRequest();
                default:
                    return builder.InstancePerDependency();
            }
        }

        private static string _executingPath;
        private static string _containerType;

        private static ContainerBuilder BuildContainerInternal()
        {
            var builder = new ContainerBuilder();
            // sample
            builder.RegisterFromAssembly("MyApp.Logger", "MyApp.Logger.GlobalLogger");

            return builder;
        }

        public static IContainer BuildContainer()
        {
            var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _executingPath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            return BuildAndSet(BuildContainerInternal());
        }
    }
}