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

            return new UnaryExpression
            {
                MainExpression = new MainExpression
                {
                    ExpressionStart = new NewExpression
                    {
                        Identifier = new Identifier { Path = typeName },
                        Invocation = new Invocation { }
                    }
                }
            };

        }

    }
}
