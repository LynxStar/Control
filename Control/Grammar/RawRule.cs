using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Grammar
{

    public enum RuleType
    {
        Form,
        Token,
        Fragment
    }

    public class Rule
    {
        public RuleType RuleType { get; set; }
        public string Name { get; set; }
        public List<RuleOption> Options { get; set; } = new List<RuleOption>();
    }

    public class RuleOption
    {

        public List<Clause> Clauses { get; set; } = new List<Clause>();

    }

    public enum ClauseType
    {
        Reference,
        Literal,
        Regex,
        CaptureGroup
    }

    public class Clause
    {

        public string Value { get; set; }
        public Rule Reference { get; set; }
        public CaptureGroup CaptureGroup { get; set; }
        public ClauseType ClauseType { get; set; }

    }

    public enum CaptureModifier
    {
        None,
        Optional,
        OneOrMore,
        NoneToOne,
    }

    public class CaptureGroup : RuleOption 
    { 
        public CaptureModifier Modifier { get; set; }
    }
    
}
