using Control.Grammar;
using Shift.Aspects;
using Shift.Aspects.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift
{

    public class SyntaxMapper
    {

        public Application MapApplication(SyntaxNode node)
        {

            var app = new Application();

            var aspects = node
                .CGR
                .Select(x => x.Aspect)
                ;

            foreach(var child in aspects)
            {
                //atm it saves it into the known types so nothing to really do with these
                if (child.Rule.Name == "data")
                {
                    MapData(child, app);
                }

                if(child.Rule.Name == "library")
                {
                    MapLibrary(child, app);
                }

            }

            return app;

        }

        public Data MapData(SyntaxNode node, Application app)
        {

            var data = new Data();

            data.Source = "User Defined";
            data.Identifier = node.Identifier.TokenValue;

            app.UsedTypes.Add(data.Identifier, data);

            data.Fields = node
                .CGR
                .Select(x => x.Field)
                .Select(x => MapField(x, app))
                .ToList()
                ;

            return data;

        }

        public Library MapLibrary(SyntaxNode node, Application app)
        {

            var library = new Library();

            library.Source = "User Defined";
            library.Identifier = node.Identifier.TokenValue;

            app.UsedTypes.Add(library.Identifier, library);

            library.Methods = node
                .CGR
                .Select(x => x.Method)
                .Select(x => MapMethod(x, app))
                .ToList()
                ;

            return library;

        }

        public Field MapField(SyntaxNode node, Application app)
        {

            var field = new Field();

            field.Type = app[node.TypeDef.Type.TokenValue];
            field.Identifier = node.TypeDef.Identifier.TokenValue;

            return field;

        }

        public Method MapMethod(SyntaxNode node, Application app)
        {

            var method = new Method();

            method.Signature = MapSignature(node.Signature, app);

            method.Statements = node
                .Block
                .CGR
                .Select(x => x.Statement)
                .Select(x => MapStatement(x, app))
                .ToList()
                ;

            return method;

        }

        public Signature MapSignature(SyntaxNode node, Application app)
        {

            var signature = new Signature();

            signature.Type = app[node.TypeDef.Type.TokenValue];
            signature.Identifier = node.TypeDef.Identifier.TokenValue;

            var parametersNode = node.CGR.SingleOrDefault();

            if(parametersNode is not null)
            {
                signature.Parameters = MapParameters(parametersNode, app);
            }

            return signature;

        }

        public List<Parameter> MapParameters(SyntaxNode node, Application app)
        {

            var parameters = new List<Parameter>();

            var firstParameter = MapParameter(node.Parameter, app);

            var additionalParameters = node
                .CGR
                .Select(x => x.Parameter)
                .Select(x => MapParameter(x, app))
                .ToList()
                ;

            parameters.Add(firstParameter);
            parameters.AddRange(additionalParameters);

            return parameters;

        }

        public Parameter MapParameter(SyntaxNode node, Application app)
        {

            var parameter = new Parameter();

            parameter.Type = app[node.TypeDef.Type.TokenValue];
            parameter.Identifier = node.TypeDef.Identifier.TokenValue;

            return parameter;

        }

        public Statement MapStatement(SyntaxNode node, Application app)
        {

            Statement statement = null;



            return statement;

        }

    }
}
