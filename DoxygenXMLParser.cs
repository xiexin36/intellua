using System;
using System.Linq;
using System.Xml.Linq;

namespace Intellua
{
    internal class DoxygenXMLParser
    {
        #region Methods (1)

        // Public Methods (1) 

        public static AutoCompleteData Parse(string filename)
        {
            XDocument doc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + filename);
            AutoCompleteData autoCompleteData = new AutoCompleteData();
            //scan all classes first.
            foreach (XElement node in doc.Descendants("compounddef"))
            {
                if (node.Attribute("kind").Value == "class" || node.Attribute("kind").Value == "namespace")
                {
                    string name = node.Element("compoundname").Value;
                    string id = node.Attribute("id").Value;

                    if (name.Contains(':'))
                    {
                        int pos = name.LastIndexOf(':');
                        name = name.Substring(pos + 1);
                    }

                    Type t = new Type(id);
                    t.DisplayName = name;

                    //System.Diagnostics.Debug.Print("Type added: " + name);
                    if (name.StartsWith("__"))
                    {
                        int pos = name.LastIndexOf('_');
                        t.DisplayName = name.Substring(pos + 1).ToLower();
                        t.HideDeclare = true;
                    }
                    autoCompleteData.Types.add(t);
                }
            }
            //scan enums
            foreach (XElement node in doc.Descendants("memberdef"))
            {
                if (node.Attribute("kind").Value == "enum")
                {
                    string name = node.Element("name").Value;
                    string id = node.Attribute("id").Value;

                    Type t = new Type(id);
                    t.DisplayName = "enum " + name;

                    //System.Diagnostics.Debug.Print("Enum added: " + name);
                    autoCompleteData.Types.add(t);
                }
            }

            //set outer class for inner class
            foreach (XElement node in doc.Descendants("compounddef"))
            {
                if (node.Attribute("kind").Value == "class" || node.Attribute("kind").Value == "namespace")
                {
                    string id = node.Attribute("id").Value;
                    Type t = autoCompleteData.Types.get(id);

                    foreach (XElement inner in node.Descendants("innerclass"))
                    {
                        if (inner.Attribute("refid") != null)
                        {
                            Type i = autoCompleteData.Types.get(inner.Attribute("refid").Value);
                            i.OuterClass = t;
                        }
                    }
                }
            }

            //add a static variable for access to classes
            foreach (XElement node in doc.Descendants("compounddef"))
            {
                if (node.Attribute("kind").Value == "class" || node.Attribute("kind").Value == "namespace")
                {
                    string id = node.Attribute("id").Value;
                    Type t = autoCompleteData.Types.get(id);

                    Variable var = new Variable(t.DisplayName);
                    var.IsNamespace = true;
                    var.IsStatic = true;
                    var.Type = t;
                    var.Desc = node.Element("briefdescription").Value;

                    if (t.OuterClass == null)
                    {
                        autoCompleteData.Variables.add(var);
                    }
                    else
                    {
                        t.OuterClass.addMember(var);
                    }
                }
            }

