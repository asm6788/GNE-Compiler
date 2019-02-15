using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GNE_Compiler
{
    internal class Compiler
    {
        public int bracket;
        private List<string> generated = new List<string>();

        public void Compile()
        {
            Console.WriteLine("개,돼지->사회 컴파일중");
            string[] usings = { "using System;", "using System.Runtime.CompilerServices;", "using Microsoft.CSharp.RuntimeBinder;", "using System.Diagnostics;" };
            string TEMPLATE_A = string.Join("\r\n", usings) + "\r\nnamespace CSharp_Script_Namespace {\r\n class Program { \r\n";
            string TEMPLATE_B = "}}";
            string[] REFERENCES = { "System.Dynamic.Runtime.dll", "Microsoft.CSharp.dll", "System.Core.dll" };

            File.WriteAllText("output.cs", TEMPLATE_A + string.Join("\r\n", generated.ToArray()) + TEMPLATE_B);
            CompilerResults result = Compile(TEMPLATE_A + string.Join("\r\n", generated.ToArray()) + TEMPLATE_B, AppDomain.CurrentDomain.BaseDirectory + "output.exe", REFERENCES);
            if (result.Errors.Count > 0)
            {
                foreach (CompilerError CompErr in result.Errors)
                {
                    Console.WriteLine("C# Line number " + (CompErr.Line - 2) +
                        ", Error Number: " + CompErr.ErrorNumber +
                        ", '" + CompErr.ErrorText);
                    Console.WriteLine("그네어 Line number " + (CompErr.Line - 1));
                }
            }
            Console.WriteLine("개,돼지->사회 컴파일완료");
        }

        private static CompilerResults Compile(string source, string outputFile, params string[] references)
        {
            CompilerParameters parameters = new CompilerParameters(references, outputFile);
            parameters.GenerateExecutable = true;

            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                return provider.CompileAssemblyFromSource(parameters, source);
            }
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
            private List<Function> List_Function = new List<Function>();
            private List<string> Un_used = new List<string>();
            private Function current_func = null;
            private static Hashtable FunctionTable = new Hashtable();
            private static Hashtable VariableTable = new Hashtable();

            public new void Function(string source, int location)
            {
                string hash = RandomString();
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
                    switch (type)
                    {
                        case "공평하게":
                            type = "void";
                            break;

                        case "적절한숫자하게":
                            type = "int";
                            break;

                        case "적절한소수하게":
                            type = "double";
                            break;

                        case "한국어로":
                            type = "string";
                            break;

                        default:
                            Console.WriteLine("파서 오류: 돌릴수 없는 그걸..이것을..");
                            break;
                    }
                    FunctionTable.Add(name, hash);
                    generated.Add("public static " + type + " " + hash + "(");
                    bracket++;
                    List<Parameter> parameters = Parameter.ParseRawFunction(source);
                    if (parameters != null)
                    {
                        foreach (Parameter parameter in parameters)
                        {
                            hash = RandomString();
                            VariableTable.Add(parameter.name, hash);
                        }
                    }
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            if (i == parameters.Count - 1)
                            {
                                Append_Gen(parameters[i].type + " " + VariableTable[parameters[i].name] + ") {");
                            }
                            else
                            {
                                Append_Gen(parameters[i].type + " " + VariableTable[parameters[i].name] + ", ");
                            }
                        }
                    }
                    else
                    {
                        Append_Gen(") {");
                    }
                    List_Function.Add(new Function(hash, generated.Count - 1, parameters));
                    current_func = List_Function[List_Function.Count - 1];
                }
            }

            public void Process_Raw_Code(string[] source)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    source[i] = source[i].Trim();
                    if (source[i].Contains("Gmail"))
                    {
                        Console.WriteLine("안보 경고: Gmail을 사용하는 사람은 종북세력입니다. 믿을 수 있는 샵메일을 대신 사용하십시오.");
                    }
                    else if (source[i].StartsWith("익명"))
                    {
                        Console.WriteLine("국가보안법에 의거한 안보 경고: 어떤 함수가 테러방지법에 저촉되는 복면을 쓰고 있다는 것이 통신 감청을 통해서 감지되는 이런 상황에 발목을 잡는 야당이 이렇게 IS(이슬람국가)도 지금 얼굴을 감추고 그렇게 하고 있지 않느냐 하는 이번에야말로 배후에서 불법을 조종하고, 폭력을 부추기는 세력들을 법과 원칙에 따라 엄중하게 처리해서 종북 세력을 색출, 근절하고 불법과 폭력의 악순환을 끊어내야 할 것이다 하는 과정으로 마음으로 창조경제 제가 해내겠습니다.");
                    }
                    else if (source[i].StartsWith("전부 이렇게 해 가지고"))
                    {
                        Function(source[i], i);
                    }
                    else if (source[i].StartsWith("/ㄹ") || source[i].StartsWith("/근"))
                    {
                        Console.WriteLine("주석 " + source[i]);
                    }
                    else if (source[i].StartsWith("공천"))
                    {
                        Variable(source[i], false);
                    }
                    else if (source[i].StartsWith("코드 텅텅 빌때까지 한번 해 보세요"))
                    {
                        Exception_Try();
                    }
                    else if (source[i].StartsWith("예외처리 다 어디 갔냐고"))
                    {
                        Exception_Catch();
                    }
                    else if (source[i].StartsWith("메모리 텅텅 빌 때까지 한번 해 보세요 쓰레기들 다 어디 갔냐고"))
                    {
                        Req_GC();
                    }
                    else if (source[i].StartsWith("될때까지"))
                    {
                        Loop_while(source[i]);
                    }
                    else if (source[i].StartsWith("법률공포"))
                    {
                        Loop_for(source[i]);
                    }
                    else if (source[i].StartsWith("만약"))
                    {
                        IF(source[i]);
                    }
                    else if (source[i].Contains("이것이다"))
                    {
                        Assignment(source[i]);
                    }
                    else if (source[i] == "메르스();")
                    {
                        BreackPoint();
                    }
                    else if (source[i] == "고심 끝에 프로세스 해체;")
                    {
                        Terminate();
                    }
                    else if (source[i] == "{")
                    {
                        OpenBracket();
                    }
                    else if (source[i] == "}")
                    {
                        CloseBracket();
                    }
                    else if (source[i] == "게임")
                    {
                        Console.WriteLine("이 언어로 게임을 만들 수 없습니다. 왜냐하면 게임은 마약이기 때문이죠.");
                    }
                    else if (source[i].StartsWith("콘솔.로그"))
                    {
                        Console_Log(source[i]);
                    }
                    else if (source[i].StartsWith("누설한다"))
                    {
                        Fucntion_Return(source[i]);
                    }
                    else if (source[i].StartsWith("사회"))
                    {
                        generated.Add("private static void Main(string[] args){");
                    }
                    else if (Regex.IsMatch(source[i], "((?:[ㄱ-힗]+))(\\s+)((?:[ㄱ-힗]+))(\\s+)(\\{)"))
                    {
                        DeclareClass(source[i]);
                    }
                    else
                    {
                        Console.WriteLine("알수없는 토큰: " + source[i]);
                    }
                }
            }

            private void IF(string source)
            {
                generated.Add("if(" + Operator.Parse_IF(source.Remove(0, source.IndexOf('(') + 1).Remove(source.LastIndexOf(')') - source.IndexOf('(') - 1)) + "){");
            }

            private void DeclareClass(string source)
            {
                generated.Add("public class "  + source.Remove(source.Length-2).Replace(' ','_')+" {");
            }

            private void Loop_for(string source)
            {
                source = source.Remove(0, 5);
                source = source.Remove(source.LastIndexOf(')'));
                string[] parts_raw = source.Split(';');
                string[] parts = new string[3];
                parts[0] = "int " + Variable(parts_raw[0] + ";", true);
                parts[1] = Operator.Parse_IF(parts_raw[1] + ";") + ";";
                string[] Splited = parts_raw[2].Trim().Split(' ');
                string Left = ParsedToCsharp(Operator.Parse(Splited[0]));
                string Other = "";
                int i = 0;
                foreach (Operator op in Operator.Parse(parts_raw[2]))
                {
                    if (i == 0)
                    {
                        Other += op.Contents + "=";
                        i++;
                        continue;
                    }
                    Other += op.Contents;
                }
                parts[2] = Left + Other;
                generated.Add("for(" + parts[0] + parts[1] + parts[2] + "){");
            }

            private void Loop_while(string source)
            {
                throw new NotImplementedException();
            }

            private void BreackPoint()
            {
                generated.Add("if(System.Diagnostics.Debugger.IsAttached)");
                generated.Add("Debugger.Break();");
            }

            public void Fucntion_Return(string source)
            {
                generated.Add("return " + VariableTable[source.Remove(0, 5)] + ";");
            }

            public new string Variable(string source, bool Need_Converted)
            {
                source = source.Trim();
                string[] parse = source.Split(' ');
                if (parse[4] == "전부" && parse[5] == "이렇게" && parse[6] == "해")
                {
                    Console.WriteLine("익명성은 대한민국의 안보를 안전치 못하게 합니다.");
                    return "";
                }
                string name = RandomString(); ;
                int index = current_func.location;
                parse[4] = parse[4].Remove(parse[4].Length - 1, 1);
                if (VariableTable[parse[1]] == null)
                {
                    VariableTable.Add(parse[1], name);
                }
                if (parse[4] == "창조")
                {
                    if (!Need_Converted)
                    {
                        generated.Insert(index, "static dynamic " + name + " = null;");
                    }
                    source = source.Remove(0, source.IndexOf("이것이다") + 7);
                    variables.Add(new Variable("var", name, parse[5].Remove(parse[5].Length - 1, 1).Trim()));
                    if (!Need_Converted)
                    {
                        generated.Add(name + " = new " + ParsedToCsharp(Operator.Parse(source)) + ";");
                    }
                    else
                    {
                        return name + " = new " + ParsedToCsharp(Operator.Parse(source)) + ";";
                    }
                }
                else
                {
                    if (!Need_Converted)
                    {
                        generated.Insert(index, "static dynamic " + name + " = null;");
                    }
                    source = source.Remove(0, source.IndexOf("이것이다") + 5);
                    variables.Add(new Variable("var", name, parse[1]));
                    if (!Need_Converted)
                    {
                        generated.Add(name + " = " + ParsedToCsharp(Operator.Parse(source)) + ";");
                    }
                    else
                    {
                        return name + " =  " + ParsedToCsharp(Operator.Parse(source)) + ";";
                    }
                }
                return "";
            }

            public void Exception_Try()
            {
                bracket++;
                generated.Add("try {");
            }

            public void Exception_Catch()
            {
                bracket++;
                generated.Add("catch(Exception e) {");
            }

            public void Assignment(string source)
            {
                List<Operator> parse = Operator.Parse(source.Remove(0, source.IndexOf("이것이다") + 5));
                string Converted = (string)VariableTable[source.Split(' ')[0]];
                generated.Add(Converted + " = ");
                Append_Gen(ParsedToCsharp(parse));
                Append_Gen(";");
            }

            public static string ParsedToCsharp(List<Operator> input)
            {
                Operator current = input.First();
                Operator parent = input.First();
                Stack parents = new Stack();
                parents.Push(current);
                int index = 0;
                int masterindex = 0;
                string temp_generate = "";
                bool Infunction = false;
                for (int i = 0; true; i++)
                {
                    if (!current.Compiled)
                    {
                        if (Infunction && temp_generate.Last() != '(' && temp_generate.Last() != ',')
                        {
                            temp_generate += ",";
                        }
                        switch (current.type)
                        {
                            case Operator.Type.Variable:
                                temp_generate += VariableTable[current.Contents.Trim()];
                                break;

                            case Operator.Type.Function:
                                if (!Infunction)
                                {
                                    temp_generate += FunctionTable[current.Contents.Trim()] + "(";
                                    Infunction = true;
                                }
                                break;

                            case Operator.Type.True:
                                temp_generate += "true";
                                break;

                            case Operator.Type.False:
                                temp_generate += "false";
                                break;
                            case Operator.Type.Class:
                                temp_generate += current.Contents+".";
                                break;
                            default:
                                temp_generate += current.Contents;
                                break;
                        }
                    }
                    current.Compiled = true;
                    if (Get_Remain(current) == -1)
                    {
                        current = parents.Pop() as Operator;
                    }
                    else
                    {
                        current = current.slave.ElementAt(Get_Remain(current));
                        continue;
                    }
                    if (index != current.slave.Count)
                    {
                        parents.Push(current);
                        parent = current;
                        current = current.slave.ElementAt(index);
                        index++;
                    }
                    else
                    {
                        if (Infunction)
                        {
                            temp_generate += ")";
                            Infunction = false;
                        }
                        index = 0;
                        masterindex++;
                        if (masterindex == input.Count)
                        {
                            break;
                        }
                        current = input.ElementAt(masterindex);
                        parents.Push(current);
                    }
                }
                return temp_generate;
            }

            private static int Get_Remain(Operator Parents)
            {
                List<Operator.Slave> slaves = Parents.slave;
                for (int i = 0; i < slaves.Count; i++)
                {
                    if (!slaves[i].Compiled)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public void Terminate()
            {
                generated.Add("Environment.Exit(0);");
            }

            public void OpenBracket()
            {
                bracket++;
                Append_Gen("{");
            }

            public void CloseBracket()
            {
                bracket--;
                generated.Add("}");
            }

            public void Req_GC()
            {
                generated.Add("GC.Collect();");
            }

            public void Console_Log(string source)
            {
                source = source.Remove(0, 6);
                source = source.Remove(source.Length - 2, 2);
                generated.Add("Console.WriteLine(");
                Append_Gen(ParsedToCsharp(Operator.Parse(source)));
                Append_Gen(");");
            }

            public void Append_Gen(string input)
            {
                int index = generated.Count - 1;
                generated[index] = generated[index] + input;
            }
        }

        public class Operator
        {
            public class Slave : Operator
            {
                public Slave(Type type, string contents) : base(type, contents)
                {
                }
            }

            public readonly Type type;
            public readonly string Contents;
            public bool Compiled = false;
            public List<Slave> slave = new List<Slave>(); //제일 먼저보이는 함수
            private Type function;
            private string temp;

            public Operator(Type type, string contents)
            {
                this.type = type;
                Contents = contents;
            }

            public static List<Operator> Parse(string source)
            {
                bool Instring = false;
                int depth = 0;
                bool startrec = true;
                int Funtionstart = FindfunctionStart(source, 0);
                int FunctionEnd = FindfunctionEnd(source, 0);
                int ClassDot = FindClassDot(source, 0);
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
                    if (i == ClassDot)
                    {
                        temp = temp.Trim().Replace(' ', '_');
                        if (depth == 0)
                        {
                            operators.Add(new Operator(Type.Class, temp));
                        }
                        else
                        {
                            AddParseList(ref temp, depth, ref operators, ParserParamater(temp), Type.Class);
                        }
                        temp = "";
                        ClassDot = FindClassDot(source, ClassDot);
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
                        temp += source[i];
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

            private static int FindClassDot(string source, int ignore)
            {
                bool Instring = false;
                for (int i = 0; i != source.Length; i++)
                {
                    if (source[i] == '"')
                    {
                        Instring = !Instring;
                        continue;
                    }
                    if (!Instring && source[i] == '.')
                    {
                        if (i > ignore)
                        {
                            if (!Regex.IsMatch(source[i - 1].ToString(), @"^\d+$"))
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
                List<Slave> slaves = new List<Slave>();
                if (operators.Count != 0 && depth != 0)
                {
                    slaves = operators.Last().slave;
                }

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
                        if (temp == "친박")
                        {
                            generators.Add(new Slave(Type.True, temp));
                            continue;
                        }
                        else if (temp == "비박")
                        {
                            generators.Add(new Slave(Type.False, temp));
                            continue;
                        }
                        else
                        {
                            generators.Add(new Slave(Type.Variable, temp));
                        }
                        continue;
                    }
                    if (startrec)
                    {
                        temp += paramaters[i];
                    }
                    if (!Instring && !temp.Trim().Equals(""))
                    {
                        if (temp == "친박")
                        {
                            generators.Add(new Slave(Type.True, temp));
                            continue;
                        }
                        else if (temp == "비박")
                        {
                            generators.Add(new Slave(Type.False, temp));
                            continue;
                        }

                        if (temp.Trim().Last() == '*')
                        {
                            generators.Add(new Slave(Type.Variable, temp.Trim().Remove(temp.Length - 1, 1).Trim()));
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

            public static string Parse_IF(string source)
            {
                string result = "";
                List<Conditional> operators = Conditional.FindOperator(source);
                foreach (Conditional part in operators)
                {
                    if (part.type == Conditional.ConditionalType.None)
                    {
                        result += Parser.ParsedToCsharp(Parse(part.Contents));
                    }
                    else
                    {
                        result += part.Contents;
                    }
                }

                return result;
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
                True,
                False,
                Paramater,
                Class,
            }
        }

        private class Conditional
        {
            public readonly ConditionalType type;
            public readonly string Contents;

            public Conditional(ConditionalType type, string contents)
            {
                this.type = type;
                Contents = contents;
            }

            public enum ConditionalType
            {
                None,
                And,
                Not,
                Equal,
                Or,
                Less,
                Greater,
                LessEqual,
                GreaterEqual,
            }

            public static List<Conditional> FindOperator(string source)
            {
                List<Conditional> Result = new List<Conditional>();
                bool Instring = false;
                bool Rec = true;
                string temp = "";
                string Operator = "";
                for (int i = 0; i != source.Length; i++)
                {
                    if (source[i] == '"')
                    {
                        Instring = !Instring;
                    }
                    if (!Instring) //문자열 밖에있음
                    {
                        switch (source[i])
                        {
                            case '=':
                                Rec = false;
                                Operator += "=";
                                continue;
                            case '!':
                                Rec = false;
                                Operator += "!";
                                continue;
                            case '>':
                                Rec = false;
                                Operator += ">";
                                continue;
                            case '<':
                                Rec = false;
                                Operator += "<";
                                continue;
                            case '&':
                                Rec = false;
                                Operator += "&";
                                continue;
                            case '|':
                                Rec = false;
                                Operator += "|";
                                continue;
                        }
                        if (Operator != "")
                        {
                            Result.Add(new Conditional(ConditionalType.None, temp.Trim()));
                            switch (Operator)
                            {
                                case "!=":
                                    Result.Add(new Conditional(ConditionalType.Not, "!="));
                                    break;

                                case "==":
                                    Result.Add(new Conditional(ConditionalType.Equal, "=="));
                                    break;

                                case ">":
                                    Result.Add(new Conditional(ConditionalType.Greater, ">"));
                                    break;

                                case "<":
                                    Result.Add(new Conditional(ConditionalType.Less, "<"));
                                    break;

                                case ">=":
                                    Result.Add(new Conditional(ConditionalType.GreaterEqual, ">"));
                                    break;

                                case "<=":
                                    Result.Add(new Conditional(ConditionalType.LessEqual, "<"));
                                    break;

                                case "&&":
                                    Result.Add(new Conditional(ConditionalType.LessEqual, "&&"));
                                    break;

                                case "||":
                                    Result.Add(new Conditional(ConditionalType.LessEqual, "||"));
                                    break;
                            }
                            temp = "";
                            Operator = "";
                            Rec = true;
                        }
                        if (i == source.Length - 1)
                        {
                            temp += source[i];
                            Result.Add(new Conditional(ConditionalType.None, temp.Trim()));
                        }
                        if (Rec)
                        {
                            temp += source[i];
                        }
                    }
                }

                return Result;
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
            public int location;

            public Function(string name, int location, List<Parameter> parameters)
            {
                this.name = name;
                this.parameters = parameters;
                this.location = location;
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

                            case "계파":
                                type = "bool";
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