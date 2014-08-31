using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    public class Scope
    {
        private List<Scope> m_childs = new List<Scope>();
        private int m_endPos = 0;
        private Scope m_parent = null;

        private int m_startPos = 0;

        private List<Variable> m_variables = new List<Variable>();

        public List<Variable> Variables
        {
            get { return m_variables; }
        }

        private List<Function> m_functions = new List<Function>();
        
        public List<Function> Functions {
            get { return m_functions; }
        }

        public List<Scope> Childs
        {
            get { return m_childs; }
            set { m_childs = value; }
        }

        public int EndPos
        {
            get { return m_endPos; }
            set { m_endPos = value; }
        }

        public Scope Parent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }
        public int StartPos
        {
            get { return m_startPos; }
            set { m_startPos = value; }
        }
        public void addVariable(Variable var)
        {
            Scope s = getScope(var.StartPos);
            s.m_variables.Add(var);
        }

        public void addFunction(Function func) {
            Scope s = getScope(func.StartPos);
            s.m_functions.Add(func);
        }

        public void addChild(Scope child)
        {
            Childs.Add(child);
            child.Parent = this;
        }
        public Scope getScope(int pos)
        {
            foreach (Scope s in m_childs)
            {
                if (s.m_startPos <= pos && s.m_endPos > pos) return s.getScope(pos);
            }
            return this;
        }

        public Variable getVariable(string name, int pos)
        {
            Variable rst = null;
            foreach (Variable v in m_variables)
            {
                if (v.Name != name) continue;
                if (v.StartPos > pos) continue;
                rst = v;
            }
            if (rst != null) return rst;
            if (m_parent != null) return m_parent.getVariable(name, pos);
            return null;
        }

        public Function getFunction(string name, int pos)
        {
            Function rst = null;
            foreach (Function v in m_functions)
            {
                if (v.Name != name) continue;
                if (v.StartPos > pos) continue;
                rst = v;
            }
            if (rst != null) return rst;
            if (m_parent != null) return m_parent.getFunction(name, pos);
            return null;
        }
    }

}
