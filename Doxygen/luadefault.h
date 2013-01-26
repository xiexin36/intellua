#define LUA_DOCUMANTATION
#define LUA_BASIC
#define LUA_COROUTINE
#define LUA_MODULES
#define LUA_STRING
#define LUA_TABLE
#define LUA_MATH
#define LUA_IO
#define LUA_OS
#define LUA_DEBUG

#ifdef LUA_DOCUMANTATION


#ifdef LUA_BASIC
//! Issues an error when the value of its argument v is false (i.e., nil or false)
assert (v [, message]); 

//! Generic interface to the garbage collector. 
collectgarbage ([opt [, arg]]);

//! Opens the named file and executes its contents as a Lua chunk. 
dofile ([filename]);

//! Terminates the last protected function called and returns message as the error message. 
error (message [, level]);

//! A global variable that holds the global environment.
table _G;

//! Returns the current environment in use by the function.
getfenv ([f]);

//! Returns the metatable of the given object. 
getmetatable (object);

//! Returns three values: an iterator function, the table t, and 0.
ipairs (t);

//! Loads a chunk using function func to get its pieces. 
load (func [, chunkname]);

//! Loads a chunk from file filename
loadfile ([filename]);

//! Loads a chunk from the given string. 
loadstring (string [, chunkname]);

//! Returns the next index of the table and its associated value. 
next (table [, index]);

//! Returns three values: the next function, the table t, and nil
pairs (t);

//! Calls function f with the given arguments in protected mode. 
pcall (f, arg1, ...);

//! Receives any number of arguments, and prints their values to stdout;
print (...);

//! Checks whether v1 is equal to v2, without invoking any metamethod.
rawequal (v1, v2);

//! Gets the real value of table[index], without invoking any metamethod.
rawget (table, index);

//! Sets the real value of table[index] to value, without invoking any metamethod. 
rawset (table, index, value);

//! Returns all arguments after argument number index.
select (index, ...);

//! Sets the environment to be used by the given function. 
setfenv (f, table);

//! Sets the metatable for the given table.
setmetatable (table, metatable);

//! Tries to convert its argument to a number.
tonumber (e [, base]);

//! Receives an argument of any type and converts it to a string in a reasonable format.
tostring (e);

//! Returns the type of its only argument, coded as a string. 
type (v);

//! Returns the elements from the given table. 
unpack (list [, i [, j]]);

//! A global variable that holds a string containing the current interpreter version.
string _VERSION;

//! Calls function f with the given arguments in protected mode with error handler.
xpcall (f, err);
#endif

#ifdef LUA_COROUTINE


class __LUA_COROUTNE{
	//! Creates a new coroutine, with body f.
	static thread create(function f);

	//! Starts or continues the execution of coroutine co. 
	static resume (co [, val1, ...]);

	//! Returns the running coroutine.
	static thread running ();

	//! Returns the status of coroutine co
	static string status (thread co);

	//! Returns a function that resumes the coroutine each time it is called.
	static function wrap (f);

	//! Suspends the execution of the calling coroutine.
	static yield (...);
};

//!The operations related to coroutines 
__LUA_COROUTNE coroutine; 

#endif

#ifdef LUA_MODULES

//! Creates a module.
module (name [, ...]);

//Loads the given module.
require (modname);


class __LUA_PACKAGE{
	//! The path used by require to search for a C loader.
	string cpath;

	//! A table used by require to control which modules are already loaded.
	table loaded;

	//! A table used by require to control how to load modules. 
	table loaders;

	//! Dynamically links the host program with the C library libname. 
	static loadlib (libname, funcname);

	//! The path used by require to search for a Lua loader. 
	string path;

	//! A table to store loaders for specific modules 
	table preload;

	//! Sets a metatable for module with its __index field referring to the global environment.
	static seeall (module);

};
__LUA_PACKAGE package;
#endif


#ifdef LUA_STRING

class __LUA_STRING{

	//! Returns the internal numerical codes of the characters s[i], s[i+1], ..., s[j]. 
	static byte (s [, i [, j]]);

	//! Receives zero or more integers. Returns a string with length equal to the number of arguments.
	static char (...);

	//! Returns a string containing a binary representation of the given function.
	static dump (function);

