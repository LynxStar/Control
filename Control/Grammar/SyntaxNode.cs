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

        public override string ToString()
        {
            return $"{Rule.Name}:{Capture} - Nodes: {SyntaxNodes.Count()}";
        }

        //Convenience Methods

        public List<SyntaxNode> CGR => SyntaxNodes
            .Single(x => x.Rule.Name.Contains("CGR"))
            .SyntaxNodes
            ;

        public SyntaxNode Aspect => this["aspect"];
        public SyntaxNode Identifier => this["identifier"];
        public SyntaxNode Field => this["field"];
        public SyntaxNode TypeDef => this["typeDef"];
        public SyntaxNode Type => this["type"];
        public SyntaxNode Method => this["method"];
        public SyntaxNode Signature => this["signature"];
        public SyntaxNode Block => this["block"];
        public SyntaxNode Parameter => this["parameter"];
        public SyntaxNode Statement => this["statement"];


        public SyntaxNode Expression => this["expression"];
        public SyntaxNode ReturnExpression => this["return_expression"];
        public SyntaxNode UnaryExpression => this["unary_expression"];
        public SyntaxNode PrimaryExpression => this["primary_expression"];

        public SyntaxNode Literal => this["literal"];

        public string TokenValue => Capture is not null
            ? Capture
            : SyntaxNodes.Single().Capture
            ;

        public SyntaxNode this[string ruleName] => SyntaxNodes.Single(x => x.Rule.Name == ruleName);

    }

}
