using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Aspects.Expressions
{
    
    public interface PrimaryExpression { }

    public interface ChainPart { }

    public class ChainIdentifier : ChainPart
    {
        public string Identifier { get; set; }
    }

    public class Chain : PrimaryExpression
    {
        public List<ChainPart> ChainParts { get; set; } = new List<ChainPart>();
    }

    public class UnaryExpression : ShiftExpression
    {
        public PrimaryExpression PrimaryExpression { get; set; }
    }

    public abstract class ShiftExpression : PrimaryExpression { }
}
