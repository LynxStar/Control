data datastruct
{
    
    string bob;
    string sid;

    int mark;//this shouldn't be an iussue

    bool sydney;
//or this
//int foo
}

library blue
{
    int foo()
    {
        return 7;
    }

    string bar(string a)
    {
        return "adsfadf";
    }

    string baz(string a)
    {
        return "another string";
    }

    int buck(string a, int b)
    {
        return 3;
    }

    int sid()
    {

        //declaration
        int f;
        //declaration with initializer
        int g = 0;
        datastruct baz = bob();
        var beeds = new datastruct(7);

        
        datastruct foo;//C# equivilent to var foo = new datastruct();

        //assignment
        g = 8;//Literal
        g = f;//Identifier
        g = bob();//expression
        
        foo.bob = 8;//accessor chain for assignment

        //expressions
        ////unary expressions
        /////primary expressions

        (sid).bob();

        //return_expression
        return g;
    }

}




