using System;
using System.Collections.Generic;
using System.Text;

namespace Control.Grammar
{
    public class RuleClause
    {
        //parameter*&COMMA
        public string Clause { get; set; }
        public bool IsLiteral { get; set; }
        public ClauseQualifier Qualifier { get; set; }
        public bool DelimiterInUse { get; set; }
        public string QualifierArgument { get; set; }
        public ClauseQualifier RightQualifier { get; set; }

        public override string ToString()
        {

            var left = $"{Clause}";

            var leftQualifier = Qualifier switch
            { 
                ClauseQualifier.None => String.Empty,
                ClauseQualifier.NoneToOne => "?",
                ClauseQualifier.OneOrMore => "+",
                ClauseQualifier.Optional => "*"
            };

            var right = "";
            var rightQualifier = "";

            if(DelimiterInUse)
            {
                right = $"&{QualifierArgument}";
                rightQualifier = RightQualifier switch
                {
                    ClauseQualifier.None => String.Empty,
                    ClauseQualifier.NoneToOne => "?",
                    ClauseQualifier.OneOrMore => "+",
                    ClauseQualifier.Optional => "*"
                };

            }

            return $"{left}{leftQualifier}{right}{rightQualifier}";
        }

    }

    public enum ClauseQualifier
    {
        None,
        Optional,
        OneOrMore,
        NoneToOne,
    }

}
