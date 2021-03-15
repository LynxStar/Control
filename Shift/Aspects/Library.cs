using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Aspects
{
    
    public class Library : ShiftType
    {
        public List<Method> Methods { get; set; } = new List<Method>();
    }
}
