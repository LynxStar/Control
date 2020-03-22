﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Control.Grammar
{
    
    public class TokenRegex
    {

        public string Name { get; set; }
        public Regex Regex { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Regex}";
        }


    }
}
