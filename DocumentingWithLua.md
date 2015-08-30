# Documenting With Lua #

Intellua allows declaration and documentation of functions, classes, and variables in individual Lua files. By using `require()`, class info in those files will also be imported.


# Documentation Block #
Any documentation must be inside a documentation block to have Intellua parse them. Documentation Block is a Lua comment that start immediately with a "!", such as
```lua

--!   documents...
```
or
```lua

--[[!
documents...
]]
```
All documentation blocks are joined into a single one before any parsing occurs.

Strings in a documentation block must follow the following syntax. In the current version, all parsing in a single Lua file is disabled if any syntax error occurs in a documentation block.

## Comment ##
c++ style comments that doesn't start with a "!" are ignored.
```
// single line comment
/*
block comment
*/
```

## Document comment ##
Document comment is used to give a brief description to the next entity, which is shown in autocomplete or calltip.

Document comment is a c++ style comment that starts immediately with a "!".
```
//! foo is something
class foo{};
/*!
bar do
something else
*/
void bar();
```

As a document comment is only designed for brief description, newlines and extra whitespaces in document comments are stripped.

In the current version, no more than one document comment is allowed for a single entity. extra comments might be concatenated.

## Variable declaration ##
Variable declarations follow the syntax:

> `[`documantation`]` `[`**static**`]` `[`type`]` name `[` **[** number **]** `]` `[` **`=`** defaultValue `]` **;**

items in `[``]` is optional.

Static variable is directly accessible in the namespace the variable is declared. Global variables are always static whether the keyword static is used or not.

Example:
```
//! a
foo a;

foo b[2];

static foo c = "abc";


//! this could be foo or bar or something else.
unknownObject;  //type can be omitted.
```

## Function declaration ##
Function declarations follow the syntax:

> `[`documantation`]` `[`**static**`]` `[`returnType`]` name **(** parameterList **)** **;**

static functions will be shown in the autocomplete list with a green icon instead of purple, indicated a `self` object is not used implicitly in the parameter list, so it should be called with `type.foo()` instead of `object:foo()`

overloaded function is allowed.

Example:
```
//! returns square root of x
float sqrt(float x);

void foo();
void foo(x, baz y = 1); //overloading
```
## Class declaration ##
Class declarations follow the syntax:
> `[`documantation`]` **class** `[`name`]` `[` **:** baseClass `]` **{** declarations **}** `[`objectName`]` **;**

Class name can be omitted. unnamed class will not show up in auto complete and the only way to declare object of unnamed class is declaring it directly after the "}".

Member variables, functions, and nested classes can be declared inside a class.

Classes can inherit from a base class, which imports every declarations in that class. Only single inheritance is allowed

Member function with the same name as the class is a constructor. constructors will be shown in the scope same as the class, instead of inside it.

Example:
```
//! foo
class foo{
  //! a new foo
  foo(); //constructor
  
  int x; //member

  void doStuff(); //method
  
  class bar {}; //nested class

  static bar y; //static member

  static void doMoreStuff(); // static method;
};

class baz : foo{}; //inherit from foo;

class {
  int x;
  foo y;
} z;   //z of unnamed class, has member x and y;
```