using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Domain
{

    public class TypeTracker
    {

        public Dictionary<string, TrackedType> ConsumedTypes = new Dictionary<string, TrackedType>();


        public TrackedType this[string name]
        {
            get
            {
                var type = ConsumedTypes.GetValueOrDefault(name);

                if (type is null)
                {
                    type = new TrackedType { Name = name };
                    ConsumedTypes.Add(name, type);
                }

                return type;


            }
        }

        public TrackedType this[Type type]
        {
            get
            {
                //Blow up for now
                return ConsumedTypes[type.Name];
            }
        }

        public T MapTypeDef<T>(Concrete.TypeDef typeDef) where T : TypeDefinition, new()
        {

            T target = new T();

            target.Type = this[typeDef.Type.IDENTIFIER];
            target.Identifier = typeDef.Identifier;

            return target;

        }
    }

    //Is this necessary?
    public class Application : Namespace
    {

        public TypeTracker Tracker { get; set; } = new TypeTracker();


        public void AddType(Type type)
        {

            var tracked = Tracker[type];
            tracked.BackingType = type;

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

    public class TrackedType
    {
        public string Name { get; set; }
        public Type BackingType { get; set; }
        public TypeSource Source { get; set; }

        public override string ToString()
        {
            return $"{Name} from: {Source}";
        }

    }

    public class Type
    {
        public string Name { get; set; }
        public string Namespace { get; set; }

        public override string ToString()
        {
            return $"{Namespace}.{Name}";
        }

    }

    public class Data : Type
    {
        public Dictionary<string, Field> Fields = new Dictionary<string, Field>();
    }

    public class Field : TypeDefinition { }

    public class TypeDefinition
    {
        public TrackedType Type { get; set; }
        public string Identifier { get; set; }
    }

    public class Library : Type
    {
        public Dictionary<string, List<Method>> Methods = new Dictionary<string, List<Method>>();
    }

    public class Service : Type
    {
        public Dictionary<string, Field> Fields = new Dictionary<string, Field>();
        public Dictionary<string, List<Method>> Methods = new Dictionary<string, List<Method>>();
        public Dictionary<string, List<Method>> Constructors = new Dictionary<string, List<Method>>();
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
