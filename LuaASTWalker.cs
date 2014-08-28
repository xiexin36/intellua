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
            for (int i = 0; i < vl.Count; i++)
            {
                LuaVariable var = vl[i];
                Variable v = new Variable(var.Name);
                if (i >= explist.ComponentGroup.Count)
                {
                    v.Type = m_ac.Types.get("nil");
                }
                else {
                    v.Type = getExpressionType(explist.ComponentGroup[i]);
                }
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
                rst.Add(var);
            }
            return rst;
        }

        List<LuaVariable> getNamelist(LuaAST namelist){
            List<LuaVariable> rst = new List<LuaVariable>();

            foreach (LuaAST v in namelist.ComponentGroup)
            {
                LuaVariable var = getVariable(v);
                rst.Add(var);
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
        
        Type getExpressionType(LuaAST exp) {
            if (exp == null)
            {
                return m_ac.Types.get("nil");
            }
            if (exp.Components.ContainsKey("value")) {
                return getValueType(exp.Components["value"]);
            }
            return getExpressionType(exp.Components["exp"]);
        }

        Type getValueType(LuaAST value)
        {
            if (value.Token != null) {
                switch (value.Token.Type) { 
                    case LuaTokenType.KW_nil:
                        return m_ac.Types.get("nil");
                    case LuaTokenType.KW_false:
                    case LuaTokenType.KW_true:
                        return m_ac.Types.get("bool");
                    case LuaTokenType.Number:
                        return m_ac.Types.get("number");
                    case LuaTokenType.StringLiteral:
                        return m_ac.Types.get("string");
                    case LuaTokenType.OP_ellipsis:
                        return m_ac.Types.get("object");
                }
            }
            if (value.Components.ContainsKey("function"))
            {
                return m_ac.Types.get("function");
            }

            if (value.Components.ContainsKey("tableconstructor"))
            {
                return m_ac.Types.get("table");
            }

            if (value.Components.ContainsKey("functioncall"))
            {
                return getFunctionCallType(value.Components["functioncall"]);
            }
            if (value.Components.ContainsKey("var"))
            {
                return getVarType(value.Components["var"]);
            }
            if (value.Components.ContainsKey("exp"))
            {
                return getExpressionType(value.Components["exp"]);
            }

            return m_ac.Types.get("nil");
        }
        Type getNameType(LuaAST n) {
            string name = Encoding.UTF8.GetString(n.Token.data);
            Variable v = m_ac.Variables.getVariable(name, n.Token.pos);
            if (v != null) return v.Type;
            return m_ac.Types.get(name);   
        }
        Type getVarType(LuaAST var) {
            if (var.Name == "Name") {
                return getNameType(var);
            }

            Type t = getPrefixType(var.Components["prefix"]);
            foreach (LuaAST suffix in var.ComponentGroup)
            {
                t = getSuffixType(t, suffix);
            }
            return t;



            return m_ac.Types.get("nil");
        }
        Type getFunctionCallType(LuaAST func) {
            Type t = getPrefixType(func.Components["prefix"]);
            foreach(LuaAST suffix in func.ComponentGroup){
                t = getSuffixType(t, suffix);
            }
            return t;
        }
        Type getPrefixType(LuaAST prefix) {
            if (prefix.Components.ContainsKey("exp")) { 
                return getExpressionType(prefix.Components["exp"]);
            }
            return getNameType(prefix.Components["name"]);

        }

        Type getSuffixType(Type t,LuaAST suffix) {
            if (suffix.Components.ContainsKey("index")) {
                LuaAST index = suffix.Components["index"];
                if(index.Components.ContainsKey("name")){
                    LuaVariable name = getVariable(index.Components["name"]);
                    if (t.Members.ContainsKey(name.Name)) {
                        return t.Members[name.Name].Type;
                    }
                    if (t.Methods.ContainsKey(name.Name)) {
                        return t.Methods[name.Name].ReturnType;
                    }
                    if(t.InnerClasses.ContainsKey(name.Name)){
                        return t.InnerClasses[name.Name];
                    }
                }
            }

            return t;
        }


    }
}
