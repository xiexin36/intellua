# Documenting With Doxygen #
Intellua uses class hierarchy from Doxygen generate xml for autocomplete and calltips.
It is recommended to create a separate API header specifically written for Doxygen to parse.

# Generating the xml #
The default way to generate the xml is to put all the c++ style headers (`*`.h) in the `./Doxygen/` folder under the intellua package, and execute `./Doxygen/generateXML.bat`, which will gather all .h files under that folder and generate `classdef.xml`

`./Doxygen/luadefault.h` has already documented all the default functions in Lua5.1. Modules can be disabled by commenting out the `#define` lines.

# Doxygen Constructs Transformed by Intellua #

Doxygen constructs are transformed into Lua concepts by Intellua. The trasnform should match those made by Luabind bindings.

## Descriptions ##
  * Only the brief description is imported.

## Variables ##
  * Variable type can be omitted as in Doxygen.

## Functions ##
  * Function return type can be omiited.
  * Function parameters and its' default values etc. is treated as a single string (the `<argstring>` element in the xml).
  * unction overloading is supported.

## Classes ##
  * Classes are transformed to a table containing all of
  * Nested class is supported.
  * Only single inheritance is supported.
  * Constructor is treated as a function of the class name in the scope of the class.
  * Classes that start with double underscore( `__` ) will not be shown in autocomplete

  * Static variable and functions are directly accessible by the class table.
  * Nonstatic function have a purple icon in autocomplete while static have green, to indicate `class:func()` should be used instead of `class.func()` because of the hidden `self` parameter.

## Enums ##
  * Enums are transformed to a series of static value of no type in its' scope.
  * Named enums have its' value defined in a table of its' name
  * Unnamed enums have its' value defined in the scope the enum is declared

## Namespaces ##
  * Namespaces are transformed to a class with all its' member forced to static.

# Examples #
## Simple Class ##
```
class Vector{
	Vector();
	Vector(float x,float y);
	Vector(Vector v);
	float Dot(Vector v);
	float Length();
	//! Angle between this vector and v, nil for x axis
	float Angle(Vector v);

	void Clamp(float max);
	//! Normalize this vector
	Vector& Normalize();

	//! Rotate this vector
	Vector& Rotate();

	//! Scale this vector
	Vector& Scale(Vector s);

	float x;
	float y;
};
```

```lua

v = Vector()
v.x = 1
v.y = 2
print(v:Length())
```

## Table of Functions ##
```
namespace file{
	//!Closes file.
	close ();

	//!Saves any written data to file. 
	flush ();

	//! Returns an iterator function that, each time it is called, returns a new line from the file.
	lines ();

	//!Reads the file file, according to the given formats, which specify what to read.
	read (...);

	//!Sets and gets the file position, measured from the beginning of the file, to the position given by offset plus a base specified by the string whence
	seek ([whence] [, offset]);

	//!Sets the buffering mode for an output file. 
	setvbuf (mode [, size]);

	//!Writes the value of each of its arguments to the file.
	write (...);
};
```

```lua

file.write("helloworld")
```