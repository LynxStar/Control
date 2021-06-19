using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Intermediate
{

    public class TrackedType
    {

        public string Name { get; set; }

        public bool Resolved { get { return _backedType is not null; } }

        public Func<string, Domain.Type> Resolver { get; set; }

        private Domain.Type _backedType;

        public Domain.Type BackingType
        {
            get
            {
                if(_backedType is null)
                {
                    _backedType = Resolver(Name);
                }

                return _backedType;

            }
        }

        public override string ToString()
        {

            var state = Resolved
                ? $"{BackingType}"
                : "{{UNREALIZED}}"
                ;

            return $"{Name} - {state}";
        }

    }
}
