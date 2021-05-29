using Shift.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Services
{
    
    public class ExpressionBuilder
    {

        public Expression NewDefault(string typeName)
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
                                        MainExpression = new MainExpression
                                        {
                                            ExpressionStart = new NewExpression
                                            {
                                                Identifier = new Identifier { Path = typeName },
                                                Invocation = new Invocation { }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

        }

    }
}
