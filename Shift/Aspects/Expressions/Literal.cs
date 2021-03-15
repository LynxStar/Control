using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Aspects.Expressions
{
    public class Literal<T> : PrimaryExpression, ChainPart
    {
        public ShiftType ShiftType { get; set; }
        public T Value { get; set; }
    }
}
