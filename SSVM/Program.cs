using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSVM
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"
   _____ _______      ____  __ 
  / ____/ ____\ \    / /  \/  |
 | (___| (___  \ \  / /| \  / |
  \___ \\___ \  \ \/ / | |\/| |
  ____) |___) |  \  /  | |  | |
 |_____/_____/    \/   |_|  |_|
");
            if (args.Length < 1)
            {
                Console.WriteLine("입력된 개,돼지가 없습니다.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("입력한 개,돼지가 존재하지 않습니다.");
                return;
            }

            string[] REFERENCES = { "System.Dynamic.Runtime.dll", "Microsoft.CSharp.dll", "System.Core.dll" };
            CompilerResults result = Compile(args[0], REFERENCES);
            if (result.Errors.Count > 0)
            {
                foreach (CompilerError CompErr in result.Errors)
                {
                    Console.WriteLine("C# Line number " + (CompErr.Line - 2) +
                        ", Error Number: " + CompErr.ErrorNumber +
                        ", '" + CompErr.ErrorText);
                    Console.WriteLine("그네어 Line number " + (CompErr.Line - 1));
                }
                Console.Read();
                return;
            }
            
            object instant = result.CompiledAssembly.CreateInstance("CSharp_Script_Namespace.Program");
            MethodInfo func_main =  instant.GetType().GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic |
                              BindingFlags.Static | BindingFlags.Instance);
            try
            {
                func_main.Invoke(instant, new object[] { args });
            }
            catch (Exception e)
            {
                if(e.InnerException is NullReferenceException)
                {
                    Console.WriteLine("안보 경고:나쁜 변수이더라");
                }
                else
                {
                    Console.WriteLine("안보 경고:제가 뭐라고 했습니까?");
                }
            }

            Console.ReadLine();
        }

        private static CompilerResults Compile(string source, params string[] references)
        {
            CompilerParameters parameters = new CompilerParameters(references);
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true; 

            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                return provider.CompileAssemblyFromFile(parameters, source);
            }
        }
    }
}
