using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GNE_Compiler
{
    internal class Compiler
    {
        public int bracket;
        private string generated = "";

        public void Compile()
        {
            File.WriteAllText("output.cs", generated);
        }

        private Random random = new Random();

        public string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public class Parser : Compiler
        {
            private List<Variable> variables = new List<Variable>();
            private Hashtable FunctionTable = new Hashtable();
            private Hashtable VariableTable = new Hashtable();

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
                List<Operator> parse = Operator.Parse(source.Remove(0, source.IndexOf("이것이다") + 5));
                string Converted = VariableTable.Keys.OfType<String>().FirstOrDefault(s => (string)VariableTable[s] == source.Split(' ')[0]);
                generated += Converted + " = ";
                ParsedToCsharp(parse);
                generated += Environment.NewLine;
            }

            private void ParsedToCsharp(List<Operator> input)
            {
                Operator current = input.First();
                int index = 0;
                int masterindex = 0;
                bool Infunction = false;
                for (int i = 0; true; i++)
                {
                    switch (current.type)
                    {
                        case Operator.Type.Variable:
                            generated += VariableTable.Keys.OfType<String>().FirstOrDefault(s => (String)VariableTable[s] == current.Contents.Trim());
                            break;

                        case Operator.Type.Function:
                            if (!Infunction)
                            {
                                generated += FunctionTable.Keys.OfType<String>().FirstOrDefault(s => (String)FunctionTable[s] == current.Contents.Trim()) + "(";
                                Infunction = true;
                            }
                            break;

                        default:
                            generated += current.Contents;
                            break;
                    }
                    if (index == current.slave.Count - 1 && current.slave.Count != 0)
                    {
                        current = current.slave.ElementAt(index);
                        index++;
                    }
                    else
                    {
                        if (Infunction)
                        {
                            generated += ")";
                            Infunction = false;
                        }
                        index = 0;
                        masterindex++;
                        if (masterindex == input.Count)
                        {
                            break;
                        }
                        current = input.ElementAt(masterindex);
                    }
                }
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
                generated += "Console.WriteLine(" + source + ");" + Environment.NewLine;
            }
        }

        private class Operator
        {
            public class Slave : Operator
            {
                public Slave(Type type, string contents) : base(type, contents)
                {
                }
            }

            public readonly Type type;
            public readonly string Contents;
            public List<Slave> slave = new List<Slave>(); //제일 먼저보이는 함수

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
                int Funtionstart = FindfunctionStart(source, 0);
                int FunctionEnd = FindfunctionEnd(source, 0);
                string temp = "";
                List<Operator> operators = new List<Operator>();
                for (int i = 0; i != source.Length; i++)
                {
                    if (i == Funtionstart)
                    {
                        if (depth == 0)
                        {
                            operators.Add(new Operator(Type.Function, temp));
                        }
                        else
                        {
                            AddParseList(ref temp, depth, ref operators, null, Type.Function);
                        }
                        temp = "";
                        depth++;
                        Funtionstart = FindfunctionStart(source, Funtionstart);
                        continue;
                    }
                    if (i == FunctionEnd)
                    {
                        if (depth == 0)
                        {
                            operators.Add(new Operator(Type.Function, temp));
                        }
                        else
                        {
                            if (temp == " " || temp == "")
                            {
                                continue;
                            }
                            if (temp[0] == ',')
                            {
                                temp = temp.Remove(0, 1);
                            }
                            AddParseList(ref temp, depth, ref operators, ParserParamater(temp), Type.Paramater);
                        }
                        temp = "";
                        depth--;
                        FunctionEnd = FindfunctionEnd(source, FunctionEnd);
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
                            AddParseList(ref temp, depth, ref operators);
                            operators.Add(new Operator(Type.Multiplication, "*"));
                            continue;
                        }
                        else if (temp.Trim() == ";'")
                        {
                            AddParseList(ref temp, depth, ref operators);
                            continue;
                        }
                        else if (temp.Trim() == "/")
                        {
                            AddParseList(ref temp, depth, ref operators);
                            operators.Add(new Operator(Type.Division, "/"));
                            continue;
                        }
                        else if (temp.Trim() == "+")
                        {
                            AddParseList(ref temp, depth, ref operators);
                            operators.Add(new Operator(Type.Add, "+"));
                            continue;
                        }
                    }
                    if (i == source.Length - 1) //문자열의끝
                    {
                        startrec = !startrec;
                        AddParseList(ref temp, depth, ref operators, ParserParamater(temp), Type.Paramater);
                        continue;
                    }
                    if (startrec)
                    {
                        temp += source[i];
                    }
                }
                return operators;
            }

            private static int FindfunctionStart(string source, int ignore)
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
                        if (i > ignore)
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }

            private static int FindfunctionEnd(string source, int ignore)
            {
                bool Instring = false;
                for (int i = 0; i != source.Length; i++)
                {
                    if (source[i] == '"')
                    {
                        Instring = !Instring;
                        continue;
                    }
                    if (!Instring && source[i] == ')') //문자열 안도아니고 함수의 시작점인 함수()에서 )를 감지
                    {
                        if (i > ignore)
                        {
                            if (source[i + 1] != ')')
                            {
                                return i;
                            }
                        }
                    }
                }
                return -1;
            }

            private static void AddParseList(ref string temp, int depth, ref List<Operator> operators, List<Slave> Paramater = null, Type type = Type.None)
            {
                double number = 0;
                string[] unparsed = null;
                List<Slave> slaves = operators.Last().slave;

                if (temp != "")
                {
                    if (type == Type.Function)
                    {
                        for (int i = 0; i != depth - 1; i++)
                        {
                            slaves = slaves.Last().slave;
                        }
                        slaves.Add(new Slave(type, temp));
                    }
                    else if (type == Type.Paramater)
                    {
                        if (depth != 0)
                        {
                            for (int i = 0; i != depth - 1; i++)
                            {
                                slaves = slaves.Last().slave;
                            }
                            slaves.AddRange(Paramater);
                        }
                        else
                        {
                            operators.AddRange(Paramater);
                        }
                    }
                }
                temp = "";
            }

            public static List<Slave> ParserParamater(string paramaters)
            {
                List<Slave> generators = new List<Slave>();
                string temp = "";
                int index = 0;
                bool Instring = false;
                bool startrec = true;
                for (int i = 0; i != paramaters.Length; i++)
                {
                    double number = 0;
                    if (paramaters[i] == '"')
                    {
                        temp += paramaters[i];
                        if (Instring)
                        {
                            // 문자열 끝 "~"
                            generators.Add(new Slave(Type.String, temp));
                            temp = "";
                        }
                        Instring = !Instring;
                        continue;
                    }
                    if (i == paramaters.Length - 1) //문자열의끝
                    {
                        startrec = !startrec;
                        temp += paramaters[i];
                        generators.Add(new Slave(Type.Variable, temp));
                        continue;
                    }
                    if (startrec)
                    {
                        temp += paramaters[i];
                    }
                    if (!Instring && !temp.Trim().Equals(""))
                    {
                        if (temp.Trim().Last() == '*')
                        {
                            generators.Add(new Slave(Type.Multiplication, "*"));
                            temp = "";
                            continue;
                        }
                        else if (double.TryParse(temp, out number))
                        {
                            generators.Add(new Slave(Type.Number, temp));
                            temp = "";
                            continue;
                        }
                        else if (temp.Trim().Last() == '/')
                        {
                            generators.Add(new Slave(Type.Division, "/"));
                            temp = "";
                            continue;
                        }
                        else if (temp.Trim().Last() == '+')
                        {
                            generators.Add(new Slave(Type.Add, "+"));
                            temp = "";
                            continue;
                        }
                        else if (temp.Trim().Last() == ',')
                        {
                            generators.Add(new Slave(Type.Variable, temp.Remove(temp.Length - 1, 1)));
                            temp = "";
                            continue;
                        }
                    }
                }
                return generators;
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
                Function,
                Paramater
            }
        }

        private class Variable
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

        private class Function
        {
            public string name;
            public List<Parameter> parameters;
            private static List<Function> generated = new List<Function>();

            public Function(string name, List<Parameter> parameters)
            {
                this.name = name;
                this.parameters = parameters;
            }

            public static Function ParseFunction(string source, Operator op, Hashtable FunctionTable, Hashtable VariableTable)
            {
                string functionname = source;
                functionname = FunctionTable.Keys.OfType<String>().FirstOrDefault(s => (String)FunctionTable[s] == functionname);
                //source = source.Remove(0, source.IndexOf("(") + 1);
                //source = source.Remove(source.IndexOf(")"));
                List<Parameter> parameters = new List<Parameter>();
                switch (op.type)
                {
                    case Operator.Type.String:
                        parameters.Add(new Parameter("", "\"" + op.Contents + "\""));
                        break;

                    case Operator.Type.Variable:
                        parameters.Add(new Parameter("", VariableTable.Keys.OfType<String>().FirstOrDefault(s => (String)VariableTable[s] == op.Contents.Trim())));
                        break;

                    case Operator.Type.Function:
                        string name = op.Contents;
                        name = FunctionTable.Keys.OfType<String>().FirstOrDefault(s => (String)FunctionTable[s] == name);
                        //foreach (Parameter func in ParseFunction(source, FunctionTable, VariableTable).parameters)
                        //{
                        //    parameters.Add(func);
                        //}
                        break;
                }
                return new Function(functionname, parameters);
            }
        }

        private class Parameter : Variable
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