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

        public override string ToString()
        {

            var format = CaptureGroup is not null
                ? "({0})" + CaptureGroup.Modifier
                : "{0}"
                ;

            var value = CaptureGroup is not null
                ? CaptureGroup.ToString()
                : Value
                ;

            return String.Format(format, value);
        }

    }
}
