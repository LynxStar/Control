using Newtonsoft.Json;
using Shift.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shift.Services
{
    
    public class FrameworkAssembly
    {
        public string Path { get; set; }
        public string AssemblyName { get; set; }
        public string PublicKeyToken { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string Hash { get; set; }

        public List<ExternalType> ExternalTypes { get; set; } = new List<ExternalType>();

    }

    public class ExternalType : Domain.Type
    {

        public string Namespace { get; set; }
        public string FullName { get; set; }
        public string AssemblySource { get; set; }

        public Dictionary<string, Field> Fields = new Dictionary<string, Field>();
        public Dictionary<string, List<Method>> Methods = new Dictionary<string, List<Method>>();

    }

    public class ExternalPlaceholder : Domain.Type { }

    public class TypeService
    {

        private readonly Dictionary<string, List<ExternalType>> SeedTypes = new Dictionary<string, List<ExternalType>>();

        public TypeService()
        {

            SeedTypes.Add("int", new List<ExternalType> { new ExternalType { Name = "int", AssemblySource = "Bootstraped", Namespace = "Shift" } });
            SeedTypes.Add("string", new List<ExternalType> { new ExternalType { Name = "string", AssemblySource = "Bootstraped", Namespace = "Shift" } });
            SeedTypes.Add("bool", new List<ExternalType> { new ExternalType { Name = "bool", AssemblySource = "Bootstraped", Namespace = "Shift" } });

            SeedTypes.Add("object", new List<ExternalType> { new ExternalType { Name = "object", AssemblySource = "Bootstraped", Namespace = "Shift" } });
            SeedTypes.Add("void", new List<ExternalType> { new ExternalType { Name = "void", AssemblySource = "Bootstraped", Namespace = "Shift" } });

        }

        public Application LinkExternalTypes(Application app)
        {

            var externalTypes = LoadExternalTypes();

            var unknownTypes = app
                .Types
                .Where(x => x.Value.Source is null)
                .Where(x => x.Key != "var")//We'll infer var later
                ;

            foreach(var unknownType in unknownTypes)
            {

                if(externalTypes.ContainsKey(unknownType.Key))
                {

                    var found = externalTypes[unknownType.Key];

                    if (found.Count() > 1)
                    {

                        var error = found
                            .Select(x => x.FullName)
                            .Aggregate((x, y) => $"{x} and {y}")
                            ;

                        throw new Exception($"Ambigiuous reference resolving [{unknownType.Key}] amongst {error}");
                    }

                    var matchedType = found.Single();

                    unknownType.Value.Source = new TypeSource { From = $"External Assembly" };
                    unknownType.Value.BackingType = matchedType;

                }
                else
                {
                    throw new Exception("Uknown type internally or externally");
                }

            }

            return app;

        }

        private Dictionary<string, List<ExternalType>> LoadExternalTypes()
        {

            return SeedTypes;

            var cachedAssemblies = LoadFromCache();

            var assemblies = GetSDKAssemblies();

            var paths = assemblies
                .Select(x => $"C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/5.0.0/{x.Path}")
                ;

            var resolver = new PathAssemblyResolver(paths);
            using var mlc = new MetadataLoadContext(resolver);

            foreach (var assembly in assemblies)
            {

                //See if it's already cached
                if (cachedAssemblies.ContainsKey(assembly.AssemblyName) && cachedAssemblies[assembly.AssemblyName].Hash == assembly.Hash)
                {
                    assembly.ExternalTypes = cachedAssemblies[assembly.AssemblyName].ExternalTypes;
                }
                else
                {
                    var loadedAssembly = mlc.LoadFromAssemblyPath($"C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/5.0.0/{assembly.Path}");

                    assembly.ExternalTypes = loadedAssembly
                        .ExportedTypes
                        .Select(x => BuildExternalType(x, assembly))
                        .ToList()
                        ;
                }

            }

            SaveToCache(assemblies);

            return assemblies
                .SelectMany(x => x.ExternalTypes)
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

        }

        public ExternalType BuildExternalType(System.Type type, FrameworkAssembly assembly)
        {

            var externalType = new ExternalType
            {
                Name = type.Name,
                Namespace = type.Namespace,
                FullName = type.FullName,
                AssemblySource = assembly.AssemblyName
            };

            var fields = type
                .GetFields()
                .ToList()
                ;

            var types = type
                .GetProperties()
                .Where(x => !x.GetIndexParameters().Any())//For now don't support indexers
                .ToList()
                ;

            foreach (var field in fields)
            {

                var externalField = new Field 
                { 
                    Identifier = field.Name, 
                    Type = new TypeMeta
                    {
                        BackingType = new ExternalPlaceholder { Name = field.FieldType.FullName },
                    }
                };

                externalType.Fields.Add(field.Name, externalField);
            }

            foreach (var property in types)
            {

                var externalField = new Field
                {
                    Identifier = property.Name,
                    Type = new TypeMeta
                    {
                        BackingType = new ExternalPlaceholder { Name = property.PropertyType.FullName },
                    }
                };

                externalType.Fields.Add(property.Name, externalField);
            }

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(m => !m.IsSpecialName);

            externalType.Methods = methods
                .Select(x => BuildExternalMethod(x))
                .GroupBy(x => x.Signature.Identifier)
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

            //Constructors haven't been implemented yet so unnecessary
            //var constructors = type.GetConstructors();

            return externalType;

        }

        private static Method BuildExternalMethod(MethodInfo method)
        {
            var externalMethod = new Method
            {
                Signature = new Signature
                {
                    Identifier = method.Name,
                    Type = new TypeMeta
                    {
                        BackingType = new ExternalPlaceholder { Name = method.ReturnType.FullName },
                    },
                }
            };

            externalMethod.Signature.Parameters = method
                .GetParameters()
                .Select(x => new Parameter
                {
                    Identifier = x.Name,
                    Type = new TypeMeta
                    {
                        BackingType = new ExternalPlaceholder { Name = x.ParameterType.FullName },
                    }
                })
                .ToList();
            return externalMethod;
        }

        private Dictionary<string, FrameworkAssembly> LoadFromCache()
        {
            if (!Directory.Exists("./assemblyMetaCache"))
            {
                Directory.CreateDirectory("./assemblyMetaCache");
            }

            Dictionary<string, FrameworkAssembly> cachedAssemblies;

            if (File.Exists("./assemblyMetaCache/assemblyInfo.json"))
            {
                var json = File.ReadAllText("./assemblyMetaCache/assemblyInfo.json");
                cachedAssemblies = JsonConvert.DeserializeObject<Dictionary<string, FrameworkAssembly>>(json);
            }
            else
            {
                cachedAssemblies = new Dictionary<string, FrameworkAssembly>();
            }

            return cachedAssemblies;
        }

        private void SaveToCache(IEnumerable<FrameworkAssembly> assemblies)
        {

            var cacheFormat = assemblies
                .ToDictionary(x => x.AssemblyName)
                ;

            File.WriteAllText("./assemblyMetaCache/assemblyInfo.json", JsonConvert.SerializeObject(cacheFormat, Formatting.Indented));

        }

        private IEnumerable<FrameworkAssembly> GetSDKAssemblies()
        {
            var frameworkList = File.ReadAllText(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\data\FrameworkList.xml");
            var doc = XDocument.Parse(frameworkList);

            var nodes = doc.Descendants("File");

            var assemblies = nodes
                .Select(x =>
                {
                    var assembly = new FrameworkAssembly
                    {
                        Path = x.Attribute("Path").Value,
                        AssemblyName = x.Attribute("AssemblyName").Value,
                        PublicKeyToken = x.Attribute("PublicKeyToken").Value,
                        AssemblyVersion = x.Attribute("AssemblyVersion").Value,
                        FileVersion = x.Attribute("FileVersion").Value
                    };

                    assembly.Hash = GetHash($"{assembly.Path}{assembly.AssemblyName}{assembly.PublicKeyToken}{assembly.AssemblyVersion}{assembly.FileVersion}");

                    return assembly;
                })
                .ToList()
                ;

            return assemblies;
        }

        private static readonly SHA256 sha256 = SHA256.Create();

        private static string GetHash(string text)
        {

            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

    }
}
