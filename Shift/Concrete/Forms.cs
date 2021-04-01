using Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Concrete
{
    public class Source
    {
        [InnerCGR]
        public List<Aspect> Aspects { get; set; }
    }

    public class Aspect
    {

    }

    public class Data
    {
        public Identifier Identifier { get; set; }
        [InnerCGR]
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

    public class Signature
    {
        public TypeDef TypeDef { get; set; }
        [InnerCGR]
        public Parameters Parameters { get; set; }

    }

    public class Parameters
    {
        public Parameter Parameter { get; set; }
        [InnerCGR(1)]
        public List<Parameter> AdditionalParameters { get; set; }
    }

    public class Parameter
    {
        public TypeDef TypeDef { get; set; }
    }



}
