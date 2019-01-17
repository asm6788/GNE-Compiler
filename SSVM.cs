using System;

namespace GNE_Compiler
{
    internal class SSVM
    {
        private SSVM()
        {
            if (Math.Ceiling(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (Double)1073741824) < 486)
            {
                Console.WriteLine("하드웨어 경고: 램 없으면 니네 부모를 원망해. 램도 실력이야. 있는 우리 컴퓨터 가지고 감 놔라 배 놔라 하지 말고. 남의 욕하기 바쁘니 아무리 다른 것 한들 어디 성공하겠니? 모자란 애들 상대하기 더러워서 안 하는 거야");
            }
        }
    }
}