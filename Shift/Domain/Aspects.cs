using Shift.Intermediate;
using Shift.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Domain
{


    //Is this necessary?
    public class Application : Namespace
    {

        public void AddType(Type type)
        {
            Types.Add(type.Name, type);
        }

    }

    public class Namespace
    {

        public string Name { get; set; }
        public string Fullname => Parent is null
            ? Name
            : $"{Parent.Fullname}.{Name}"
            ;

        public Namespace Parent { get; set; }

        public Dictionary<string, Namespace> Namespaces = new Dictionary<string, Namespace>();
        public Dictionary<string, Type> Types = new Dictionary<string, Type>();


    }

    public enum TypeSource
    {
        Unknown,
        User,
        External,
        Seeded
    }

    public class Type
    {

        public string Name { get; set; }
        public string Namespace { get; set; }
        public TypeSource Source { get; set; } = TypeSource.Unknown;

        public override string ToString()
        {
            return $"{Namespace}.{Name} from: {Source}";
        }

    }

    public interface HasFields
    {
        Dictionary<string, Field> Fields { get; set; }
        IEnumerable<OperatorMethod> OperatorMethods { get; set; }
    }

    public interface HasMethods
    {
        Dictionary<string, List<Method>> Methods { get; set; }
    }

    public interface HasConstructors
    {
        IEnumerable<Method> Constructors { get; set; }
    }

    public class Data : Type, HasFields
    {
        public Dictionary<string, Field> Fields { get; set; } = new Dictionary<string, Field>();
        public IEnumerable<OperatorMethod> OperatorMethods { get; set; } = new List<OperatorMethod>();

    }

    public class Field : TypeDefinition { }

    public class TypeDefinition
    {
        public TrackedType Type { get; set; }
        public string Identifier { get; set; }
    }

    public class Library : Type, HasMethods
    {
        public Dictionary<string, List<Method>> Methods { get; set; } = new Dictionary<string, List<Method>>();
    }

    public class Service : Type, HasFields, HasMethods, HasConstructors
    {
        public Dictionary<string, Field> Fields { get; set; } = new Dictionary<string, Field>();
        public Dictionary<string, List<Method>> Methods { get; set; } = new Dictionary<string, List<Method>>();
        public IEnumerable<Method> Constructors { get; set; } = new List<Method>();
        public IEnumerable<OperatorMethod> OperatorMethods { get; set; } = new List<OperatorMethod>();
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

    public class Signature : TypeDefinition
    {
        public List<Parameter> Parameters = new List<Parameter>();
    }

    public class Parameter : TypeDefinition { }

    public class Block
    {
        public List<Statement> Statements { get; set; } = new List<Statement>();
    }

    public interface Statement { }

    public class ControlStatement : Statement
    {
        public ConditionalOrExpression Condition { get; set; }
        public Block Block { get; set; }
    }

    public class IfControl : ControlStatement { }
    public class WhileControl : ControlStatement { }

    public class Declaration : Statement
    {
        public TypeDefinition TypeDefinition { get; set; }
        public Expression InitializerExpression { get; set; }
    }

    public class Assignment : Statement
    {
        public List<Identifier> IdentifierChain = new List<Identifier>();
        public Expression Expression { get; set; }
    }

    public class ReturnExpression : Statement
    {
        public Expression Expression { get; set; }
    }

    public class Expression : ExpressionBase, Statement
    {
    }

    public class UnaryExpression : ExpressionBase
    {
        public MainExpression MainExpression { get; set; }
    }

    public class MainExpression : ExpressionBase
    {
        public ExpressionStart ExpressionStart { get; set; }
        public List<ExpressionChain> ExpressionChain { get; set; } = new List<ExpressionChain>();
    }

    public interface ExpressionStart { }

    public class Literal : ExpressionStart { }

    public class Literal<T> : Literal
    {
        public T Value { get; set; }
    }

    public class Identifier : ExpressionStart, ExpressionChain
    {
        public string Path { get; set; }
    }

    public class ParensExpression : ExpressionStart
    {
        public Expression Expression { get; set; }
    }

    public class NewExpression : ExpressionStart
    {
        public Identifier Identifier { get; set; }
        public Invocation Invocation { get; set; }
    }

    public class Invocation : ExpressionStart, ExpressionChain
    {
        public List<Expression> Arguments { get; set; } = new List<Expression>();
    }

    public interface ExpressionChain { }

}
