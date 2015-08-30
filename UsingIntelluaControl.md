# Using Intellua Control #
Intellua's usage is nearly identical to scintillaNET, Expect lexing and styling is hard-configured, so changing it in control properties or constructors will have no effect.

Documentations for scintillaNET can be found at https://scintillanet.codeplex.com/

# Importing Doxygen XML #
Intellua does not attempt to parse any xml by it self. To import a xml, a (mostly common) `Intellua.AutoCompleteData(string filename)` should be constructed with the file, and call Intellua.Intellua.setParent(AutoCompleteData ac) with it. Intellua kept data parsed from lua in a seperate object, so this will not be polluted.