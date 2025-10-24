using RT.Util.ExtensionMethods;

namespace Qoph
{
    static class SapphicSynonyms
    {
        public static void Generate()
        {
            var data = @"BIBLE/1/44,BABIES/4/7,INSANE/4/5,SEA/4/5,BALL/5/6,ALLIES/6/7".Split(',').Select(row => row.Split('/')).Select(arr => (word: arr[0], index: int.Parse(arr[1]), length: arr[2])).ToArray();
            General.ReplaceInFile(@"D:\c\Qoph\EnigmorionFiles\Puzzles\sapphic-synonyms.html",
                "<!--%%-->", "<!--%%%-->",
                data.Select(tup => $@"<div class='box'><div class='stripes'>{tup.word.Select(ch => $@"<div class='stripe c-{"LESBIAN".IndexOf(ch) + 1}'></div>").JoinString()}</div><div class='extra'>( {tup.length.Select(ch => $"<div class='stripe c-{ch}'></div>").JoinString(", ")} )</div><div class='extra'><div class='stripe c-{tup.index}'></div></div></div>").JoinString());
        }
    }
}