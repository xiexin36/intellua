using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
namespace Intellua
{
    class DoxygenXMLParser
    {
        public static void Parse(string filename, VariableManager variableManager,TypeManager typeManager)
        {
            XDocument doc = XDocument.Load(filename);

            //scan all classes first.
            foreach (XElement node in doc.Descendants("compounddef"))
            {
                if (node.Attribute("kind").Value == "class")
                {
                    string name = node.Element("compoundname").Value;
                    Type t = new Type(name);
                    System.Diagnostics.Debug.Print("Type added: " + name);
                    if (name.StartsWith("__")) {
                        int pos = name.LastIndexOf('_');
                        t.DisplayName = name.Substring(pos + 1).ToLower();
                        t.HideDeclare = true;
                    }
                    typeManager.add(t);
                }
            }
            foreach (XElement node in doc.Descendants("compounddef"))
            {
                if (node.Attribute("kind").Value == "class")
                {
                    string name = node.Element("compoundname").Value;
                    Type t = typeManager.get(name);

                    if (node.Element("basecompoundref") != null)
                    {
                        t.Base = typeManager.get(node.Element("basecompoundref").Value);
                    }

                    foreach (XElement member in node.Descendants("memberdef"))
                    {
                        if (member.Attribute("kind").Value == "variable")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            Type mt = typeManager.get(memberType);
                            Variable var = new Variable(memberName);
                            var.Type = mt;
                            var.Desc = member.Element("briefdescription").Value;
                            t.addMember(var);
                            System.Diagnostics.Debug.Print("Member added: " + memberType + " " + name + ":" + memberName);
                        }
                        else if (member.Attribute("kind").Value == "function")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            Function f = new Function(memberName);
                            f.Param.Add( member.Element("argsstring").Value);
                            f.Desc.Add( member.Element("briefdescription").Value); 

                            if (memberName == name)
                            {
                                f.ReturnType = t;
                                f.Static = true;
                                variableManager.add(f);
                                System.Diagnostics.Debug.Print("Constructor added: " + name + ":" + f.ToString());
                            }
                            else
                            {
                                if (member.Attribute("static").Value == "yes") {
                                    f.Static = true;
                                }
                                Type mt = typeManager.get(memberType);
                                
                                f.ReturnType = mt;

                                t.addMethod(f);
                                System.Diagnostics.Debug.Print("Method added: " + memberType + " " + name + ":" + f.ToString());
                            }
                        }
                    }


                }
                else if (node.Attribute("kind").Value == "file")
                {
                    foreach (XElement member in node.Descendants("memberdef"))
                    {
                        if (member.Attribute("kind").Value == "variable")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            Type mt = typeManager.get(memberType);
                            Variable var = new Variable(memberName);
                            var.Type = mt;
                            var.IsStatic = true;
                            var.Desc = member.Element("briefdescription").Value;
                            variableManager.add(var);
                            System.Diagnostics.Debug.Print("Static variable added: " + memberType + " " + memberName);
                        }
                        else if (member.Attribute("kind").Value == "function")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            Type mt = typeManager.get(memberType);
                            Function f = new Function(memberName);
                            f.Param.Add( member.Element("argsstring").Value);
                            f.ReturnType = mt;
                            f.Desc.Add(member.Element("briefdescription").Value);
                            f.Static = true;
                            variableManager.add(f);
                            System.Diagnostics.Debug.Print("Global function added: " + memberType + " " + f.ToString());
                        }
                    }
                }
            }

        }
    }
}
