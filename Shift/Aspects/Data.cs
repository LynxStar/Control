using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Aspects
{
    
    public class Data : ShiftType
    {
        public List<Field> Fields { get; set; } = new List<Field>();

    }
}
