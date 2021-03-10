using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Aspects
{

    public class Application
    {

        public Dictionary<string, ShiftType> UsedTypes = new Dictionary<string, ShiftType>();

        public ShiftType this[string name] => Retrieve(name);

        public ShiftType Retrieve(string id)
        {

            if (UsedTypes.ContainsKey(id))
            {
                return UsedTypes[id];
            }

            var newType = new ShiftType
            {
                Identifier = id
            };

            UsedTypes.Add(id, newType);

            return newType;
        }

    }

}
