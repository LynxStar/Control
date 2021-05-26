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
        datastruct beeds = new datastruct();

        
        datastruct foo;//C# equivilent to var foo = new datastruct();

        //assignment
        g = 8;//Literal
        g = f;//Identifier
        baz = bob();//expression
        
        foo.bob = "new string value";//accessor chain for assignment

        //expressions
        ////unary expressions
        /////primary expressions

        var quack = new Bob();

        (quack).A();

        //return_expression
        return g;
    }

    datastruct bob()
    {

        return new datastruct();

    }

}


service Bob
{
	
	int A()
	{
		return Foo;
	}
	
	int Foo;
	int Bar;
	
	Bob()
	{
		Foo = 8;
		Bar = 3;
	}
	
	
	
}

library Program
{

    int Main()
    {
    
        int foo = 3;

    }

}
