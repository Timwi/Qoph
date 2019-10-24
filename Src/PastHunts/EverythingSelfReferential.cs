using System;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleStuff
{
    static class EverythingSelfReferential
    {
        public static void Do()
        {
            var input = @"1	0	2	7	0	0	0	6	0	0	0	0	10,11	0	0	0	0	8	4	3,5	9	0	0	0	12	0
6	0	7,8	0	2,9	0	0	0	0	0	0	0	0	0	4	0	0	5	1,10,11	0	0	0	3	0	0	0
11	0	0	0	6,13	0	9	2	3,4	0	0	7,14	0	0	0	0	0	10	1,8	0	0	5,12	0	0	15	0
11,15	0	9	0	0	0	0	0	2,5,16	0	4	14	12	3,6,10,17	8	13	0	0	1	7	0	0	0	0	0	0
14	9	0	0	0	13	0	0	6,10	0	0	5,15,16	0	0	2	7,11	0	0	4,8,12,17	1,3	0	0	0	0	0	0
0	0	5	6	0	0	10	11	4	0	0	0	0	0	2	0	0	7,8	0	1,12	9	0	0	3	0	0
2	1	0	5	4	0	0	0	0	0	0	0	7	0	6	0	0	3	0	0	0	0	0	0	0	0
4	0	10	0	0	0	0	3	6	0	11	0	8	2,5	0	0	0	0	0	0	1,9	0	0	0	7	0
0	0	0	6	10	1	0	0	0	0	0	5	0	0	0	9	0	7	0	0	2,8	0	0	0	0	3,4
8	10	0	0	0	0	0	13	3	0	0	1,6	9	0	0	0	0	2	12	0	11	0	0	0	7	4,5
6,8,10	0	7	0	11,17	0	0	16	4	0	5	0	0	0	3,14	15	0	2,13	0	1,9,12	0	0	0	0	0	0".Replace("\r", "").Split('\n').Select(row => row.Split('\t').Select(cl => cl.Split(',').Select(int.Parse).ToArray()).ToArray()).ToArray();

            for (var row = 0; row < input.Length; row++)
            {
                var word = new char[input[row].Max(cel => cel.Max())];
                for (int i = 0; i < 26; i++)
                {
                    foreach (var number in input[row][i])
                        if (number != 0)
                            word[number - 1] = (char) ('A' + i);
                }
                Console.WriteLine($"{row + 1} = {word.JoinString().Replace('\0', '_')}");
            }
        }
    }
}