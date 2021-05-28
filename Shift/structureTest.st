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

library Prover
{

    void ComparisonOperators()
    {
    
        Test.True(3 == 3, "Equal Correct Test");
        Test.True(3 != 4, "NotEqual Correct Test");
        Test.True(3 < 4, "LessThan Correct Test");
        Test.True(3 <= 3, "LessThanEqual Correct Test");
        Test.True(5 > 3, "GreaterThan Correct Test");
        Test.True(3 >= 3, "GreaterThanEqual Correct Test");

        Test.False(3 != 3, "Equal Incorrect Test");
        Test.False(3 == 4, "NotEqual Incorrect Test");
        Test.False(3 > 4, "LessThan Incorrect Test");
        Test.False(4 <= 3, "LessThanEqual Incorrect Test");
        Test.False(3 > 5, "GreaterThan Incorrect Test");
        Test.False(2 >= 3, "GreaterThanEqual Incorrect Test");

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

        while(i != 5)
        {

            //i = i + 1;

        }

    }

    void Old()
    {
        int foo = 3;

        var testService = new Bob();
        var testServiceConstructor = new Bob(5);

        Test.Check(foo, 3, "basic");
        Test.Check(testService.Foo, 0, "basic");

        var binaryTest = 5 == 5;
        var binaryTest2 = 5 != 6;

        Test.True(binaryTest, "basic");
        Test.True(binaryTest2, "basic");

        Test.True(5 == 5, "basic");
        Test.True(5 != 6, "basic");
        Test.Check(5 == 500, false, "basic");
        Test.Check(7 != 7, false, "basic");


        testServiceConstructor.Foo = 8;

        var methodCheck = testServiceConstructor.A();

        Test.True(testServiceConstructor.Foo == methodCheck, "basic");
    }

}

library Program
{

    int Main()
    {
    
        
        Prover.ComparisonOperators();
        Prover.IfControl();

    }

}
