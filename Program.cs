using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GNE_Compiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
			if (args.Length < 1) return;
			  
            Compiler.Parser parser = new Compiler.Parser();
            if (GetFileEncoding(args[0]) != Encoding.GetEncoding(949))
            {
                Console.WriteLine("EUC-KR만을 지원합니다. 유니코드는 한국의 기술이 아니며 국가경쟁력을 강화하지 못하니까요.");
            }

            var source = File.ReadAllLines(args[0], Encoding.GetEncoding(949));
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
                    parser.Function(source[i], i);
                }
                else if (source[i].StartsWith("/ㄹ") || source[i].StartsWith("/근"))
                {
                    Console.WriteLine("주석 " + source[i]);
                }
                else if (source[i].StartsWith("공천"))
                {
                    parser.Variable(source[i]);
                }
                else if (source[i].StartsWith("코드 텅텅 빌때까지 한번 해 보세요"))
                {
                    parser.Exception_Try();
                }
                else if (source[i].StartsWith("예외처리 다 어디 갔냐고"))
                {
                    parser.Exception_Catch();
                }
                else if (source[i].StartsWith("메모리 텅텅 빌 때까지 한번 해 보세요 쓰레기들 다 어디 갔냐고"))
                {
                    parser.Req_GC();
                }
                else if (source[i].Contains("이것이다"))
                {
                    parser.Assignment(source[i]);
                }
                else if (source[i] == "고심 끝에 프로세스 해체;")
                {
                    parser.Terminate();
                }
                else if (source[i] == "{")
                {
                    parser.OpenBracket();
                }
                else if (source[i] == "}")
                {
                    parser.CloseBracket();
                }
                else if (source[i] == "게임")
                {
                    Console.WriteLine("이 언어로 게임을 만들 수 없습니다. 왜냐하면 게임은 마약이기 때문이죠.");
                }
                else if (source[i].StartsWith("콘솔.로그"))
                {
                    parser.Console_Log(source[i]);
                }
                else if (source[i].StartsWith("누설한다"))
                {
                    parser.Fucntion_Return(source[i]);
                }
                else
                {
                    Console.WriteLine("알수없는 토큰: " + source[i]);
                }
            }
            parser.Compile();
            Console.Read();
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