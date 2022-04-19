namespace RunPipeliner.Common.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoMapper;

    public static class AutoMapperConfig
    {
        private static List<Assembly> GetAssemblies(List<Assembly> allAssemblies, Assembly target)
        {
            var assemblies = target.GetReferencedAssemblies().Select(LoadAssembly)
                .Where(x => x != null && !IsMicrosoftAssembly(x) && !allAssemblies.Contains(x)).ToList();

            allAssemblies.AddRange(assemblies);

            foreach (var localAssembly in assemblies)
            {
                GetAssemblies(allAssemblies, localAssembly);
            }

            return allAssemblies;
        }

        private static bool IsMicrosoftAssembly(ICustomAttributeProvider assembly)
        {
            return assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)
                .OfType<AssemblyCompanyAttribute>().Any(attr => attr.Company.StartsWith("Microsoft"));
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            Assembly assembly = null;

            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (Exception e)
            {
                // Some assemblies are referenced but may not exist because they aren't used.
                Console.WriteLine(e.Message);
            }

            return assembly;
        }

        public static void Initialize()
        {
            var target = Assembly.GetCallingAssembly();

            var assemblies = new List<Assembly> { target };

            var assembliesToLoad = GetAssemblies(assemblies, target);

            assembliesToLoad.Add(target);

            LoadMapsFromAssemblies(assembliesToLoad.ToArray());
        }
        public static void LoadMapsFromAssemblies(params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(a => a.GetExportedTypes()).ToArray();

            //AutoMapper.MapperConfiguration();(cfg => Load(cfg, types));
            var config = new MapperConfiguration(cfg => Load(cfg, types));

            config.CreateMapper();
        }

        private static void Load(IMapperConfigurationExpression cfg, Type[] types)
        {
            LoadIMapFromMappings(cfg, types);
            //LoadIMapToMappings(cfg, types);

            LoadCustomMappings(cfg, types);

            //Bootstrap all custom mappings defined in profiles
            LoadAllProfiles(cfg, types);
        }

        private static void LoadAllProfiles(IMapperConfigurationExpression cfg, IEnumerable<Type> types)
        {
            var profiles = types.Where(type => type.IsSubclassOf(typeof(Profile)));

            foreach (var profile in profiles)
            {
                cfg.AddProfile(profile);
            }
        }

        private static void LoadCustomMappings(IMapperConfigurationExpression cfg, IEnumerable<Type> types)
        {
            var maps = (from t in types
                        from i in t.GetInterfaces()
                        where typeof(IHaveCustomMappings).IsAssignableFrom(t) &&
                              !t.IsAbstract &&
                              !t.IsInterface
                        select (IHaveCustomMappings)Activator.CreateInstance(t)).ToArray();

            foreach (var map in maps.Distinct())
            {
                map.CreateMappings(cfg);
            }
        }

        private static void LoadIMapFromMappings(IMapperConfigurationExpression cfg, IEnumerable<Type> types)
        {
            var maps = (from t in types
                        from i in t.GetInterfaces()
                        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                              !t.IsAbstract &&
                              !t.IsInterface
                        select new
                        {
                            Source = i.GetGenericArguments()[0],
                            Destination = t
                        }).ToArray();

            foreach (var map in maps.Distinct())
            {
                cfg.CreateMap(map.Source, map.Destination).ReverseMap();
            }
        }
    }
}
