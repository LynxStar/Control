using Control;
using Control.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Concrete
{
    public class Source
    {
        public List<Aspect> Aspects { get; set; }
    }

    public class Aspect
    {

    }

    public class Data
    {
        public Identifier Identifier { get; set; }
        public List<Field> Fields { get; set; }
    }

    public class Identifier
    {
        public string IDENTIFIER { get; set; }
    }

    public class Field
    {
        public TypeDef TypeDef { get; set; }
    }

    public class TypeDef
    {
        public Type Type { get; set; }
        public Identifier Identifier { get; set; }
    }

    public class Type
    {         
        public string IDENTIFIER { get; set; }
    }

    public class Library
    {
        public Identifier Identifier { get; set; }
        public List<Method> Methods { get; set; }
    }

    public class Method
    {
        public Signature Signature { get; set; }
        public Block Block { get; set; }
    }

    public class Signature
    {
        public TypeDef TypeDef { get; set; }
        [Direct]
        public Parameters Parameters { get; set; }

    }

    public class Parameters
    {
        public Parameter Parameter { get; set; }
        [Direct(1)]
        public List<Parameter> AdditionalParameters { get; set; }
    }

    public class Parameter
    {
        public TypeDef TypeDef { get; set; }
    }

    public class Block
    {

        [Direct]
        public List<Statement> Statements { get; set; }

    }

    [Options(typeof(Declaration), typeof(Assignment), typeof(ReturnExpression), typeof(Expression))]
    public class Statement { }

    public class Declaration : Statement
    {

    }

    public class Assignment : Statement
    {

    }

    [Form("return_expression")]
    public class ReturnExpression : Statement
    {

    }

    public class Expression : Statement
    {

    }

}