	//! Looks for the first match of pattern in the string s.
	static find (s, pattern [, init [, plain]]);

	//! Returns a formatted version of its variable number of arguments following the description given in its first argument.
	static format (formatstring, ...);

	//! Returns an iterator function that, each time it is called, returns the next captures from pattern over string s.
	static gmatch (s, pattern);

	//! Returns a copy of s in which occurrences of the pattern have been replaced by repl.
	static gsub (s, pattern, repl [, n]);

	//! Receives a string and returns its length. 
	static len (s);

	//! Receives a string and returns a copy of this string with all uppercase letters changed to lowercase.
	static lower (s);

	//! Looks for the first match of pattern in the string s.
	static match (s, pattern [, init]);

	//! Returns a string that is the concatenation of n copies of the string s. 
	static rep (s, n);

	//! Returns a string that is the string s reversed.
	static reverse (s);

	//! Returns the substring of s that starts at i and continues until j.
	static sub (s, i [, j]);

	//! Receives a string and returns a copy of this string with all lowercase letters changed to uppercase. 
	static upper (s);
};

//!  Provides generic functions for string manipulation.
__LUA_STRING string;

#endif


#ifdef LUA_TABLE

class __LUA_TABLE{
	
	//! Given an array where all elements are strings or numbers, returns table[i]..sep..table[i+1] ... sep..table[j].
	static concat (table [, sep [, i [, j]]]);

	//! Inserts element value at position pos in table, shifting up other elements to open space, if necessary.
	static insert (table, [pos,] value);

	//! Returns the largest positive numerical index of the given table, or zero if the table has no positive numerical indices.
	static maxn (table);

	//! Removes from table the element at position pos, shifting down other elements to close the space, if necessary.
	static remove (table [, pos]);

	//!Sorts table elements in a given order, in-place, from table[1] to table[n], where n is the length of the table.
	static sort (table [, comp]);

};
//! Provides generic functions for table manipulation.
__LUA_TABLE table;

#endif

#ifdef LUA_MATH

class __LUA_MATH{
	//!Returns the absolute value of x. 
	static abs(x);

	//!Returns the arc cosine of x (in radians). 
	static acos(y,x);

	//!Returns the arc sine of x (in radians). 
	static asin (x);

	//! Returns the arc tangent of x (in radians). 
	static atan (x);

	//! Returns the arc tangent of y/x (in radians), but uses the signs of both parameters to find the quadrant of the result.
	static atan2 (y, x);

	//! Returns the smallest integer larger than or equal to x. 
	static ceil (x);

	//! Returns the cosine of x (assumed to be in radians). 
	static cos (x);

	//! Returns the hyperbolic cosine of x. 
	static cosh (x);

	//! Returns the angle x (given in radians) in degrees. 
	static deg (x);

	//! Returns the value e^x. 
	static exp (x);

	//!  Returns the largest integer smaller than or equal to x.
	static floor (x);

	//! Returns the remainder of the division of x by y that rounds the quotient towards zero. 
	static fmod (x, y);
	
	//! Returns m and e such that x = m2^e, e is an integer and the absolute value of m is in the range [0.5, 1) (or zero when x is zero). 
	static frexp (x);
	
	//! The value HUGE_VAL, a value larger than or equal to any other numerical value. 
	static double huge;

	//! Returns m2^e
	static ldexp (m,int e);

	//!Returns the natural logarithm of x. 
	static log (x);

	//!Returns the base-10 logarithm of x. 
	static log10 (x);

	//!Returns the maximum value among its arguments. 
	static max (x, ...);

	//!Returns the minimum value among its arguments. 
	static min (x, ...);

	//!Returns two numbers, the integral part of x and the fractional part of x. 
	static modf (x);

	//!The value of pi. 
	static double pi;

	//!Returns x^y.
	static pow (x, y);

	//! Returns the angle x (given in degrees) in radians.
	static rad (x);

	//! Returns a uniform pseudo-random real number in the range [0,1).
	static double random();

	//! Returns a uniform pseudo-random integer in the range [1, m]. 
	static int random(int m);

	//! Returns a uniform pseudo-random integer in the range [m, n]. 
	static int random(int m,int n);

	//! Sets x as the "seed" for the pseudo-random generator.
	static randomseed (x);

