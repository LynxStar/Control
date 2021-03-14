using Control.Grammar;
using Shift.Aspects;
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

            foreach(var child in node.SyntaxNodes)
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



            return library;

        }

        public Field MapField(SyntaxNode node, Application app)
        {

            var field = new Field();

            field.Type = app[node.TypeDef.Type.TokenValue];
            field.Identifier = node.TypeDef.Identifier.TokenValue;

            return field;

        }

    }
}
