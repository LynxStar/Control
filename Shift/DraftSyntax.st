int GetAge(Animal foo)
{

    if(foo is Cat x)
    {

        var multiplier = x.Lives;//Only exists on the Cat class

        return foo.Age * multiplier;

    }

    if (foo is Dog x)
    {

        return foo.Age + 10;

    }

}

//Improvements for shift

int GetAge(Animal foo)
{

    //Foo is an Animal:{any}
    //I'll get to the :{any} at a later time

    if(foo is Cat)//Pattern is asserted
    {//foo's type is Cat inside of lexical scope

        

        var multiplier = x.Lives;//Only exists on the Cat class

        return foo.Age * multiplier;

    }


    //foo's Type returns to Animal:{any} after the if block's lexical scope is exited

    if (foo is Dog)//Pattern is asserted
    {//foo's type is Dog inside of lexical scope


        return foo.Age + 10;

    }

    //foo's Type returns to Animal:{any} after the if block's lexical scope is exited


}

//I think this is technically dynamic typing because foo is declared as an Animal, and it's workable type changes, in an at compile time way
//In the C# syntax this is bypass via introducing a local scope variable, x 
//I don't think calling this dynamic typing is a useful perspective though, the typing never changed.
//It will 100% of the time come in as an Animal
//Because of covariance foo might be a Cat at runtime, this is normal polymorphism and still is fully statically typed.
//Inside of the if block, we KNOW that it's a Cat or a Dog respectively. Furthermore the compiler knows, which is why C# 8's pattern matching works
//This is also why C#'s pattern local variables (this is where the x is allocated from)
//Prior to it you could imagine the first line inside of the if block being `var x = (Cat)foo;` which is unsafe when isoalted in a vacuum
//But in light of the if statement immediately preceeding it, we know it's a Cat

//Anywho, I am refering to this behavior as asserted typing. It's statically typed, in that the types are fixed and fully known at compile time
//The introduction of type inference such as var foo = 3; does not break static typing, because foo's type is 100% provable and knowable by the compiler.
//This is because we've `proven` sic that 3 is a constant of type Int32 (via the constants system bootstrapping that), that's how var gains it's knowledge

//So with this in mind, I am thinking that any pattern match will apply it's guarentee to the underlying variable check
//Because type dependence exists, that means all aspects of the pattern can be applied to this variable, let me throw out another concept before diving further into that


int GetAge(Animal foo)
{

    var myArray = new Array<10,BigDonkies>; //Understood by the compiler as Array<15:BigDonkies>:{any}
            //Just walking through some previous concepts
            //We declared a new Array of BigDonkies, with a size of 10
            //The type definition of Array is Array<Size:int,T>
            //10 is the first type argument, and it is internally referenced as Size within the Array class
            //It's declared as a dependent type, bound to an int, aka a value of Type int will be passed in and then lifted to a type
            //The second argument is BigDonkies, and the TypeArgument is left open, so any type could be passed in
            //In C# you'd have new Array<BigDonkies>(10);
            //The 10 becomes part of the Array's type, since it's static, it's an aspect of the declared array and cannot changed
            //In Shift, all types have a default constructor, so you just do `new X;` you do not need to invoke a constructor
            //Again not explaining the :{any} part

    if(foo.Age > 15)
    {
        
        myArray[foo.Age].DonkeyHonk();
            //C# will compile no problem (ignore the Shift syntax)
            //This is because an array indexer only checks that an Int32 is passed in
            //But any not Jimmy developer can tell that this is 100% of the time going to crash since the array only has 0-9 as valid indexes
            //And we're passing in something that's guarenteed to be over 15

        //So how can we make the compiler help this?
        //In C# the type is fixed when the variable is declared. So it's just Animal
        //In shift foo's type inside of the block would be differnet than C#

        //foo's type is Animal:{Age > 15}
        
            //I call this is a ValueAssertion
            //Many languages support something called TypeAssertions, check out C#'s syntax from 2005

            //var bar = foo as Cat;
            //bar is a Cat
            //Semantically we might htink of this as Nullable<Cat> because all reference types (classes) are Nullable

            //This is similiar to a TypeAssertion, but we can assert any statically provable claim
            
            //Somewhere will prove or disprove a claim. If the proof is valid, then assertion of the claims is made.

            //This only can be done on compiler time things, which is basically anything pure
        

    }


}

//Let's take a look at what the indexer method for an Array would look like in Shift (ignore that Arrays are bootstrapped, this would be what you're IDE would show you)

data Array<Size:int, T> where Size > 0
{

    ///Skipping everythign but the indexer

    T this[int index] where index >= 0 and < Size
    {

        //The type for index inside of this block is int:{>= 0 and < Size}
        //Size is an alias to int:{ > 0}

        //Code to retrieve and return the element of the index
    }

    //The where clause is VERY different than the where clause you'd see in a C# method
    //Next the index isn't generic (they can't be in C# and likely wont be able to be in Shift, mostly because of syntax limitations, but indexes are 100% syntatic sugar anyways)
    //This isn't different in Shift, just pointing this out because in C#, where clauses are ONLY possible for generic methods or class definitions


}


//So looking back at this example

