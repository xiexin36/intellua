cd doxygen
doxygen doxyfile
cd..
xslt\xsltproc Doxygen/xml/combine.xslt doxygen/xml/index.xml > all.xml
pause