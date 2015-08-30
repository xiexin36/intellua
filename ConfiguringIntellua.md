# Configuring Intellua #

Intellua is configured by a xml file named `intelluaConfig.xml` in the same directory as the executable using `intellua.dll`.

Currently the structure of the xml doesn't matter at all. Intellua just recursively search of elements it needs.

## 

&lt;path&gt;

 ##

Paths to search for files required by `require()`, relative to the executable using `intellua.dll`. Multiple `<path>` element can be used.

Intellua will search for files in the following order:
  * Relative to the current Lua file calling `require()`.
  * Relative to paths specified by the configuration, in element order.
  * Relative to the executable's path.
  * Absolute path.

This should conform to how lua's `packaged.loader` search for file.

## 

&lt;ext&gt;

 ##
Extensions to add to the filename to search for files required by `require()`. Multiple `<ext>` elements can be used. Intellua will always search for file without additional extensions first.