            //add member and methods for classes
            foreach (XElement node in doc.Descendants("compounddef"))
            {
                if (node.Attribute("kind").Value == "class" || node.Attribute("kind").Value == "namespace")
                {
                    bool isNamespace = node.Attribute("kind").Value == "namespace";

                    string id = node.Attribute("id").Value;
                    Type t = autoCompleteData.Types.get(id);
                    string name = t.DisplayName;
                    if (node.Element("basecompoundref") != null)
                    {
                        t.Base = autoCompleteData.Types.get(node.Element("basecompoundref").Attribute("refid").Value);
                    }

                    foreach (XElement member in node.Descendants("memberdef"))
                    {
                        if (member.Attribute("kind").Value == "variable")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;
                            string memberTypeID = null;
                            if (member.Element("type").Element("ref") != null)
                                memberTypeID = member.Element("type").Element("ref").Attribute("refid").Value;

                            Type mt = autoCompleteData.Types.get(memberTypeID);
                            Variable var = new Variable(memberName);
                            var.Type = mt;
                            var.Desc = member.Element("briefdescription").Value;

                            if (member.Attribute("static").Value == "yes")
                            {
                                var.IsStatic = true;
                            }

                            if (isNamespace) var.IsStatic = true;
                            t.addMember(var);
                            //System.Diagnostics.Debug.Print("Member added: " + memberType + " " + name + "::" + memberName);
                        }
                        else if (member.Attribute("kind").Value == "function")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;
                            string memberTypeID = null;
                            if (member.Element("type").Element("ref") != null)
                                memberTypeID = member.Element("type").Element("ref").Attribute("refid").Value;
                            Function f = new Function(memberName);
                            f.Param.Add(member.Element("argsstring").Value);
                            f.Desc.Add(member.Element("briefdescription").Value);
                            if (memberName == name)
                            {
                                f.ReturnType = t;
                                f.Static = true;
                                if (t.OuterClass == null)
                                    autoCompleteData.Variables.add(f);
                                else t.OuterClass.addMethod(f);

                                //System.Diagnostics.Debug.Print("Constructor added: " + name + "::" + f.getName() + member.Element("argsstring").Value);
                            }
                            else
                            {
                                if (member.Attribute("static").Value == "yes")
                                {
                                    f.Static = true;
                                }
                                if (isNamespace) f.Static = true;
                                Type mt = autoCompleteData.Types.get(memberTypeID);

                                f.ReturnType = mt;

                                t.addMethod(f);
                                //System.Diagnostics.Debug.Print("Method added: " + memberType + " " + name + ":" + f.getName() + member.Element("argsstring").Value);
                            }
                        }
                        else if (member.Attribute("kind").Value == "enum")
                        {
                            string eid = member.Attribute("id").Value;
                            Type e = autoCompleteData.Types.get(eid);
                            foreach (XElement evalue in member.Descendants("enumvalue"))
                            {
                                Variable var = new Variable(evalue.Element("name").Value);
                                var.IsStatic = true;
                                var.Type = autoCompleteData.Types.NullType;
                                var.Class = e;
                                var.Desc = evalue.Element("briefdescription").Value;
                                t.addMember(var);
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
                            string memberTypeID = null;
                            if (member.Element("type").Element("ref") != null)
                                memberTypeID = member.Element("type").Element("ref").Attribute("refid").Value;
                            Type mt = autoCompleteData.Types.get(memberTypeID);
                            Variable var = new Variable(memberName);
                            var.Type = mt;
                            var.IsStatic = true;
                            var.Desc = member.Element("briefdescription").Value;
                            autoCompleteData.Variables.add(var);
                            //System.Diagnostics.Debug.Print("Static variable added: " + memberType + " " + memberName);
                        }
                        else if (member.Attribute("kind").Value == "function")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;
                            string memberTypeID = null; ;
                            if (member.Element("type").Element("ref") != null)
                                memberTypeID = member.Element("type").Element("ref").Attribute("refid").Value;
                            Type mt = autoCompleteData.Types.get(memberTypeID);
                            Function f = new Function(memberName);
                            f.Param.Add(member.Element("argsstring").Value);
                            f.ReturnType = mt;
                            f.Desc.Add(member.Element("briefdescription").Value);
                            f.Static = true;
                            autoCompleteData.Variables.add(f);
                            //System.Diagnostics.Debug.Print("Global function added: " + memberType + " " + f.getName() + member.Element("argsstring").Value);
                        }
                        else if (member.Attribute("kind").Value == "enum")
                        {
                            string eid = member.Attribute("id").Value;
                            Type e = autoCompleteData.Types.get(eid);
                            foreach (XElement evalue in member.Descendants("enumvalue"))
                            {
                                Variable var = new Variable(evalue.Element("name").Value);
                                var.IsStatic = true;
                                var.Type = autoCompleteData.Types.NullType;
                                var.Class = e;
                                var.Desc = evalue.Element("briefdescription").Value;
                                autoCompleteData.Variables.add(var);
                            }
                        }
                    }
                }
            }
            autoCompleteData.Variables.removeEmptyNamespace();
            autoCompleteData.Types.removeEmptyNamespace();

            return autoCompleteData;
        }

        #endregion Methods
    }
}