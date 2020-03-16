using System;
using System.Collections.Generic;
using System.Text;

namespace Control.Grammar
{
    public class CodeFragment
    {

        GrammarRule Rule { get; set; }
        public string Fragment { get; set; }

        public CodeFragment Previous { get; set; }
        public CodeFragment Next { get; set; }

    }
}
