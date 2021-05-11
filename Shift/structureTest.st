data datastruct
{
    
    String bob;
    String sid;

    Int32 mark;//this shouldn't be an iussue

    Boolean sydney;
//or this
//Int32 foo
}

library blue
{
    Int32 foo()
    {
        return 7;
    }

    String bar(String a)
    {
        return "adsfadf";
    }

    String baz(String a)
    {
        return "another String";
    }

    Int32 buck(String a, Int32 b)
    {
        return 3;
    }

    Int32 sid()
    {

        //declaration
        Int32 f;
        //declaration with initializer
        Int32 g = 0;
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

