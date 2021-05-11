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
        public Dictionary<string, TypeMeta> Types = new Dictionary<string, TypeMeta>();

        public TypeMeta this[string name]
        {
            get
            {
                var type = Types.GetValueOrDefault(name);

                if (type is null)
                {
                    type = new TypeMeta { };
                    Types.Add(name, type);
                }

                return type;


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

    public class TypeSource
    {
        public string From { get; set; }
        public static TypeSource UserDefined => new TypeSource { From = "User Defined" };

    }

    public class TypeMeta
    {
        public Type BackingType { get; set; }
        public TypeSource Source { get; set; }
    }

    public class Type
    {
        public string Name { get; set; }
    }

    public class Data : Type
    {
        public Dictionary<string, Field> Fields = new Dictionary<string, Field>();
    }

    public class Field : TypeDefinition { }

    public class TypeDefinition
    {
        public TypeMeta Type { get; set; }
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

    public class Declaration : Statement
    {
        public TypeDefinition TypeDefinition { get; set; }
        public Expression Expression { get; set; }
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

    public class Expression : Statement
    {
        public UnaryExpression UnaryExpression { get; set; }
    }

    public class UnaryExpression
    {
        public MainExpression MainExpression { get; set; }
    }

    public class MainExpression
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
