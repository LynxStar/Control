structure datastruct
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

        //Let's throw this in here for good measure

        var a = "this is anotehr string";//This will be a useful test case when testing out inference
        int 3 = "";

        //make sure we get some default values in there too
        string b;
        string c;

        return 1;
    }

}

service Red
{

    int a;
    int b;
    int c;
    blue d;

    Red() 
    {
        
    }

    int addAll()
    {

        return a + b + c;

    }

    void honkyhonkhonkhonk()
    {

    }

}