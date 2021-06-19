using Shift.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Domain
{

    public class ExpressionBase
    {
        public TrackedType Output { get; set; }
    }


    public class ConditionalOrExpression : Expression
    {

        public ConditionalAndExpression ConditionalAndExpression { get; set; }
        public List<ConditionalAndExpression> ConditionalAndExpressions { get; set; } = new List<ConditionalAndExpression>();
    }

    public class ConditionalAndExpression : ExpressionBase
    {
        public EqualityExpression EqualityExpression { get; set; }
        public List<EqualityExpression> EqualityExpressions { get; set; } = new List<EqualityExpression>();
    }

    public enum ConditionalOperator
    {
        AND,
        OR
    }

    public class EqualityExpression : ExpressionBase
    {
        public RelationalExpression Left { get; set; }
        public EqualityOperator EqualityOperator { get; set; }
        public RelationalExpression Right { get; set; }
    }

    public enum EqualityOperator
    {
        Equals,
        NotEquals
    }

    public class RelationalExpression : ExpressionBase
    {
        public AdditiveExpression Left { get; set; }
        public RelationalOperator RelationalOperator { get; set; }
        public AdditiveExpression Right { get; set; }
    }

    public enum RelationalOperator
    {
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public class AdditiveExpression : ExpressionBase
    {
        public MultiplicativeExpression MultiplicativeExpression { get; set; }
        public List<(AdditiveOperator op, MultiplicativeExpression expr)> MultiplicativeExpressions { get; set; }
            = new List<(AdditiveOperator op, MultiplicativeExpression expr)>();
    }

    public enum AdditiveOperator
    {
        Addition,
        Subtraction
    }

    public class MultiplicativeExpression : ExpressionBase
    {
        public UnaryExpression UnaryExpression { get; set; }
        public List<(MultiplicativeOperator op, UnaryExpression expr)> UnaryExpressions { get; set; }
            = new List<(MultiplicativeOperator op, UnaryExpression expr)>();
    }

    public enum MultiplicativeOperator
    {        
        Multiplication,
        Division
    }

    public class Operator
    {
        ConditionalOperator? ConditionalOperator { get; set; }
        EqualityOperator EqualityOperator { get; set; }
        RelationalOperator RelationalOperator { get; set; }
        AdditiveOperator AdditiveOperator { get; set; }
        MultiplicativeOperator MultiplicativeOperator { get; set; }
    }

}
