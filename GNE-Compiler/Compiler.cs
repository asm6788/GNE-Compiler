using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNE_Compiler
{
    class Compiler
    {
        public int bracket;
        string generated = "";
        public void Compile()
        {
            
            File.WriteAllText("output.cs", generated);
        }

        Random random = new Random();
        public string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public class Parser : Compiler
        {
            List<Variable> variables = new List<Variable>();
            Hashtable FunctionTable = new Hashtable();
            Hashtable VariableTable = new Hashtable();
            public new void Function(string source)
            {
                List<Variable> variables = new List<Variable>();
                source = source.Trim();
                if (source.StartsWith("전부 이렇게 해 가지고"))
                {
                    string type = source.Split(' ')[4];
                    string name = source.Split(' ')[5];
                    if (!name.Contains("K-") && !name.Contains("한국형-"))
                    {
                        Console.WriteLine("문법 오류: 희망찬 모두의 대한민국의 청년들을 위한 한국형 한국의 이름 명명방법 규칙이 아닙니다.");
                    }
                    switch (type.Remove(type.Length - 2, 2))
                    {
                        case "공평":
                            type = "void";
                            break;
                        case "적절한숫자":
                            type = "int";
                            break;
                        case "적절한소수":
                            type = "double";
                            break;
                        case "한국어":
                            type = "string";
                            break;
                        default:
                            Console.WriteLine("파서 오류: 돌릴수 없는 그걸..이것을..");
                            break;
                    }
                    if (name == "K-투자")
                    {
                        generated += "static void Main(string[] args";
                    }
                    else
                    {
                        string hash = RandomString();
                        FunctionTable.Add(hash, name);
                        generated += "public static " + type + " " + hash + "(";
                    }
                    bracket++;
                    List<Parameter> parameters = Parameter.ParseRawFunction(source);
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            if (i == parameters.Count - 1)
                            {
                                generated += parameters[i].type + " " + parameters[i].name + ") {" + Environment.NewLine;
                            }
                            else
                            {
                                generated += parameters[i].type + " " + parameters[i].name + ", ";
                            }
                        }
                    }
                    else
                    {
                        generated += ") {" + Environment.NewLine;
                    }
                }
            }
            public new void Variable(string source)
            {
                string[] parse = source.Split(' ');
                string hash = RandomString();
                VariableTable.Add(hash, parse[1]);
                if (parse[4] == "창조")
                {
                    variables.Add(new Variable("var", hash, parse[5].Remove(parse[5].Length - 1, 1).Trim()));
                    generated += "var " + hash + " = new " + parse[5].Remove(parse[5].Length - 1, 1) + ";" + Environment.NewLine;
                }
                else
                {
                    variables.Add(new Variable("var", hash, parse[1]));
                    generated += "var " + hash + " = " + parse[4] + Environment.NewLine;
                }

            }
            public void Exception_Try()
            {
                bracket++;
                generated += "try {" + Environment.NewLine;
            }
            public void Exception_Catch()
            {
                bracket++;
                generated += "catch(Exception e) {" + Environment.NewLine;
            }
            public void Assignment(string source)
            {
                List<Operator> parse =  Operator.Parse(source.Remove(0, source.IndexOf("이것이다") + 5));
                string Converted = VariableTable.Keys.OfType<String>().FirstOrDefault(s => (string)VariableTable[s] == source.Split(' ')[0]);
                generated += Converted + " = ";
                foreach (Operator parsed in parse)
                {
                    switch (parsed.type)
                    {
                        case Operator.Type.String:
                            generated += "\"" + parsed.Contents + "\"";
                            break;
                        case Operator.Type.Variable:
                            generated += VariableTable.Keys.OfType<String>().FirstOrDefault(s => (String)VariableTable[s] == parsed.Contents.Trim());
                            break;
                        case Operator.Type.Function:
                            Function function = Compiler.Function.ParseFunction(parsed.Contents,FunctionTable,VariableTable);
                            generated += function.name + "(";
                            foreach (Parameter parameter in function.parameters)
                            {
                                generated += parameter.name;
                            }
                            generated += ")";
                            break;
                        default:
                            generated += parsed.Contents;
                            break;
                    }
                }
                generated += Environment.NewLine;
            }
            public void Terminate()
            {
                generated += "Environment.Exit(0);" + Environment.NewLine;
            }
            public void OpenBracket()
            {
                bracket++;
                generated += "{" + Environment.NewLine;
            }
            public void CloseBracket()
            {
                bracket--;
                generated += "}" + Environment.NewLine;
            }
            public void Req_GC()
            {
                generated += "GC.Collect();" + Environment.NewLine;
            }
            public void Console_Log(string source)
            {
                source = source.Remove(0, 6);
                source = source.Remove(source.Length - 2, 2);
                generated += "Console.WriteLine("+source+");"+ Environment.NewLine;
            }
        }

        class Operator
        {
            public class SlaveFunction
            {
                public readonly Type type = Type.Function;
                public readonly string Contents;
                public List<SlaveFunction> slave = new List<SlaveFunction>();

                public SlaveFunction(string contents)
                {
                    Contents = contents;
                }
            }
            public readonly Type type;
            public readonly string Contents;
            public List<SlaveFunction> slave = new List<SlaveFunction>(); //제일 먼저보이는 함수

            public Operator(Type type, string contents)
            {
                this.type = type;
                Contents = contents;
            }


            static public List<Operator> Parse(string source)
            {
                bool Instring = false;
                int depth = 0;
                bool startrec = true;
                int Funtionstart = Findfunction(source,0);
                string temp = "";
                List<Operator> operators = new List<Operator>();
                for (int i = 0; i != source.Length; i++)
                {
                    if(i == Funtionstart)
                    {
                        if (depth == 0)
                        {
                            operators.Add(new Operator(Type.Function, temp));
                        }
                        else
                        {
                            AddParseList(ref startrec, ref temp, depth,ref operators,Type.Function,source.Remove(0,i+1));
                        }
                        temp = "";
                        depth++;
                        Funtionstart = Findfunction(source,Funtionstart);
                        continue;
                    }
                    if (source[i] == '"')
                    {
                        temp += source[i];
                        if (Instring)
                        {
                            // 문자열 끝 "~"
                            operators.Add(new Operator(Type.String, temp));
                            temp = "";
                        }
                        Instring = !Instring;
                        continue;
                    }
                    if (!Instring && temp.Trim() != "")
                    {
                        if (temp.Trim() == "*")
                        {
                            AddParseList(ref startrec, ref temp, depth,ref operators);
                            operators.Add(new Operator(Type.Multiplication, "*"));
                            continue;
                        }
                        else if (temp.Trim() == ";'")
                        {
                            AddParseList(ref startrec, ref temp, depth,ref operators);
                            continue;
                        }
                        else if (temp.Trim() == "/")
                        {
                            AddParseList(ref startrec, ref temp, depth,ref operators);
                            operators.Add(new Operator(Type.Division, "/"));
                            continue;
                        }
                        else if (temp.Trim() == "+")
                        {
                            AddParseList(ref startrec, ref temp, depth,ref operators);
                            operators.Add(new Operator(Type.Add, "+"));
                            continue;
                        }
                        else if (temp.Trim().Last() == ')')
                        {
                            AddParseList(ref startrec, ref temp, depth,ref operators);
                            continue;
                        }
                    }
                    if (i == source.Length - 1) //문자열의끝
                    {
                        AddParseList(ref startrec, ref temp, depth,ref operators);
                        startrec = !startrec;
                        continue;
                    }
                    if (startrec)
                    {
                        temp += source[i];
                    }

                }
                return operators;
            }

            static int Findfunction(string source,int ignore)
            {
                bool Instring = false;
                for (int i = 0; i != source.Length; i++)
                {
                    if (source[i] == '"')
                    {
                        Instring = !Instring;
                        continue;
                    }
                    if (!Instring && source[i] == '(') //문자열 안도아니고 함수의 시작점인 함수()에서 (를 감지
                    {
                        if(i > ignore)
                        {
                            return i;
                        }
                    }

                }
                return -1;
            }
            private static void AddParseList(ref bool startrec, ref string temp,int depth ,ref List<Operator> operators,Type type = Type.None,string parameter = null)
            {
                double number = 0;
                startrec = false;
                List<SlaveFunction> slaves = operators.Last().slave;
                if (temp != "")
                {
                    if (type == Type.Function)
                    {
                        for (int i = 0; i != depth - 1; i++)
                        {
                            slaves = slaves.Last().slave;
                        }
                        slaves.Add(new SlaveFunction(temp));
                    }
                    temp = temp.Trim();
                    if (double.TryParse(temp, out number))
                    {
                        operators.Add(new Operator(Type.Number, temp));
                    }
                    else if (!temp.Contains("("))
                    {
                        operators.Add(new Operator(Type.Variable, temp));
                    }
                    temp = "";
                }
            }

            public enum Type
            {
                None,
                Add,
                Minus,
                Subtraction,
                Division,
                Multiplication,
                Modulo,
                String,
                Number,
                Variable,
                Function
            }
        }

        class Variable
        {
            public string type;
            public string name;
            public string value;
            public bool used = false;

            public Variable(string type, string name)
            {
                this.type = type;
                this.name = name;
            }

            public Variable(string type, string name, string value)
            {
                this.type = type;
                this.name = name;
                this.value = value;
            }
        }
        class Function
        {
            public string name;
            public List<Parameter> parameters;
            static List<Function> generated = new List<Function>();
            public Function(string name, List<Parameter> parameters)
            {
                this.name = name;
                this.parameters = parameters;
            }

            public static Function ParseFunction(string source, Hashtable FunctionTable, Hashtable VariableTable)
            {
                string functionname = source.Remove(source.IndexOf("("));
                functionname = FunctionTable.Keys.OfType<String>().FirstOrDefault(s => (String)FunctionTable[s] == functionname);
                List<Function> parsed = new List<Function>();
                //source = source.Remove(0, source.IndexOf("(") + 1);
                //source = source.Remove(source.IndexOf(")"));
                List<Parameter> parameters = new List<Parameter>();

                foreach (Operator op in Operator.Parse(source))
                {
                    switch (op.type)
                    {
                        case Operator.Type.String:
                            parameters.Add(new Parameter("", "\"" + op.Contents + "\""));
                            break;
                        case Operator.Type.Variable:
                            parameters.Add(new Parameter("", VariableTable.Keys.OfType<String>().FirstOrDefault(s => (String)VariableTable[s] == op.Contents.Trim())));
                            break;
                        case Operator.Type.Function:
                            string name = op.Contents.Remove(op.Contents.IndexOf("("));
                            name = FunctionTable.Keys.OfType<String>().FirstOrDefault(s => (String)FunctionTable[s] == name);
                            //foreach (Parameter func in ParseFunction(source, FunctionTable, VariableTable).parameters)
                            //{
                            //    parameters.Add(func);
                            //}
                            break;
                    }
                }
                return new Function(functionname, parameters);
            }
        }
        class Parameter : Variable
        {

            public Parameter(string type, string name) : base(type, name)
            {
                this.type = type;
                this.name = name;
            }

            public static List<Parameter> ParseRawFunction(string source)
            {
                string temp_paramter = source.Remove(0, source.IndexOf("(") + 1);
                temp_paramter = temp_paramter.Remove(temp_paramter.Length - 2);
                if (temp_paramter == "")
                {
                    return null;
                }
                else
                {
                    List<Parameter> parameters = new List<Parameter>();
                    for (int i = 0; i != temp_paramter.Split(',').Length; i++)
                    {
                        string type = temp_paramter.Split(',')[i].Split(' ')[0];
                        switch (type)
                        {
                            case "공평":
                                type = "void";
                                break;
                            case "적절한숫자":
                                type = "int";
                                break;
                            case "적절한소수":
                                type = "double";
                                break;
                            case "한국어":
                                type = "string";
                                break;
                            default:
                                Console.WriteLine("파서 오류: 돌릴수 없는 그걸..이것을..");
                                break;
                        }

                        parameters.Add(new Parameter(type, temp_paramter.Split(',')[i].Split(' ')[1]));
                    }
                    return parameters;
                }
            }

        }
    }
}
