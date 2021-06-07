using Shift.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Services
{
    
    public static class ExpressionExtensions
    {

        public static ConditionalOrExpression ToExpression(this MainExpression mainExpression)
        {

            return new ConditionalOrExpression
            {
                ConditionalAndExpression = new ConditionalAndExpression
                {
                    EqualityExpression = new EqualityExpression
                    {
                        Left = new RelationalExpression
                        {
                            Left = new AdditiveExpression
                            {
                                MultiplicativeExpression = new MultiplicativeExpression
                                {
                                    UnaryExpression = new UnaryExpression
                                    {
                                        MainExpression = mainExpression
                                    }
                                }
                            }
                        }
                    }
                }
            };

        }

    }

    public class ExpressionBuilder
    {

        public Expression TypeDefaultExpression(Domain.Type type)
        {

            var mainExpression = type switch
            {
                SeedType seed => seed.Initializer,
                _ => new MainExpression
                {
                    ExpressionStart = new NewExpression
                    {
                        Identifier = new Identifier { Path = type.Name },
                        Invocation = new Invocation { }
                    }
                }
            };

            return mainExpression.ToExpression();

        }

    }
}
