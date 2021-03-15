using Shift.Aspects.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Aspects
{

    public class Parameter
    {
        public ShiftType Type { get; set; }
        public string Identifier { get; set; }
    }

    public class Signature
    {
        public ShiftType Type { get; set; }
        public string Identifier { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public class Method
    {

        public Signature Signature { get; set; }
        public List<Statement> Statements { get; set; } = new List<Statement>();

    }

}
