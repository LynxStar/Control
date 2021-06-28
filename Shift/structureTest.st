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
	
	int Foo;
	int Bar;
	
	this(int f)
	{
		Foo = f;
		Bar = 3;
	}
	
	int A()
	{
		return Foo;
	}
	
	
	
}

library BinaryExpressions
{

    void All()
    {
        EqualityOperators();
        RelationalOperators();
    }

    void EqualityOperators()
    {
        Test.True(3 == 3, "Equal Correct Test");
        Test.True(3 != 4, "NotEqual Correct Test");
        Test.False(3 != 3, "Equal Incorrect Test");
        Test.False(3 == 4, "NotEqual Incorrect Test");

    }

    void RelationalOperators()
    {
    
        Test.True(3 < 4, "LessThan Correct Test");
        Test.True(3 <= 3, "LessThanEqual Correct Test");
        Test.True(5 > 3, "GreaterThan Correct Test");
        Test.True(3 >= 3, "GreaterThanEqual Correct Test");

        Test.False(3 > 4, "LessThan Incorrect Test");
        Test.False(4 <= 3, "LessThanEqual Incorrect Test");
        Test.False(3 > 5, "GreaterThan Incorrect Test");
        Test.False(2 >= 3, "GreaterThanEqual Incorrect Test");

    }

    void AdditiveOperators()
    {

        Test.True(3 + 3 == 6, "Addition Test");
        Test.True(10 - 3 == 7, "Subtraction Test");

    }

    void MultiplitiveOperators()
    {
        Test.True(3 * 4 == 12, "Multiplication Test");
        Test.True(12 / 4 == 3, "Division Test");
    }

    

}

library ControlStatements
{

    void All()
    {
        IfControl();
        WhileControl();
    }

    void IfControl()
    {

        var foo = 0;

        var ran = 0;

        if(foo == 0)
        {
            ran = 1;
        }

        //Verify that if statements run
        Test.Check(ran, 1, "If Control True");

        ran = 0;

        if(foo == 1)
        {
            ran = 1;
        }

        //Verify that they don't run when condition is false
        Test.Check(ran, 0, "If Control False");

    }

    void WhileControl()
    {

        var i = 0;

        while(i < 5)
        {

            i = i + 1;

        }

        Test.True(i == 5, "While condition check");

    }

}

library Program
{

    int Main()
    {
    
        
        BinaryExpressions.All();
        ControlStatements.All();

    }

}
