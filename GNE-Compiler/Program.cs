using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GNE_Compiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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

            Compiler.Parser parser = new Compiler.Parser();
            if (GetFileEncoding(args[0]) != Encoding.GetEncoding(949))
            {
                Console.WriteLine("EUC-KR만을 지원합니다. 유니코드는 한국의 기술이 아니며 국가경쟁력을 강화하지 못하니까요.");
            }

            var source = File.ReadAllLines(args[0], Encoding.GetEncoding(949));
            parser.Process_Raw_Code(source);
           // parser.MakeMain();
            parser.Compile();
            Console.WriteLine("계속 하려면 아무 키나 누르십시오. . . ");
            Console.ReadKey();
        }


        public static Encoding GetFileEncoding(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var encodings = Encoding.GetEncodings()
                .Select(e => e.GetEncoding())
                .Select(e => new { Encoding = e, Preamble = e.GetPreamble() })
                .Where(e => e.Preamble.Any())
                .ToArray();

            var maxPrembleLength = encodings.Max(e => e.Preamble.Length);
            byte[] buffer = new byte[maxPrembleLength];

            using (var stream = File.OpenRead(path))
            {
                stream.Read(buffer, 0, (int)Math.Min(maxPrembleLength, stream.Length));
            }

            return encodings
                .Where(enc => enc.Preamble.SequenceEqual(buffer.Take(enc.Preamble.Length)))
                .Select(enc => enc.Encoding)
                .FirstOrDefault() ?? Encoding.Default;
        }
    }
}