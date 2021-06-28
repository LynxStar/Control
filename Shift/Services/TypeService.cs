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
    
    public class AssemblySchema
    {
        public string Path { get; set; }
        public string AssemblyName { get; set; }
        public string PublicKeyToken { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string Hash { get; set; }

        public List<ExportedType> ExportedTypes { get; set; } = new List<ExportedType>();

    }

    public class ExportedType : Domain.Type
    {
        public string FullName { get; set; }
        public string AssemblySource { get; set; }

    }

    public class SeedType : Domain.Type
    {
        public string CSharpNamespace { get; set; }
        public MainExpression Initializer { get; set; }
    }

    public class TypeService
    {

        private readonly Dictionary<string, SeedType> SeedTypes = new Dictionary<string, SeedType>();

        private readonly Dictionary<string, AssemblySchema> externalAssemblies = new Dictionary<string, AssemblySchema>();
        private readonly Dictionary<string, List<ExportedType>> exportedTypes = new Dictionary<string, List<ExportedType>>();

        public TypeService()
        {

            SeedTypes.Add("void", new SeedType { Name = "void", Namespace = "Shift" });
            SeedTypes.Add("string", new SeedType 
            {
                Name = "string", 
                Namespace = "Shift", 
                CSharpNamespace = "System",
                Initializer = new MainExpression
                {
                    ExpressionStart = new Identifier { Path = "String" },
                    ExpressionChain = new List<ExpressionChain> 
                    {
                        new Identifier { Path = "Empty" }
                    }
                }
            });

            var assemblies = LoadExternalTypes();

            externalAssemblies = assemblies
                .ToDictionary(x => x.AssemblyName)
                ;

            exportedTypes = assemblies
                .SelectMany(x => x.ExportedTypes)
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

        }

        public Domain.Type RetrieveExternalType(TrackedType trackedType)
        {

            var key = trackedType.Name;

            //Replace with type aliases down the road

            key = key switch
            {
                "int" => "Int32",
                "bool" => "Boolean",
                _ => key
            };

            if (SeedTypes.ContainsKey(key))
            {
                return SeedTypes[key];
            }
            else if (exportedTypes.ContainsKey(key))
            {

                var found = exportedTypes[key];

                if (found.Count() > 1)
                {

                    var error = found
                        .Select(x => x.FullName)
                        .Aggregate((x, y) => $"{x} and {y}")
                        ;

                    throw new Exception($"Ambigiuous reference resolving [{key}] amongst {error}");
                }

                var matchedType = found.Single();

                return matchedType;

            }
            else
            {
                throw new Exception("Uknown type internally or externally");
            }

        }

        private IEnumerable<AssemblySchema> LoadExternalTypes()
        {

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
                    assembly.ExportedTypes = cachedAssemblies[assembly.AssemblyName].ExportedTypes;
                }
                else
                {
                    var loadedAssembly = mlc.LoadFromAssemblyPath($"C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/5.0.0/{assembly.Path}");

                    assembly.ExportedTypes = loadedAssembly
                        .ExportedTypes
                        .Select(x => BuildExportedType(x, assembly))
                        .ToList()
                        ;
                }

            }

            SaveToCache(assemblies);

            return assemblies;

        }

        public ExportedType BuildExportedType(System.Type type, AssemblySchema assembly)
        {

            var externalType = new ExportedType
            {
                Name = type.Name,
                Namespace = type.Namespace,
                FullName = type.FullName,
                AssemblySource = assembly.AssemblyName
            };

            return externalType;

        }

        private Dictionary<string, AssemblySchema> LoadFromCache()
        {
            if (!Directory.Exists("./assemblyMetaCache"))
            {
                Directory.CreateDirectory("./assemblyMetaCache");
            }

            Dictionary<string, AssemblySchema> cachedAssemblies;

            if (File.Exists("./assemblyMetaCache/assemblyInfo.json"))
            {
                var json = File.ReadAllText("./assemblyMetaCache/assemblyInfo.json");
                cachedAssemblies = JsonConvert.DeserializeObject<Dictionary<string, AssemblySchema>>(json);
            }
            else
            {
                cachedAssemblies = new Dictionary<string, AssemblySchema>();
            }

            return cachedAssemblies;
        }

        private void SaveToCache(IEnumerable<AssemblySchema> assemblies)
        {

            var cacheFormat = assemblies
                .ToDictionary(x => x.AssemblyName)
                ;

            File.WriteAllText("./assemblyMetaCache/assemblyInfo.json", JsonConvert.SerializeObject(cacheFormat, Formatting.Indented));

        }

        private IEnumerable<AssemblySchema> GetSDKAssemblies()
        {
            var frameworkList = File.ReadAllText(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\data\FrameworkList.xml");
            var doc = XDocument.Parse(frameworkList);

            var nodes = doc.Descendants("File");

            var assemblies = nodes
                .Select(x =>
                {
                    var assembly = new AssemblySchema
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
