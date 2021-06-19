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

    public class Aspect : Option<Data, Library, Service> { }

    public interface Identifiable
    {
        public Identifier Identifier { get; set; }
    }

    public class Data : Identifiable
    {
        public Identifier Identifier { get; set; }
        public List<Field> Fields { get; set; }
    }

    public class Identifier
    {
        public string IDENTIFIER { get; set; }

        public static implicit operator string(Identifier identifier) => identifier.IDENTIFIER;

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

    public class Library : Identifiable
    {
        public Identifier Identifier { get; set; }
        public List<Method> Methods { get; set; }
    }

    public class Service : Identifiable
    {
        public Identifier Identifier { get; set; }
        [Direct]
        public List<ServiceMember> ServiceMembers { get; set; }
    }

    public class ServiceMember: Option<OperatorMethod, Method, Field, Constructor> { }

    public class Constructor
    {
        [Direct]
        public Parameters Parameters { get; set; }
        public Block Block { get; set; }
    }

    public class OperatorMethod
    {
        public Operator Operator { get; set; }
        public Method Method { get; set; }
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

    public class Statement : Option<ControlStatement, Declaration, Assignment, ReturnExpression, ExpressionStatement> { }

    public class ControlStatement : Option<IfControl, WhileControl> { }

    public class IfControl
    {
        public ConditionalOrExpression Condition { get; set; }
        public Block Block { get; set; }
    }

    public class WhileControl
    {
        public ConditionalOrExpression Condition { get; set; }
        public Block Block { get; set; }
    }

    public class Declaration
    {
        public TypeDef TypeDef { get; set; }
        [Direct]
        public Initializer Initializer { get; set; }
    }

    public class Initializer
    {
        public Expression Expression { get; set; }
    }

    public class Assignment
    {
        public Accessor Accessor  { get; set; }
        public Expression Expression { get; set; }
    }

    public class Accessor
    {
        public Identifier Identifier { get; set; }
        [Direct(1)]
        public List<Identifier> DotAccessors { get; set; }

    }

    public class ReturnExpression
    {
        public Expression Expression { get; set; }
    }

    public class ExpressionStatement
    {
        public Expression Expression { get; set; }
    }

    public class Expression : Option<ConditionalOrExpression> { }


    public class UnaryExpression
    {
        public MainExpression MainExpression { get; set; }
    }

    public class MainExpression
    {
        public ExpressionStart ExpressionStart { get; set; }
        public List<ExpressionChain> ExpressionChains { get; set; }
    }

    public class ExpressionStart : Option<Literal, Identifier, ParensExpression, NewExpression, Invocation> { }

    public class Literal : Option<Boolean, String, TokenValue> { }

    public class Boolean : TokenValue { }
    public class String
    {
        public string STRING { get; set; }
    }

    public class ParensExpression
    {
        [Direct]
        public Expression Expression { get; set; }
    }

    public class NewExpression
    {
        public Identifier Identifier { get; set; }
        public Invocation Invocation { get; set; }
    }

    public class Invocation
    {
        [Direct]
        public Arguments Arguments { get; set; }

    }

    public class Arguments
    {
        public Argument Argument { get; set; }
        [Direct(1)]
        public List<Argument> AdditionalArguments { get; set; }
    }

    public class Argument
    {
        public Expression Expression { get; set; }
    }

    public class ExpressionChain : Option<MemberAccess, Invocation> { }

    public class MemberAccess
    {
        public Identifier Identifier { get; set; }
    }

}
