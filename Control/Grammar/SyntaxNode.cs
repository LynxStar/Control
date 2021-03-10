using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Grammar
{

    public class SyntaxNode
    {

        public Rule Rule { get; set; }
        public string Capture { get; set; }
        public List<SyntaxNode> SyntaxNodes { get; set; } = new List<SyntaxNode>();


        //Convenience Methods

        public List<SyntaxNode> CGR => SyntaxNodes
            .Single(x => x.Rule.Name.Contains("CGR"))
            .SyntaxNodes
            ;

        public SyntaxNode Identifier => this["identifier"];
        public SyntaxNode Field => this["field"];
        public SyntaxNode TypeDef => this["typeDef"];
        public SyntaxNode Type => this["type"];

        public string TokenValue => Capture is not null
            ? Capture
            : SyntaxNodes.Single().Capture
            ;

        public SyntaxNode this[string ruleName] => SyntaxNodes.Single(x => x.Rule.Name == ruleName);

    }

}
