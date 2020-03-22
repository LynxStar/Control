using System;
using System.Collections.Generic;
using System.Text;

namespace Control.Grammar
{
    
    public class Token
    {

        public string Name { get; set; }
        public string Capture { get; set; }
        public bool IsRaw { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
