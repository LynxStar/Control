using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Grammar
{
    public class Clause
    {

        public string Value { get; set; }
        public Rule Reference { get; set; }
        public CaptureGroup CaptureGroup { get; set; }
        public ClauseType ClauseType { get; set; }

    }
}