int GetAge(Animal foo)
{

    var myArray = new Array<10,BigDonkies>; 

    myArray[foo.Age].DoSomething();//Throws a compilation error because of a type mismatch


    //Age `might` be int:{>= 0 and < 10} but it could be a negative or 10 or greater
    //If you notice in the method efinition it's < Size but here it's 10
    //This is because of type dependence. Size is the type argument identifier within the class, but an instantiatied instance of the class, will have a value passed in here
    //The same way the C# compiler will bitch about this

    //Cat bar = foo;
    //It's POSSIBLE that the runtime value of foo is a Cat, but it isn't guarenteed, hence the compiler throws an error and blocks

    //Value asserted, asserted typeing, proven typing? naming is irrelevent for now

    //Point is the pattern can be lifted into the TypeSignature of anything
    //This means we can get type checking from the pattern
    //Syntax for this requires the pattern MUST be pure and guarentee to halt

    //So if I have a less asserted version how do I interact with the index?
    //I must make assertions to prove it, lifting the type into it's more knowledgable version
    //Property discovery?


    if(foo.Age > 15)
    {

        //Age is now int:{ > 15 }
        myArray[foo.Age].DoSomething();//This still fails to compile, because we know before even running the program 100% of all times this code is hit it would fails
        //By making the pattern assertions part of the type system, we can let the compiler worry about these things

    }

    if(foo.Age > 2 and < 8)
    {
        
        myArray[foo.Age].DoSomething();//NOW it lets us compiler it. Let's explore this

        //The type system requires int:{>= 0 and < 10}
        //and we've proven it to be int:{>2 and < 8}

        //Because it must be the pattern matching restrictions, it's possible for the compiler to reason about this
        //It's basically looking for a range intersection

    }


}

//Now let's revisit our indexer for a bit to see the benefits on the definition side

//the datastructure definition
data Array<Size:int, T> where Size > 0
//And the index definition
T this[int index] where index >= 0 and < Size

//Some of the benefits of this are that the where clause doesn't need any unit tests around boundaries
//You don't need to test -1, you don't need to test null, you don't need to test a value that is too big
//The where clause on the Array definition ensures you only have a positive size array
//You get a whole boat load of guarentees, the method is protected from being passed a bad value
//This basically saves you having to thrown an exception for an InvalidArrayIndex
//This shifts the developer mistake to the compiler, which through IDE tooling, can shift it to realtime as they type

Let's marry this concept with type aliasing

type PositiveInt = int i where i > 0;
type ArrayIndex<x:PositiveInt> = int i where i >= 0 and < x

data Array<Size:PositiveInt, T>
{

    T this[ArrayIndex<Size> index]

}

//All of the same typechecks but this let's us build up a standard library of type/value/property assertions (really need a stingent name)
//We can then reuse/leverage them for cleaner more concise code
//Unpacking the ArrayIndex<Size>

//ArrayIndex has 1 type argument x:PositiveInt
//The identifier of the argument is x, and it's a dependent type of PositiveInt
//This means anywhere you have an ArrayIndex it'll have a value of PositiveInt passed in

//PositiveInt is an integer, that proves an assertion

//Size is a TypeArgument from the data level (think class Type argument)
//It's type it's a dependent type of PositiveInt
//This means Size is a identifier whose type is an identical match to the type argument for ArrayIndex

//Let's look back at our example Array declaration and call example and walk through how that expands out the type system

var myArray = new Array<10,BigDonkies>;

//10 is a constant, you can imagine it as being this int:{10}
//Size as a type argument requires that it be an instance of PositiveInt
//The compiler checks if 10 is a PositiveInt
//PositiveInt is an int, so pass there!
//Then there is a pattern of x => x > 0; //Local variable in the type declaration use i, I'm intentionally using x to demostrate the pattern operation applied doesn't give a shift
//10 is constant, so it's realized, it's pure, and it halts. Evaluating what 10 is, is just 10 => 10 AKA a constant. Stating the tautology to point out we KNOW what the input is
//So mentally we can imagine the computer going
//10 => 10 > 0;//This is a binary expression and it values to true
//So check on the pattern assertion
//Since all aspects of the Type defintion have been met, and are 100% guarenteed under all permuations of 10 (which is exactly 1 since it's a constant, really stressing why it's a constant)
//This means we can implicitly, lift it into a PositiveInt and furthermore we can begin filling in some of those arguments PositiveInt:{10:{>0}}

    //We've proven PositiveInt is value 10
    //We've proven 10 is > 0

//When it comes to using the indexer we have similiar conversions

myArray[7].DoSomething();

//So we need to pass in an ArrayIndex<Size> Size being the type argument identifier for the datastructure
//Since the datastructure has been declared, it's gone from open generic to closed

//Size is a PositiveInt, which as entioned above you imagine inspecting the inside of the datastructure and seeing your IDE tell you it's PositiveInt:{10:{>0}}
//Replacing this inside of ArrayIndex<PositiveInt:{10:{>0}}>
//This type is an alias to int i where i >= 0 and < x
//x being the PositiveInt:{10:{>0}} type argument
//Further expanding 

//int i where i >= 0 and < PositiveInt:{10:{>0}}
//Rewriting in lambda syntax, to show how the pattern will be evaluated
//x => x >= 0 && x < 10
//10's inner assertions do not need to be rechecked by the lambda, because it's already been proven!!!, hence by we don't continuously check that 10 > 0

//C#

class Array<T>
{

    private readonly int size;

    public Array(int size)
    {
        this.size = size;
    }

    T this[int index]
    {
        if(index < 0)
        {
            throw new InvalidIndexException();
        }
        if (index >= size)
        {
            throw new InvalidIndexException();
        }

        //do stuff

    }
}

//Shift with explicit where clauses, or maybe we call them guards?

data Array<Size:int, T> where Size > 0
{
    T this[int index] where index >= 0 and < Size { //do stuff }

}

//Shift using Type aliases to have more reusable parts
//Such as anywhere that wants to restrict to a positive integer

data Array<Size:PositiveInt, T>
{

    T this[ArrayIndex<Size> index] { //do stuff }

}