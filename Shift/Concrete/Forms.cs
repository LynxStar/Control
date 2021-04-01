using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Concrete
{
    public class Source
    {
    }

    public class Aspect
    {

    }

    public class Data
    {
        public Identifier Identifier { get; set; }
        public List<Field> Fields { get; set; }
    }

    public class Identifier
    {
        public string IDENTIFIER { get; set; }
    }

    public class Field
    {
        public TypeDef TypeDef { get; set; }
    }

    public class TypeDef
    {
        public Type Type { get; set; }
        public Identifier Identifier { get; set; }
    }

    public class Type
    {         
        public string IDENTIFIER { get; set; }
    }


}
