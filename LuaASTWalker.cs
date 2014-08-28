using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace Intellua
{
    class LuaVariable {
        public string Name;
        public int StartPos;
    }
    class LuaFuncName {
        public List<string> Names = new List<string>();
        public bool isColon = false;
    }
    class LuaASTWalker
    {
        AutoCompleteData m_ac;
        Scope m_currentScope;
        public LuaASTWalker() { 
         
        }
        public void walk(LuaAST chunk, AutoCompleteData ac) {
            m_ac = ac;
            m_ac.Variables.scope = new Scope();
            m_currentScope = m_ac.Variables.scope;
            m_currentScope.StartPos = chunk.start;
            m_currentScope.EndPos = chunk.end;
            walkChuck(chunk);
        }
        void walkChuck(LuaAST chunk) {
            
            foreach (LuaAST s in chunk.ComponentGroup) {
                walkStatment(s);
            }
        }
        void walkBlock(LuaAST block) {
            Scope s = new Scope();
            s.StartPos = block.start;
            s.EndPos = block.end;
            Scope cs = m_currentScope;
            cs.addChild(s);
            m_currentScope = s;
            walkChuck(block);
            m_currentScope = cs;

        }
        void walkStatment(LuaAST s) {
            if (s.Name == "assignExp") {
                walkAssignExp(s);
            }
            else if (s.Name == "functioncallExp") {
                walkFunctioncallExp(s);
            }
            else if (s.Name == "doExp") {
                walkDoExp(s);
            }
            else if (s.Name == "whileExp")
            {
                walkWhileExp(s);
            }
            else if (s.Name == "repeatExp")
            {
                walkRepeatExp(s);
            }
            else if (s.Name == "ifExp")
            {
                walkIfExp(s);
            }
            else if (s.Name == "forExp")
            {
                walkForExp(s);
            }
            else if (s.Name == "forInExp")
            {
                walkForInExp(s);
            }
            else if (s.Name == "functionExp")
            {
                walkFunctionExp(s);
            }
            else if (s.Name == "localFunctionExp")
            {
                walkLocalFunctionExp(s);
            }
            else if (s.Name == "localAssignExp")
            {
                walkLocalAssignExp(s);
            }

        }

        void walkAssignExp(LuaAST assignExp) {
            LuaAST varlist = assignExp.Components["varlist"];
            LuaAST explist = assignExp.Components["explist"];

            List<LuaVariable> vl = getVariables(varlist);
            foreach (LuaVariable var in vl) {
                Variable v = new Variable(var.Name);
                v.Type = m_ac.Types.get("object");
                v.StartPos = var.StartPos;
                m_currentScope.addVariable(v);
            }

        }
        void walkLocalAssignExp(LuaAST assignExp)
        {
            walkAssignExp(assignExp);
        }
        void walkFunctioncallExp(LuaAST s) { 
            // do nothing
        }
        void walkDoExp(LuaAST s)
        {
            walkBlock(s.Components["block"]);
        }
        void walkWhileExp(LuaAST s)
        {
            walkBlock(s.Components["block"]);
        }

        void walkRepeatExp(LuaAST s)
        {
            walkBlock(s.Components["block"]);
        }
        void walkIfExp(LuaAST s)
        {
            walkBlock(s.Components["block"]);
            foreach (LuaAST expblock in s.ComponentGroup) {
                walkBlock(expblock.Components["block"]);
            }
            if (s.Components.ContainsKey("elseBlock")) {
                walkBlock(s.Components["elseBlock"]);
            }
        }
        void walkForExp(LuaAST st)
        {
            Scope s = new Scope();
            s.StartPos = st.start;
            s.EndPos = st.end;
            Scope cs = m_currentScope;
            cs.addChild(s);
            m_currentScope = s;
            {
                LuaVariable var = getVariable(st.Components["name"]);
                Variable v = new Variable(var.Name);
                v.Type = m_ac.Types.get("object");
                v.StartPos = var.StartPos;
                m_currentScope.addVariable(v);

                walkChuck(st.Components["block"]);

            }
            m_currentScope = cs;
        }
        void walkForInExp(LuaAST st)
        {
            Scope s = new Scope();
            s.StartPos = st.start;
            s.EndPos = st.end;
            Scope cs = m_currentScope;
            cs.addChild(s);
            m_currentScope = s;
            {
                List<LuaVariable> vl = getNamelist(st.Components["namelist"]);
                foreach (LuaVariable var in vl)
                {
                    Variable v = new Variable(var.Name);
                    v.Type = m_ac.Types.get("object");
                    v.StartPos = var.StartPos;
                    m_currentScope.addVariable(v);
                }

                walkChuck(st.Components["block"]);

            }
            m_currentScope = cs;
        }
        void walkFunctionExp(LuaAST st) {
            LuaFuncName funcname = getFuncname(st.Components["funcname"]);

            if (funcname.Names.Count == 1) {
                Variable v = new Variable(funcname.Names[0]);
                v.Type = m_ac.Types.get("function");
                v.StartPos = st.Components["funcname"].start;
                m_currentScope.addVariable(v);
            }
            walkFuncionBody(st.Components["funcbody"]);
           
        }
        void walkLocalFunctionExp(LuaAST s)
        {
            walkFunctionExp(s);
        }

        void walkFuncionBody(LuaAST st) {
            Scope s = new Scope();
            s.StartPos = st.start;
            s.EndPos = st.end;
            Scope cs = m_currentScope;
            cs.addChild(s);
            m_currentScope = s;
            {
                if(st.Components.ContainsKey("parlist")){
                    List<LuaVariable> vl = getNamelist(st.Components["parlist"].Components["namelist"]);
                    foreach (LuaVariable var in vl)
                    {
                        Variable v = new Variable(var.Name);
                        v.Type = m_ac.Types.get("object");
                        v.StartPos = var.StartPos;
                        m_currentScope.addVariable(v);
                    }
                }

                walkChuck(st.Components["block"]);

            }
            m_currentScope = cs;
        }
        List<LuaVariable> getVariables(LuaAST varlist) {
            List<LuaVariable> rst = new List<LuaVariable>();

            foreach (LuaAST v in varlist.ComponentGroup) {
                LuaVariable var = getVariable(v);
                if (var != null) rst.Add(var);
            }
            return rst;
        }

        List<LuaVariable> getNamelist(LuaAST namelist){
            List<LuaVariable> rst = new List<LuaVariable>();

            foreach (LuaAST v in namelist.ComponentGroup)
            {
                LuaVariable var = getVariable(v);
                if (var != null) rst.Add(var);
            }
            return rst;
        }

        LuaVariable getVariable(LuaAST var) {
            if (var.Name == "Name") {
                LuaVariable v = new LuaVariable();
                v.Name = Encoding.UTF8.GetString(var.Token.data);
                v.StartPos = var.start;
                return v;
            }
            return null;
        }

        LuaFuncName getFuncname(LuaAST funcname) {
            LuaFuncName rst = new LuaFuncName();
            foreach (LuaAST n in funcname.ComponentGroup) {
                LuaVariable v = getVariable(n);
                rst.Names.Add(v.Name);
            }
            if(funcname.Components.ContainsKey("colonName")){
                LuaVariable v = getVariable(funcname.Components["colonName"]);
                rst.Names.Add(v.Name);
                rst.isColon = true;
            }

            return rst;
        }
    }
}
