using Shift.Domain;
using Shift.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Services
{

    public class TypeTracker
    {

        public Dictionary<string, TrackedType> TrackedTypes = new Dictionary<string, TrackedType>();

        private readonly Application _application;
        private readonly TypeService _typeService;

        public TypeTracker(Application application, TypeService typeService)
        {
            _application = application;
            _typeService = typeService;
        }

        public TrackedType this[string name]
        {
            get
            {
                var type = TrackedTypes.GetValueOrDefault(name);

                if (type is null)
                {
                    type = new TrackedType
                    {
                        Name = name,
                        Resolver = Resolve
                    };
                    TrackedTypes.Add(name, type);
                }

                return type;

            }
        }

        public Domain.Type Resolve(string name)
        {

            if (_application.Types.ContainsKey(name))
            {
                return _application.Types[name];
            }

            name = name switch
            {
                "int" => "Int32",
                "bool" => "Boolean",
                _ => name
            };

            if (_typeService.ResolveSeedType(name) is SeedType seedType)
            {
                return seedType;
            }

            return _typeService.ResolveExternalType(name, this);

        }

        public T MapTypeDef<T>(Concrete.TypeDef typeDef) where T : TypeDefinition, new()
        {

            T target = new T();

            target.Type = this[typeDef.Type.IDENTIFIER];
            target.Identifier = typeDef.Identifier;

            return target;

        }
    }
}
