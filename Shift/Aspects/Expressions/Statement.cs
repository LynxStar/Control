using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Aspects.Expressions
{

    public class Invocation : ChainPart
    {
        public List<ShiftExpression> Arguments { get; set; } = new List<ShiftExpression>();
    }

    public class Call : Statement
    {

        public ShiftExpression Expression { get; set; }
        public Invocation Invocation { get; set; }

    }

    public class ReturnExpression : Statement
    {
        public ShiftExpression Expression { get; set; }
    }

    public class Assignment : Statement
    {
        public string Accessor { get; set; }
        public ShiftExpression Expression { get; set; }
    }

    public class Declaration : Statement
    {
        public ShiftType Type { get; set; }
        public string Identifier { get; set; }
        public ShiftExpression Expression { get; set; }
    }

    public class Statement { }
}
