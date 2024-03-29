﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Grammar
{

    public class SyntaxNode
    {

        public Rule Rule { get; set; }
        public RuleOption SelectedOption { get; set; }
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

        public List<SyntaxNode> OneOrMany(Func<SyntaxNode, SyntaxNode> selector)
        {

            var repeats = new List<SyntaxNode>();

            var first = selector(this);

            var theRest = CGR.Select(selector);

            repeats.Add(first);
            repeats.AddRange(theRest);

            return repeats
                .ToList()
                ;
        }

        public List<SyntaxNode> InvocationArgExpressions
        {
            get
            {

                //incoming invocation
                //Grab CGR for arguements
                //Grab inner
                //Expand to many
                //Project to expression

                return CGR
                    .First()
                    .OneOrMany(x => x.Argument)
                    .Select(x => x.Expression)
                    .ToList()
                    ;
            }
        }

        public List<SyntaxNode> Chain
        {
            get
            {

                return CGR
                    .First()
                    .SyntaxNodes
                    .ToList()
                    ;
            }
        }

        public SyntaxNode Arguments => CGR.First();
        public SyntaxNode Argument => this["argument"];

        public SyntaxNode Declaration => this["declaration"];
        public SyntaxNode Initializer => this["initializer"];


        public SyntaxNode Assignment => this["assignment"];
        public List<string> Accessor
        {
            get
            {

                return this["accessor"]
                    .OneOrMany(x => x.Identifier)
                    .Select(x => x.TokenValue)
                    .ToList()
                    ;
            }
        }


        public SyntaxNode ExpressionStatement => this["expression_statement"];
        public SyntaxNode Expression => this["expression"];
        public SyntaxNode ReturnExpression => this["return_expression"];
        public SyntaxNode UnaryExpression => this["unary_expression"];
        public SyntaxNode MainExpression => this["main_expression"];
        public SyntaxNode ExpressionStart => this["expression_start"];
        public SyntaxNode NewExpression => this["new_expression"];
        public SyntaxNode ParenthesesExpression => this["parens_expression"].CGR.First();
        public SyntaxNode ExpressionChain => this["expression_chain"];
        public SyntaxNode MemberAccess => this["member_access"];

        public SyntaxNode conditional_or_expression => this["conditional_or_expression"];
        public SyntaxNode conditional_and_expression => this["conditional_and_expression"];
        public SyntaxNode equality_expression => this["equality_expression"];
        public SyntaxNode relational_expression => this["relational_expression"];
        public SyntaxNode additive_expression => this["additive_expression"];
        public SyntaxNode multiplicative_expression => this["multiplicative_expression"];


        public SyntaxNode Invocation => this["invocation"];

        public SyntaxNode ExpToUnary => Expression
            .conditional_or_expression
            .conditional_and_expression
            .equality_expression
            .relational_expression
            .additive_expression
            .multiplicative_expression
            .UnaryExpression
            ;

        public SyntaxNode ExpToStart => ExpToUnary
            .MainExpression
            .ExpressionStart
            ;

        public SyntaxNode ConditionalOrExprToStart => conditional_or_expression
            .conditional_and_expression
            .equality_expression
            .relational_expression
            .additive_expression
            .multiplicative_expression
            .UnaryExpression
            .MainExpression
            .ExpressionStart
            ;

        public SyntaxNode BasicLiteral => ExpToStart
                .Literal
                ;

        public SyntaxNode Literal => this["literal"];

        public string TokenValue => Capture is not null
            ? Capture
            : SyntaxNodes.Single().Capture
            ;

        public SyntaxNode this[string ruleName] => SyntaxNodes.Single(x => x.Rule.Name == ruleName);

    }

}
