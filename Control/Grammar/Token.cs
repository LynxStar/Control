using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Grammar
{
    
    public class Token
    {
        public Rule Rule { get; set; }
        public string Capture { get; set; }

        public override string ToString()
        {
            return $"{Rule} - {Capture}";
        }

    }
}
