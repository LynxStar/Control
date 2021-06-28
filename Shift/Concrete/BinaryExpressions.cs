using Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Concrete
{
    public class ConditionalOrExpression
    {
        public ConditionalAndExpression ConditionalAndExpression { get; set; }
        [Direct(1)]
        public List<ConditionalAndExpression> ConditionalAndExpressions { get; set; }
    }

    public class ConditionalAndExpression
    {
        public EqualityExpression EqualityExpression { get; set; }
        [Direct(1)]
        public List<EqualityExpression> EqualityExpressions { get; set; }
    }

    public class EqualityExpression
    {
        public RelationalExpression RelationalExpression { get; set; }
        [CGR]
        public EqualityExpressionChain EqualityExpressionChain { get; set; }
    }

    public class EqualityExpressionChain
    {

        public EqualityOperator EqualityOperator { get; set; }
        public RelationalExpression RelationalExpression { get; set; }
    }

    public class EqualityOperator : TokenValue { }

    public class RelationalExpression
    {
        public AdditiveExpression AdditiveExpression { get; set; }
        [CGR]
        public RelationalExpressionChain RelationalExpressionChain { get; set; }
    }

    public class RelationalExpressionChain
    {
        public RelationalOperator RelationalOperator { get; set; }

        public AdditiveExpression AdditiveExpression { get; set; }
    }

    public class RelationalOperator : TokenValue { }

    public class AdditiveExpression
    {
        public MultiplicativeExpression MultiplicativeExpression { get; set; }
        public List<AdditiveExpressionChain> AdditiveExpressionChain { get; set; }
    }

    public class AdditiveExpressionChain
    {
        public AdditiveOperator AdditiveOperator { get; set; }

        public MultiplicativeExpression MultiplicativeExpression { get; set; }
    }
    public class AdditiveOperator : TokenValue { }

    public class MultiplicativeExpression
    {
        public UnaryExpression UnaryExpression { get; set; }
        public List<MultiplicativeExpressionChain> MultiplicativeExpressionChain { get; set; }
    }

    public class MultiplicativeExpressionChain
    {
        public MultiplicativeOperator MultiplicativeOperator { get; set; }

        public UnaryExpression UnaryExpression { get; set; }
    }

    public class MultiplicativeOperator : TokenValue { }

}