	//!Returns the sine of x (assumed to be in radians). 
	static sin (x);

	//!Returns the hyperbolic sine of x. 
	static sinh (x);

	//!Returns the square root of x.
	static sqrt (x);

	//!Returns the tangent of x (assumed to be in radians). 
	static tan (x);

	//!Returns the hyperbolic tangent of x. 
	static tanh (x);

};
//! Interface to the standard C math library.
__LUA_MATH math;

#endif

#ifdef LUA_IO

class file{
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

class __LUA_IO{
	//! closes the default output file. 
	static close ();
	
	//! Equivalent to file:close()
	static close (file);

	//!Equivalent to file:flush over the default output file. 
	static flush ();
	
	//!opens the named file (in text mode), and sets its handle as the default input file.
	static input (string filename);

	//! returns the current default input file. 
	static input();

	//!sets the file handle as the default input file.
	static input(file filehandle);

	//!Opens the given file name in read mode and returns an iterator function that, each time it is called, returns a new line from the file. 
	static lines ([filename]);

	//opens a file, in the mode specified in the string mode. returns a new file handle.
	static file open (filename [, mode]);

	//!opens the named file (in text mode), and sets its handle as the default output file.
	static output (string filename);

	//! returns the current default output file. 
	static output();

	//!sets the file handle as the default output file.
	static output(file filehandle);

	//!Starts program prog in a separated process and returns a file handle that you can use to read or write data to this program. 
	static popen (prog [, mode]);

	//!Equivalent to io.input():read. 
	static read (...);

	//!Returns a handle for a temporary file.
	static tmpfile ();

	//! Checks whether obj is a valid file handle.
	static type (obj);

	//! Equivalent to io.output():write. 
	static write (...);
};

__LUA_IO io;

#endif

#ifdef LUA_OS

class __LUA_OS{
	//!Returns an approximation of the amount in seconds of CPU time used by the program. 
	static clock ();

	//!Returns a string or a table containing date and time, formatted according to the given string format. 
	static date ([format [, time]]);

	//!Returns the number of seconds from time t1 to time t2.
	static difftime (t2, t1);

	//!passes command to be executed by an operating system shell. Returns a status code.
	static execute ([command]);

	//!terminate the host program
	static exit ([code]);

	//!Returns the value of the process environment variable varname
	static getenv (varname);

	//!Deletes the file or directory with the given name.
	static remove (filename);

	//! Renames file or directory named oldname to newname.
	static rename (oldname, newname);

	//! Sets the current locale of the program.
	static setlocale (locale [, category]);

	//!Returns the current time
	static time ();

	//!Returns a time representing the date and time specified by the given table.
	static time(table);

	//!Returns a string with a file name that can be used for a temporary file. 
	static tmpname ();
};
//! Operating System Facilities
__LUA_OS os;

#endif

#ifdef LUA_DEBUG

class __LUA_DEBUG{
	//!Enters an interactive mode with the user, running each string that the user enters.
	static debug ();

	//!Returns the environment of object o. 
	static getfenv (o);

	//!Returns the current hook settings of the thread
	static gethook ([thread]);

	//!Returns a table with information about a function.
	static getinfo ([thread,] function [, what]);

	//!returns the name and the value of the local variable with index local of the function at level level of the stack.
	static getlocal ([thread,] level, local);

	//!Returns the metatable of the given object or nil if it does not have a metatable. 
	static getmetatable (object);

	//!Returns the registry table.
	static getregistry ();

	//! returns the name and the value of the upvalue with index up of the function func.
	static getupvalue (func, up);

	//!Sets the environment of the given object to the given table. Returns object. 
	static setfenv (object, table);

	//!Sets the given function as a hook. 
	static sethook ([thread,] hook, mask [, count]);

	//!assigns the value value to the local variable with index local of the function at level level of the stack.
	static setlocal ([thread,] level, local, value);

	//!Sets the metatable for the given object to the given table.
	static setmetatable (object, table);

	//!This function assigns the value value to the upvalue with index up of the function func.
	static setupvalue (func, up, value);

	//!Returns a string with a traceback of the call stack.
	static traceback ([thread,] [message [, level]]);

};
//!Provides the functionality of the debug interface to Lua programs.
__LUA_DEBUG debug;

#endif

#endif