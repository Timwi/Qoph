using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.TagSoup;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff
{
    static class SecretSolver2019ForAuLeaf
    {
        public static void Meta()
        {
            //var names = "SIGMA,PHI,TENMYOUJI,QUARK,CLOVER,ALICE,LUNA".Split(',');
            //var possibleWords = "MARGINS,AMUSING,MAGPIES,MASKING,GRISHAM,AMIGAOS,MIRAGES;SHIPS,PITCH,CHIPS,HIPPO,WHIPS,CHIMP,APHID,PITHY;JOURNEYTIME,JURYMENTION,INMAYTOJUNE;UKIRAQI,QUACKDR,KUTIRAQ,KSQUARE,UKQURAN;VERYCOOL,VERYCOLD,LIVEROCK,CODLIVER,VICELORD,OVALRACE,COILOVER,FORCLIVE,COVERALL,COVERTLY;SPECIAL,MICHAEL,MEDICAL,CLAIMED,ARTICLE,INPLACE,CLIMATE,CHARLIE,GLACIER,DIALECT,ICELAND,CHILEAN,MIRACLE,ETHICAL,CITADEL,REPLICA,CALIBER,LATTICE,ELASTIC,DECIMAL,RECLAIM,RECITAL,LEXICAL,PELICAN,HELICAL,TACTILE,CHALICE,ANGELIC,CALCITE,BLACKIE,CALORIE,EXCLAIM;ANNUAL,UNABLE,LAUNCH,SULTAN,MANUAL,ALUMNI,JULIAN,LAUREN,NEURAL,WALNUT,VULCAN,NEBULA,FUNGAL,DUNLAP,UNREAL,UNLOAD"
            //    .Split(';').Select(entry => entry.Split(',')).ToArray();

            var names = new List<string>();
            var possibleWords = new List<List<string>>();
            var dividerFound = false;

            foreach (var line in File.ReadLines(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\Meta planning.txt"))
            {
                if (!dividerFound)
                {
                    if (line == "---")
                        dividerFound = true;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;
                if (line.StartsWith(" "))
                    possibleWords.Last().Add(line.Trim());
                else
                {
                    var p = line.IndexOf(' ');
                    names.Add(line.Substring(0, p).Trim());
                    possibleWords.Add(new List<string> { line.Substring(p).Trim() });
                }
            }

            static string remove(string word, string lettersToRemove)
            {
                var list = word.Where(ch => ch != ' ').ToList();
                foreach (var ch in lettersToRemove)
                    list.Remove(ch);
                return list.JoinString();
            }

            var nutrimaticQuery = $"<{Enumerable.Range(0, names.Count).Select(i => i < possibleWords.Count ? possibleWords[i].Select(word => remove(word, names[i]).ToLowerInvariant()).Distinct().JoinString("|") : "AA").Select(x => $"({x})").JoinString()}>";
            //Clipboard.SetText(nutrimaticQuery);

            // All names
            //var possibleSolutions = "THE BALTIMORE COUNTY,ANY OTHER UNIVERSITY,ADDITIONAL STRENGTH,UNIVERSITY DIRECTOR,PERMANENT CEASEFIRE,COUNTY FIRE DISTRICT,SECOND ANNUAL REPORT,ALREADY FUNCTIONING,AN ADDITIONAL THREAT,DIFFERENTIAL STRESS,UNIVERSITY REPORTER,POSED A DIRECT THREAT,PORTUGUESE DIRECTOR,ADVISER AND DIRECTOR,UNIVERSITY STANDING,FIRST INDIAN ACTRESS,YOUTH FUNDING REPORT,A SUBSTANTIAL REPORT,POSES A DIRECT THREAT,FILING A FALSE REPORT,MAKING A FALSE REPORT,RESPONSE TO DISTRESS,ADDITIONAL REGISTER,DISEASE CAN BE SPREAD,SUPPRESS THE RIOTING,UNOFFICIAL REPORTER,ADDITIONAL RESISTOR,COMING YOUNG ACTRESS,AN ADDITIONAL STREET".Split(',');

            // All names except K and DIO
            //var possibleSolutions = "IDENTITY REPORT,BRANCH IDENTITY,ANY OTHER ENTITY,UNITED REPORTER,POWERED PRESENT,READERS RUNNING,UNITS SURRENDER,READ STATISTICS,IT POSED A THREAT,ALERT CHRISTIAN".Split(',');

            // All names except K
            var possibleSolutions = "POWERED PASSENGER,THE BLESSED UNREST,UNITS SURRENDERED,ANTI GRAVITY DISCS,UNRELEASED REPORT,STANDARD IDENTITY,IDENTITY STRENGTH".Split(',');

            // All names except K and QUARK
            //var possibleSolutions = "REPORT RELEASED,IMPORTANT AREAS,LESS RESTRICTED,THREADS RUNNING,POSTER RELEASED,BEAUTY STRENGTH,STANDING ENMITY,STANDARD THREAD,RELEASED REPORT,ADDING STRENGTH,REBELS REPORTED,STANDING LEADER,IMPORTANT ARENA,STRESS AND ANGER,READ THIS LETTER,EDITOR AND PRESS,DIASPORA SPREAD,READERS PRAISED".Split(',');


            var pairsToNames = new Dictionary<string, HashSet<string>>();
            for (var i = 0; i < possibleWords.Count && i < names.Count; i++)
                foreach (var possibleWord in possibleWords[i])
                    pairsToNames.AddSafe(remove(possibleWord, names[i]), names[i]);

            var validSolutions = new List<string>();
            foreach (var solution in possibleSolutions)
            {
                Console.Clear();
                ConsoleUtil.WriteLine(solution.Color(ConsoleColor.White));
                Console.WriteLine();
                Console.WriteLine();
                var pairs = solution.Where(ch => ch != ' ').Split(2).Select(chunk => chunk.JoinString()).ToArray();

                IEnumerable<(ConsoleColoredString answers, string[] leftOverPairs)> recurse(string[] pairsLeftToAssign, IEnumerable<(string pair, string name)> answerSoFar, int nameIx)
                {
                    if (nameIx >= possibleWords.Count || nameIx >= names.Count)
                    {
                        yield return (answerSoFar.Select(answer =>
                        {
                            var (pair, name) = answer;
                            var nmIx = names.IndexOf(name);
                            var words = possibleWords[nmIx].Where(pw => remove(pw, name) == pair).Select(w => w.Color(ConsoleColor.Yellow)).JoinColoredString("/".Color(ConsoleColor.DarkYellow));
                            return new ConsoleColoredString($"{pair.Color(ConsoleColor.White)} + {name.Color(ConsoleColor.Green)} = {words}");
                        }).JoinColoredString("\n"), pairsLeftToAssign);
                        yield break;
                    }

                    for (var i = 0; i < pairsLeftToAssign.Length; i++)
                    {
                        var pair = pairsLeftToAssign[i];
                        if (!pairsToNames.ContainsKey(pair) || !pairsToNames[pair].Contains(names[nameIx]))
                            continue;
                        foreach (var solution in recurse(pairsLeftToAssign.Remove(i, 1), answerSoFar.Append((pair, names[nameIx])), nameIx + 1))
                            yield return solution;
                    }
                }

                var any = false;
                foreach (var (answers, leftOverPairs) in recurse(pairs, new (string pair, string name)[0], 0))
                {
                    any = true;
                    ConsoleUtil.WriteLine(answers);
                    if (leftOverPairs.Length > 0)
                    {
                        ConsoleUtil.WriteLine($"Leftover pairs: {leftOverPairs.Select(p => p.Color(ConsoleColor.Magenta)).JoinColoredString(", ".Color(ConsoleColor.DarkMagenta))}", null);
                        ConsoleUtil.WriteLine($"Leftover names: {names.Skip(possibleWords.Count).Select(n => n.Color(ConsoleColor.Cyan)).JoinColoredString(", ".Color(ConsoleColor.DarkCyan))}", null);
                    }
                    Console.WriteLine();
                }

                if (any)
                {
                    validSolutions.Add(solution);
                    Console.ReadLine();
                }
            }

            Console.Clear();
            Console.WriteLine(validSolutions.JoinString("\n"));
        }

        public static void RabbitHole()
        {
            var clues = Ut.NewArray<(string answer, string crypticClue)>(
                ("melting", "Thawing, extracting without entropy (7)"),
                ("ice", "It’s cold, everything starts to freeze (3)"),
                ("reveals", "Pry back as fifth and sixth exchange disclosures (7)"),
                ("final", "Lost soldier failing to scramble for exam (5)"),
                ("variable", "Resolve to be a rival for X, say (8)"));

            const string finalZipfile = "The Rabbit Hole.zip";
            if (File.Exists(finalZipfile))
                File.Delete(finalZipfile);

            Directory.SetCurrentDirectory(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf");
            File.WriteAllBytes("(7)", new byte[0]);
            string curFile = "(7)";

            for (var i = clues.Length - 1; i >= 0; i--)
            {
                if (File.Exists(clues[i].crypticClue))
                    File.Delete(clues[i].crypticClue);
                CommandRunner.RunRaw($@"7z a {((i == clues.Length - 1) ? "" : $@"""-p{clues[i + 1].answer}""")} tmp.zip ""{curFile}"" -tzip -mx9").Go();
                File.Move("tmp.zip", clues[i].crypticClue);
                File.Delete(curFile);
                curFile = clues[i].crypticClue;
            }

            CommandRunner.RunRaw($@"7z a ""-p{clues[0].answer}"" ""{finalZipfile}"" ""{curFile}"" -tzip -mx9").Go();
            File.Delete(curFile);

            Console.WriteLine(Convert.ToBase64String(File.ReadAllBytes(finalZipfile)));
        }

        public static void Elementary()
        {
            var elements = @"H,HE,LI,BE,B,C,N,O,F,NE,NA,MG,AL,SI,P,S,CL,AR,K,CA,SC,TI,V,CR,MN,FE,CO,NI,CU,ZN,GA,GE,AS,SE,BR,KR,RB,SR,Y,ZR,NB,MO,TC,RU,RH,PD,AG,CD,IN,SN,SB,TE,I,XE,CS,BA,LA,CE,PR,ND,PM,SM,EU,GD,TB,DY,HO,ER,TM,YB,LU,HF,TA,W,RE,OS,IR,PT,AU,HG,TL,PB,BI,PO,AT,RN,FR,RA,AC,TH,PA,U,NP,PU,AM,CM,BK,CF,ES,FM,MD,NO,LR,RF,DB,SG,BH,HS,MT,DS,RG,CN,NH,FL,MC,LV,TS,OG".Split(',');
            var data = @"
Ken Ribet         │ Fermat’s Last Theorem
Hannah Fry        │ Delicious Problems
Cliff Stoll       │ The Klein Bottle Guy
Simon Singh       │ The Math Storyteller
Matt Parker       │ Parker Square
David Eisenbud    │ A Proof in the Drawer
John Urschel      │ The Offensive Lineman
James Grime       │ The Singing Banana
Steven Strogatz   │ The C-Word
Neil Sloane       │ The Number Collector
Timothy Gowers    │ Fame and Admiration
James Maynard     │ The Badly Behaved Prime
"
                .Trim()
                .Replace("\r", "")
                .Split("\n")
                .Select(line => line.Split('│').Select(txt => txt.Trim()).ToArray())
                .Select(arr => (guest: arr[0], title: arr[1]))
                .ToArray();

            static IEnumerable<(string guest, string title, int index)[]> recurse((string guest, string title)[] data, IEnumerable<(string guest, string title, int index)> sofar, string solutionRest)
            {
                if (solutionRest.Length == 0)
                {
                    yield return sofar.ToArray();
                    yield break;
                }

                for (var i = 0; i < data.Length; i++)
                {
                    var (guest, title) = data[i];
                    guest = guest.ToUpperInvariant().Where(c => c != ' ').JoinString();

                    for (var p = 0; p < guest.Length; p++)
                    {
                        if (guest[p] != solutionRest[0] || sofar.Any(tup => tup.index == p))
                            continue;
                        foreach (var solution in recurse(data.Remove(i, 1), sofar.Append((guest, title, p)), solutionRest.Substring(1)))
                            yield return solution;
                    }
                }
            }

            var numSolutions = 0;
            foreach (var combination in recurse(data, Enumerable.Empty<(string guest, string title, int index)>(), "JUDOANYTIME"))
            {
                if (combination.All(tup => tup.index + 1 != 12))
                {
                    foreach (var (guest, title, index) in combination)
                        ConsoleUtil.WriteLine($"{elements[index].Color(ConsoleColor.White),-2} ({index + 1,2}): {guest.Color(ConsoleColor.DarkYellow).ColorSubstring(index, 1, ConsoleColor.Yellow, ConsoleColor.DarkBlue),-20} - {title.Color(ConsoleColor.DarkGreen)}", null);
                    Console.WriteLine();
                    Console.ReadLine();
                    numSolutions++;
                }
            }
            Console.WriteLine(numSolutions);
        }

        public static void Elementary_FindInsertions(string extraLetters)
        {
            var words = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Concat(File.ReadAllLines(@"D:\Daten\Wordlists\VeryCommonWords.txt")).ToHashSet();
            //var words = File.ReadAllLines(@"D:\Daten\Wordlists\VeryCommonWords.txt").ToHashSet();
            foreach (var word in words)
                for (var i = 0; i < word.Length; i++)
                {
                    var newWord = word.Insert(i, extraLetters);
                    if (words.Contains(newWord))
                        ConsoleUtil.WriteLine(newWord.Color(ConsoleColor.DarkCyan).ColorSubstring(i, extraLetters.Length, ConsoleColor.Yellow, ConsoleColor.DarkBlue));
                }
        }

        public static void ScrambleEd_FindWords(params string[] words)
        {
            var allWords = @"
                D:\Daten\Wordlists\English 60000.txt
                D:\Daten\Wordlists\English GRE word list.txt
                D:\Daten\Wordlists\English words.txt
                D:\Daten\Wordlists\english-frequency-raw.txt
                D:\Daten\Wordlists\peter_broda_wordlist_unscored.txt
                D:\Daten\Wordlists\VeryCommonWords.txt
            "
                .Trim()
                .Replace("\r", "")
                .Split("\n")
                .SelectMany(f => File.ReadLines(f.Trim()))
                .Distinct()
                .ToHashSet();

            var table = new TextTable { ColumnSpacing = 2 };
            var locker = new object();
            Enumerable.Range(0, words.Length).ParallelForEach(4, nameIx =>
            {
                var name = words[nameIx];
                var row = nameIx + 1;
                var bestMatches = allWords.Select(w => (word: w, dist: General.LevenshteinDistance(w, name))).OrderBy(inf => inf.dist).Take(5).ToArray();
                var nameRev = name.Reverse().JoinString();
                var bestMatchesRev = allWords.Select(w => (word: w, dist: General.LevenshteinDistance(w, nameRev))).OrderBy(inf => inf.dist).Take(5).ToArray();
                lock (locker)
                {
                    table.SetCell(0, row, name.Color(ConsoleColor.Yellow));
                    for (var i = 0; i < bestMatches.Length; i++)
                    {
                        table.SetCell(2 * i + 1, row, bestMatches[i].dist.ToString().Color(ConsoleColor.DarkGreen), alignment: HorizontalTextAlignment.Right);
                        table.SetCell(2 * i + 2, row, bestMatches[i].word.Color(ConsoleColor.Green));
                    }
                    for (var i = 0; i < bestMatchesRev.Length; i++)
                    {
                        table.SetCell(2 * i + 11, row, bestMatchesRev[i].dist.ToString().Color(ConsoleColor.DarkMagenta), alignment: HorizontalTextAlignment.Right);
                        table.SetCell(2 * i + 12, row, bestMatchesRev[i].word.Color(ConsoleColor.Magenta));
                    }
                    Console.WriteLine(name);
                }
            });
            Console.WriteLine();
            table.WriteToConsole();
        }

        private static char caesar(char letter, int offset) => (char) ((letter - 'A' + offset) % 26 + 'A');
        private static readonly string vowels = "AEIOU";

        public static void ScrambleEd_V1()
        {
            static string coastMangling(string str, int wordIx)
            {
                var vx = str.IndexOf(ch => vowels.Contains(ch));
                return (str.Substring(0, vx) + vowels[(vowels.IndexOf(str[vx]) + 1) % vowels.Length] + str.Substring(vx + 1, str.Length - vx - 2) + caesar(str[str.Length - 1], 2)).Reverse().JoinString();
            }
            static string cowboyMangling(string str, int wordIx)
            {
                if (wordIx % 2 == 0)
                    return str;
                var ch = str.ToCharArray();
                ch[0] = caesar(ch[0], 16);
                ch[3] = caesar(ch[3], 11);
                return ch.JoinString().Insert(5, "GU");
            }

            scrambleEdImpl(Ut.NewArray<(string clue, string clueAnswer, string intendedAnswer, Func<string, int, string> mangling)>(
                /* C */ ("WRESTLING MOVE IN WHICH OPPONENT IS THROWN TO THE GROUND BACK FIRST", "BODYSLAM", "ODYSLAB", (str, wordIx) => str.Remove(str.Length - 1, 1).Apply(s => s.Substring(1) + s[0])),
                /* A */ ("PROPOSE SOMETHING WORTH CONSIDERING", "SUGGEST", "KURZGESGT", (str, wordIx) => "K" + str.Substring(1).Apply(s => s.Length.Apply(l => s.Substring(0, l - 4) + s.Substring(l - 3, 2) + s[l - 4] + s[l - 1])).Insert(1, "RZ")),
                /* R */ ("COLLECTED TRADITIONAL LEGENDS OF AN ETHNICITY", "MYTHOLOGY", "MATHOLOGE", (str, wordIx) => str.Length == 2 ? $"{str[0]}{caesar(str[1], 6)}" : $"{str[0]}{caesar(str[1], 2)}{str.Substring(2, str.Length - 3)}{caesar(str[str.Length - 1], 6)}"),
                /* N */ ("HUGH WILSON MOVIE POLICE BLANK", "ACADEMY", "KHAACADEMY", (str, wordIx) => str[0].Apply(letter => $"{caesar(letter, 10)}{caesar(letter, 7)}{letter}{str}")),
                /* O */ ("INTERROGATIVE PRONOUN ASKING ABOUT ALTERNATIVES", "WHICH", "SCISHW", (str, wordIx) => (str.Remove(str.Length - 1, 1) + "S").Insert(2, "S").Reverse().JoinString()),
                /* V */ ("CARICATURE OR PARODY USING IRONY OR SARCASM", "SATIRE", "ERITASIUM", (str, wordIx) => ("MUI" + str).Reverse().JoinString()),
                /* E */ ("BOUNDARY BETWEEN LAND AND SEA FIVE LETTERS", "COAST", "VSAUC", coastMangling),
                /* L */ ("WESTERN STARRING JOHN WAYNE AND CHILD ACTOR STEPHEN HUDIS", "THE COWBOYS", "THESOWMOGUYS", cowboyMangling)
            ));
        }

        public static void ScrambleEd_V2()
        {
            scrambleEdImpl(Ut.NewArray<(string clue, string clueAnswer, string intendedAnswer, Func<string, int, string> mangling)>(
                /* D */ ("BABY FLOWER OR KERNEL OR SPORE", "SEED", "TEED", (str, wordIx) => $"{caesar(str[0], 1)}{str.Substring(1)}"),
                /* I */ ("VISUALIZATION OF DATA SUCH AS A GRAPH OR TABLE", "CHART", "VHART", (str, wordIx) => $"{caesar(str[0], 'V' - 'C')}{str.Substring(1)}"),
                /* N */ ("HUGH WILSON MOVIE POLICE BLANK", "ACADEMY", "KHAACADEMY", (str, wordIx) => str[0].Apply(letter => $"{caesar(letter, 10)}{caesar(letter, 7)}{letter}{str}")),
                /* G */ ("ANIMAL HUNTED BY A PREDATOR", "PREY", "CGPREY", (str, wordIx) => $"CG{str}"),
                /* O */ ("INTERROGATIVE PRONOUN ASKING ABOUT ALTERNATIVES", "WHICH", "SCISHW", (str, wordIx) => (str.Remove(str.Length - 1, 1) + "S").Insert(2, "S").Reverse().JoinString())
            ));
        }

        public static void ScrambleEd_V3()
        {
            scrambleEdImpl(Ut.NewArray<(string clue, string clueAnswer, string intendedAnswer, Func<string, int, string> mangling)>(
                /* S */ ("KIDNEY SHAPED NUT FROM EVERGREEN TREE ANACARDIUM OCCIDENTALE", "CASHEW", "CISHOW", (str, ix) => str.Select(ch => vowels.IndexOf(ch).Apply(p => p == -1 ? ch : vowels[(p + 2) % vowels.Length])).JoinString()),
                /* H */ ("COUNTRY CONTAINING ONTARIO BESIDE MANITOBA", "CANADA", "KANACADEMY", (str, ix) => $"K{str[1]}{str[2]}{str[3]}{str[0]}{str[5]}{str[4]}{str.Substring(6)}EMY"),
                /* I */ ("RANKING OF MOST POPULAR MUSIC TRACKS OR DATA VISUALIZATION", "CHART", "VHART", (str, wordIx) => $"{caesar(str[0], 'V' - 'C')}{str.Substring(1)}"),
                /* P */ ("FAMOUS STANDUP COMEDIAN AND ACTOR DREW", "CAREY", "CGGREY", (str, ix) => str.Replace("A", "GG")),
                /* S */ ("HAVING INTENSELY PRESSING IMPORTANCE", "URGENT", "KURZGEAGT", (str, ix) => $"K{str.Insert(2, "Z").Replace("N", "AG")}")
            ));
        }

        private static void scrambleEdImpl((string clue, string clueAnswer, string intendedAnswer, Func<string, int, string> mangling)[] data)
        {
            var tt = new TextTable { ColumnSpacing = 2 };
            for (var i = 0; i < data.Length; i++)
            {
                var (clue, clueAnswer, intendedAnswer, mangling) = data[i];
                string mangle(string str) => str.Split(' ').Select((s, i) => mangling(s, i)).JoinString();

                string mangledClue = null;
                try
                {
                    mangledClue = mangle(clue);
                }
                catch
                {
                }
                tt.SetCell(0, i, mangledClue != null ? mangledClue.Color(ConsoleColor.Yellow) : "XXX".Color(ConsoleColor.Magenta, ConsoleColor.DarkRed));
                tt.SetCell(1, i, mangle(clueAnswer).Color(ConsoleColor.Cyan));
                tt.SetCell(2, i, intendedAnswer.Color(mangle(clueAnswer) == intendedAnswer ? ConsoleColor.Green : ConsoleColor.Red));
            }
            tt.WriteToConsole();
        }

        private static string minifyCss(string css) => Regex.Replace(Regex.Replace(Regex.Replace(css.Trim(), @"\s*\}\s*", "}", RegexOptions.Singleline), @"\s*\{\s*", "{", RegexOptions.Singleline), @"\s+", " ");
        private static string minifyJs(string js) => Regex.Replace(js.Trim(), @"\s\s+", " ", RegexOptions.Singleline);

        static Tag page(string title, params object[] content) => new HTML(
        new HEAD(
            new META { httpEquiv = "Content-Type", content = "text/html; charset=utf-8" },
            new TITLE(title),
            new STYLELiteral(minifyCss($@"
@font-face {{
  font-family: WSA;
  font-style: normal;
  font-weight: normal;
  src: url(data:font/otf;base64,{Convert.ToBase64String(File.ReadAllBytes(@"D:\Daten\Fonts\WorkSans-Regular.otf"))});
}}
@font-face {{
  font-family: WSA;
  font-style: normal;
  font-weight: bold;
  src: url(data:font/otf;base64,{Convert.ToBase64String(File.ReadAllBytes(@"D:\Daten\Fonts\WorkSans-Bold.otf"))});
}}
")),
            new STYLELiteral(Regex.Replace(Regex.Replace(File.ReadAllText(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\Hunt.css"),
                @"\s*([\{\}:;])\s*", m => m.Groups[1].Value, RegexOptions.Singleline),
                @"\s+", " "))
        ),
        new BODY(content));

        public static void GenerateHtml()
        {
            var html = page("Secret Solver Puzzle Hunt for AuLeaf",
                new DIV { id = "all" }._(

                    new H1("Secret Solver"),
                    new DIV { class_ = "puzzle", id = "meta" }.Data("solution", "STRESSANDANGER")._(
                        new P("Flexibly misidentifying pairs, Christine fights dogs nervously."),
                        new P("What did this cause her as she couldn’t run away?"),
                        new DIV { class_ = "solution" }._(
                            new P("The words in the strange sentence match up with the solution words, giving an order. Furthermore, the phrase “couldn’t run away” hints at {0}. All of the feeder answers are characters from {0} with two extra letters:".FmtEnumerable(new CITE("Zero Escape"))),
                            new TABLE(
                                new TR(new TH("Word"), new TH("Feeder answer"), new TH(new CITE("Zero Escape"), " character"), new TH("Extra letters")),
                                @"
Flexibly       | ELASTIC      | ALICE     | ST
misidentifying | MIRAGES      | SIGMA     | RE
pairs          | SHIPS        | PHI       | SS
Christine      | CAR NOVEL    | CLOVER    | AN
fights         | JUDO ANYTIME | TENMYOUJI | DA
dogs           | DINGO        | DIO       | NG
nervously      | NEURAL       | LUNA      | ER
                                "
                                    .Trim().Replace("\r", "").Split('\n').Select(row => new TR(row.Split('|').Select(str => new TD(str.Trim()))))
                            ),
                            new P { class_ = "answer" }._("STRESS AND ANGER"))),

                    new H2("CYBORG"),
                    new DIV { class_ = "puzzle" }.Data("solution", "NEURAL")._(
                        new IMG { src = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\CYBORG.png"))}", id = "cyborg-puzzle" },
                        new DIV { class_ = "solution" }._(
                            new P("There are 6 groups of items. Each group is a set of 6, but only 5 are shown in the collage. Furthermore, there are 6 unique colors with one color missing in each group. The puzzle title indicates how to sort the missing items according to their missing colors."),
                            new TABLE { class_ = "cyborg" }._(
                                new TR(new TH("Cyan"), new TH("Yellow"), new TH("Blue"), new TH("Orange"), new TH("Red"), new TH("Green"), new TH("Group")),
                                @"
!NICKEL | PENNY;character from {0}     | DIME;basketball lifestyle magazine | QUARTER                          | HALF-DOLLAR       | DOLLAR;Nelson Dollar, US-NC politician | US coins
 ( )    |!EXPONENTIATION               | ×                                  | ÷                                | +                 | − | Order of operations
 DOWN;physician John Langdon Down   | STRANGE;Dr. Strange          |!UP                                 | CHARM                            | TOP;spinning top  | BOTTOM;British sitcom | Types of quarks
 PAWN;actress Doris Pawn   | BISHOP                       | KNIGHT                             |!ROOK                             | KING;Stephen King | QUEEN;the band Queen | Chess pieces
 BOLEYN;coat of arms | SEYMOUR;actress Jane Seymour | CLEVES;aerial shot of Cleves, Ohio | HOWARD;logo of Howard University |!ARAGON            | PARR;familiy from {1} | Wives of Henry VIII
 DAGGER | REVOLVER                     | SPANNER                            | ROPE                             | CANDLESTICK       |!LEAD PIPE | Weapons in {2}
"
                                    .Trim().Replace("\r", "").Split('\n').Select(row => new TR(
                                        row.Split('|').Select(str => new TD { class_ = str[0] == '!' ? "s" : null }._(
                                            str.Substring(1).Trim()
                                                .Apply(s => s.Contains(';') ? s.Split(';') : new[] { s, null })
                                                .Apply(s => new[] { new CITE("The Big Bang Theory"), new CITE("The Incredibles"), new CITE("Cluedo") }
                                                    .Apply(inserts => new object[] { s[0].FmtEnumerable(inserts), s[1]?.Apply(t => new SMALL(t.FmtEnumerable(inserts))) }))
                                        ))
                                    ))),
                            new P { class_ = "answer" }._("NEURAL"))),

                    new H2("Elementary"),
                    new DIV { class_ = "puzzle" }.Data("solution", "JUDOANYTIME")._(
                        new P { class_ = "flavour-text" }._("Welcome to our podcast. All of our guests are ready. What do you want to do, and when?"),
                        new UL(
                            new LI("A musical yellow threat."),
                            new LI("This athelete gives it their ball."),
                            new LI("Organizing all the papers in my desk is a chorale."),
                            new LI("A four-dimensional load."),
                            new LI("A misdemeanor that we may have to headdress."),
                            new LI("Saying this on TV may be against the claw."),
                            new LI("Questions that have a taste rafting."),
                            new LI("A quadrilateral of some sorbet."),
                            new LI("I listen to his tales when I go to bend."),
                            new LI("The result of hard work and dedication, though it may gnaw at your salinity."),
                            new LI("Compared to what this guy’s got, my shelf of figures is paneling.")),
                        new DIV { class_ = "solution" }._(
                            new P("The phrases refer to episodes of {0} and the flavor text suggests that we should look at the names of the guests on each episode. The last word in each phrase has a chemical element symbol inserted into it. Use their atomic numbers to index into the guests’ names."
                                .FmtEnumerable(new CITE("The Numberphile Podcast"))),
                            new TABLE(
                                new TR(new TH("Episode"), new TH("Guest"), new TH("Word"), new TH("Index"), new TH("Letter")),
                                @"
The Singing Banana      | JAMES GRIME     | t(H)reat    |  1 | J
The Offensive Lineman   | JOHN URSCHEL    | (B)all      |  5 | U
A Proof in the Drawer   | DAVID EISENBUD  | chor(Al)e   | 13 | D
The Klein Bottle Guy    | CLIFF STOLL     | l(O)ad      |  8 | O
The Badly Behaved Prime | JAMES MAYNARD   | (He)address |  2 | A
The C-Word              | STEVEN STROGATZ | (C)law      |  6 | N
Delicious Problems      | HANNAH FRY      | ra(F)ting   |  9 | Y
Parker Square           | MATT PARKER     | sor(Be)t    |  4 | T
The Math Storyteller    | SIMON SINGH     | be(N)d      |  7 | I
Fame and Admiration     | TIMOTHY GOWERS  | sa(Li)nity  |  3 | M
The Number Collector    | NEIL SLOANE     | pa(Ne)ling  | 10 | E
"
                                    .Trim().Replace("\r", "").Split('\n').Select(row => new TR(row.Split('|').Select(str => new TD(str.Trim()))))),
                            new P { class_ = "answer" }._("JUDO ANYTIME"))),

                    new H2("False Friends"),
                    new DIV { class_ = "puzzle" }.Data("solution", "MIRAGES")._(
                        new UL(
                            new LI("Agreements with people in your phone?"),
                            new LI("A more aspirated easy puzzle?"),
                            new LI("Destroys the proteins of false teeth?"),
                            new LI("Frivolously satirical and insubordinate?"),
                            new LI("In a shocked manner wide open?"),
                            new LI("Leaves Arab governments?"),
                            new LI("Water tanks for February people?")),
                        new DIV { class_ = "solution" }._(
                            new P("Each phrase refers to two words that differ in just one letter. The words are always 9 and 8 letters long."),
                            new TABLE(
                                new TR(new TH("9-letter word"), new TH("8-letter word"), new TH("Letter")),
                                new TR(new TD("CONTRACTS"), new TD("CONTACTS"), new TD("R")),
                                new TR(new TD("BREATHIER"), new TD("BREATHER"), new TD("I")),
                                new TR(new TD("DENATURES"), new TD("DENTURES"), new TD("A")),
                                new TR(new TD("FACETIOUS"), new TD("FACTIOUS"), new TD("E")),
                                new TR(new TD("GASPINGLY"), new TD("GAPINGLY"), new TD("S")),
                                new TR(new TD("EMIGRATES"), new TD("EMIRATES"), new TD("G")),
                                new TR(new TD("AQUARIUMS"), new TD("AQUARIUS"), new TD("M"))
                            ),
                            new P("Since these words start with A–G, the answer consists of the letters in that order."),
                            new P { class_ = "answer" }._("MIRAGES"))),

                    new H2("THE RADIO"),
                    new DIV { class_ = "puzzle" }.Data("solution", "ELASTIC")._(
                        new PRE(@"Determine what vowels occur in the first lines and
you’ll notice something that seeing folk don’t usually know.
Then obtain a coded word from the signs at the ends.
Now repeat that first task, but with another character this stage
that’s kind of a scarcer vowel which shall be extracted via lines four up
to and including six and after that the next two trios also con-
tain interesting stuff with this coding but yet different vowels.
Now I’ll start talking about signal flags used by ships —
they have a lot of varying colors and shapes indeed.
A blue block in the midst of white; horizontal bars of blue over crimson
red; a red cross that’s diagonal over a white background; a pointed flag comes next
and this flag’s colored body is blue left, white right; then a checkerboard, blue and white."),
                        new DIV { class_ = "solution" }._(
                            new P("The text suggests to look only at the vowels contained in this text, and to highlight specific vowels in each triplet of lines to obtain words written in Braille. Which vowels to look for is given by the title of the puzzle."),
                            new P("Furthermore, after the first triplet, a word is obtained from the punctuation marks at the ends of the lines, which translate as Morse code. Finally, the text describes a word expressed in international maritime signaling flags."),
                            new PRE(@"    ##.#..#...#..#.
E   ....#.#..##.......          MELTING
    #.....#..#..##

Morse code:                     ICE

    ...##.#..#..##..#.
A   #..##....#..#..#.....       ENGULFS
    .#....##.#.....#..

    .##..#.##.#..#....
I   .#..#...#....#              FINAL
    .......#.....#..

    ...#...#..#.#..#..#..#
O   ...#...#.#.....#..#...#..   VARIABLE
    ...##.#...........#......

Maritime flags:                 SEVEN
"),
                            new P("The answer is obtained by solving this cryptic clue: Melting ice (ICE anagram) engulfs (surrounds) final (LAST) variable (def) (7)."),
                            new P { class_ = "answer" }._("ELASTIC"))),

                    new H2("Notebook"),
                    new DIV { class_ = "puzzle" }.Data("solution", "CARNOVEL")._(
                        new RawTag(File.ReadAllText(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\Notebook-min.svg")),
                        new DIV { class_ = "solution" }._(
                            new P("The music depicted consists of only 5 unique notes (C, E, G, C′, E′). When plotting which notes are active at which times, the solution appears in picture form. Below, the first staff is shown in blue, the second in orange."),
                            new RawTag(@"<svg viewBox='0 0 40 5'><path fill='#0060ff' d='M16 0v1h1V0h-1zm4 0v3h1V0h-1zm7 0v1h1V0h-1zm8 0v1h1V0h-1zm2 0v4h1V0h-1zM0 1v3h1V1H0zm4 0v4h1V1H4zm3 0v4h1V1H7zm2 0v1h1V1H9zm3 0v1h1V1h-1zm10 0v3h1V1h-1zm3 0v3h1V1h-1zm6 0v1h1V1h-1zm2 0v1h1V1h-1zM16 2v3h1V2h-1zm12 0v1h1V2h-1zM9 3v2h1V3H9zm2 0v1h1V3h-1zm1 1v1h1V4h-1zm18-1v1h1V3h-1zm0 1h-1v1h1V4zm3-1v1h1V3h-1zM20 4v1h1V4h-1zm15 0v1h1V4h-1z'/><path fill='#ff6000' d='M1 0v1h2V0H1zm4 0v1h2V0H5zm4 0v1h3V0H9zm14 0v1h2V0h-2zm8 0v1h1V0h-1zm2 0v1h2V0h-2zM16 1v1h2V1h-2zm2 1v1h1V2h-1zm1 1v1h2V3h-2zm8-2v1h1V1h-1zM5 2v1h2V2H5zm4 0v1h3V2H9zm21 0v1h1V2h-1zm3 0v1h2V2h-2zm-5 1v1h1V3h-1zM1 4v1h2V4H1zm22 0v1h2V4h-2zm10 0v1h2V4h-2zm4 0v1h3V4h-3z'/></svg>"),
                            new P { class_ = "answer" }._("CAR NOVEL"))),

                    new H2("Scramble-Ed"),
                    new DIV { class_ = "puzzle" }.Data("solution", "SHIPS")._(
                        new UL(
                            new LI("KUDNOYSHIPODNETFRAMOVORGROONTROOINICIRDUEMACCUDONTILO"),
                            new LI("KOUNCRTYEMYKONTCIANINGEMYKNTAOIROEMYKESIBEDEMYKANIMOTBAEMY"),
                            new LI("KANKINGHFFOSTIOPULARFUSICMRACKSHRWATAOISUALIZATION"),
                            new LI("FGGMOUSSTGGNDUPCOMEDIGGNGGNDGGCTORDREW"),
                            new LI("KHAZVIAGGKIAGZTEAGSELYKPRZESSIAGGKIMZPORTAAGCE")),
                        new DIV { class_ = "solution" }._(
                            new P("Each line is a clue in which every word is consistently mangled. After solving the clues and applying the same mangling to the answer, the names of educational YouTube channels appear, each with one letter missing."),
                            new TABLE(
                                new TR(new TH("Clue"), new TH("Mangling"), new TH("Answer"), new TH("Mangled"), new TH("Letter")),
                                new TR(new TD("Kidney-shaped nut from evergreen tree anacardium occidentale"), new TD("Vowels are cycled forward 2"), new TD("CASHEW"), new TD("CISHOW"), new TD("S")),
                                new TR(new TD("Country containing Ontario beside Manitoba"), new TD("Letters 2–6 consistently rearranged; K+...+EMY"), new TD("CANADA"), new TD("KANACADEMY"), new TD("H")),
                                new TR(new TD("Ranking of most popular music tracks or data visualization"), new TD("First letter shifted forward 19"), new TD("CHART"), new TD("VHART"), new TD("I")),
                                new TR(new TD("Famous stand-up comedian and actor Drew"), new TD("A replaced with GG"), new TD("CAREY"), new TD("CGGREY"), new TD("P")),
                                new TR(new TD("Having intensely pressing importance"), new TD("Z inserted after 2nd letter; N replaced with AG; K+..."), new TD("URGENT"), new TD("KURZGEAGT"), new TD("S"))),
                            new P { class_ = "answer" }._("SHIPS"))),

                    new H2("Slitherword"),
                    new DIV { class_ = "puzzle" }.Data("solution", "DINGO")._(
                        new RawTag(Regex.Replace(Regex.Replace(Regex.Replace(File.ReadAllText(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\Slitherword.html"),
                            @"^.*?<body>(.*)</body>.*", m => m.Groups[1].Value, RegexOptions.Singleline),
                            @"\s+</", "</", RegexOptions.Singleline),
                            @">\s+", ">", RegexOptions.Singleline)),
                        new DIV { class_ = "solution" }._(
                            new P("The grid contains several words or phrases in a winding fashion that can be grouped into categories as indicated by the boxes. The number in parentheses shows the number of bends of each word or phrase."),
                            new RawTag(@"<div class='categories'><div><div class='category'><div class='title' style='background: linear-gradient(90deg, hsl(0, 80%, 95%), hsl(0, 80%, 85%))'>Agatha Christie</div><div class='words'><div><div class='word'><span class='word'>NEMESIS</span><span class='info'>(2)</span></div></div><div><div class='word'><span class='word'>N OR M</span><span class='info'>(0)</span></div></div><div><div class='word'><span class='word'>TOWARDS ZERO</span><span class='info'>(4)</span></div></div></div></div></div><div><div class='category'><div class='title' style='background: linear-gradient(90deg, hsl(60, 80%, 95%), hsl(60, 80%, 85%))'>Alice in Wonderland</div><div class='words'><div><div class='word'><span class='word'>DORMOUSE</span><span class='info'>(1)</span></div></div><div><div class='word'><span class='word'>JABBERWOCK</span><span class='info'>(2)</span></div></div><div><div class='word'><span class='word'>LION</span><span class='info'>(0)</span></div></div><div><div class='word'><span class='word'>UNICORN</span><span class='info'>(2)</span></div></div></div></div></div><div><div class='category'><div class='title' style='background: linear-gradient(90deg, hsl(120, 80%, 95%), hsl(120, 80%, 85%))'>Countries</div><div class='words'><div><div class='word'><span class='word'>COSTA RICA</span><span class='info'>(4)</span></div></div><div><div class='word'><span class='word'>GUATEMALA</span><span class='info'>(2)</span></div></div><div><div class='word'><span class='word'>ICELAND</span><span class='info'>(3)</span></div></div><div><div class='word'><span class='word'>NICARAGUA</span><span class='info'>(4)</span></div></div></div></div></div><div><div class='category'><div class='title' style='background: linear-gradient(90deg, hsl(180, 80%, 95%), hsl(180, 80%, 85%))'>Danganronpa</div><div class='words'><div><div class='word'><span class='word'>MISAKI ASANO</span><span class='info'>(5)</span></div></div><div><div class='word'><span class='word'>USAMI</span><span class='info'>(1)</span></div></div><div><div class='word'><span class='word'>WARRIORS OF HOPE</span><span class='info'>(4)</span></div></div></div></div></div><div><div class='category'><div class='title' style='background: linear-gradient(90deg, hsl(240, 80%, 95%), hsl(240, 80%, 85%))'>David Willis</div><div class='words'><div><div class='word'><span class='word'>DUMBING OF AGE</span><span class='info'>(5)</span></div></div><div><div class='word'><span class='word'>ROOMIES</span><span class='info'>(2)</span></div></div><div><div class='word'><span class='word'>SHORTPACKED</span><span class='info'>(3)</span></div></div></div></div></div><div><div class='category'><div class='title' style='background: linear-gradient(90deg, hsl(300, 80%, 95%), hsl(300, 80%, 85%))'>Weird Al Yankovic</div><div class='words'><div><div class='word'><span class='word'>AMISH PARADISE</span><span class='info'>(2)</span></div></div><div><div class='word'><span class='word'>DON’T WEAR THOSE SHOES</span><span class='info'>(11)</span></div></div><div><div class='word'><span class='word'>HOMER AND MARGE</span><span class='info'>(9)</span></div></div><div><div class='word'><span class='word'>JURASSIC PARK</span><span class='info'>(3)</span></div></div></div></div></div></div>"),
                            new P("There are five unused letters in the grid which form the solution."),
                            new P { class_ = "answer" }._("DINGO"))),

                    new SCRIPTLiteral(minifyJs(File.ReadAllText(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\Hunt.js"))),

                    new DIV { class_ = "thanks" }._("Thank you for solving!")));

            File.WriteAllText(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\Hunt.html", html.ToString());
        }

        public static void GenerateHtmlWriteup()
        {
            File.WriteAllText(@"D:\Daten\Puzzles\Secret Solver 2019\AuLeaf\Writeup.html", page("Writeup — Secret Solver hunt for AuLeaf",
                new DIV { id = "all", class_ = "writeup" }._(
                    new H1("Secret Solver for AuLeaf — write-up"),
                    new DIV { class_ = "puzzle", id = "meta" }._(
                        new P("Creating this little hunt for AuLeaf for the Puzzlers Club Secret Santa event was a huge amount of fun. Thank you for the opportunity! I hope that the puzzles were well-received."),
                        new P("I was quite surprised to find out that 24 days was way enough to write a small hunt like this and I was not pressed for time at all. Since I had time to spare, I added full solution explanations to the original hunt itself (as well as a spoiler-free answer checker). In hindsight I don’t know if I should have avoided this, as it may create temptation in solvers. Anyway, I won’t repeat the solutions here. Check out the original hunt to find them."),
                        new P("Also, can I take this opportunity to vent my frustration with puzzles as PDFs? I hate them. You can’t copy and paste stuff properly, especially tables, and the user experience is just overall sucky. Puzzles should come in HTML. Thank you. Rant over.")),

                    new H2("Meta"),
                    new DIV { class_ = "puzzle" }._(
                        new P("The intention here was to incorporate AuLeaf’s interest in the ", new CITE("Zero Escape"), " series of video games. I wanted a puzzle where AuLeaf would need to identify the answers as being anagrams of names of characters, each with extra letters."),
                        new P("Using Nutrimatic, I collected a large number of possible candidate words for each character name (SIGMA, PHI, TENMYOUJI, QUARK, CLOVER, ALICE, LUNA, DIO) along with the pairs of inserted letters. Then I constructed a huge Nutrimatic query to find a word or phrase consisting of those letter pairs in some order. This way I basically generated tens of possible metas."),
                        new P("There were several options that had a final solution word that I might have preferred (e.g. THE BALTIMORE COUNTY, PERMANENT CEASEFIRE, etc.), but most of them required feeder answers that were just atrocious (e.g. QUARK ⇒ K\u00a0SQUARE or TENMYOUJI ⇒ IN\u00a0MAY\u00a0TO\u00a0JUNE) so I eventually settled on the current structure as the overall least sucky option."),
                        new P("Since the extraction requires the feeder answers to be in a specific order, I constructed the nonsense phrase to provide the ordering but also to “set the scene” for the final answer.")),

                    new H2("CYBORG"),
                    new DIV { class_ = "puzzle" }._(
                        new P("I’m generally quite fond of puzzles that use groups of X elements with one element missing. I’m not, however, fond of puzzles that require lots of image googling. I could have constructed another puzzle with clues or letter jumbles, but I decided the other puzzles were already like that and I wanted more variety."),
                        new P("The puzzle was originally going to use the colors RGBCMY and it was going to be called “Creamy Beige”, but my playtester came up with the idea of using orange instead so I could construct the word CYBORG from all of the colors. Thanks!")),

                    new H2("Elementary"),
                    new DIV { class_ = "puzzle" }._(
                        new P("This was originally going to be called “", new CITE("Elementary, My Dear Solver"), "”, but I changed this when making the HTML page because this long title didn’t fit and using a smaller font size than all the other puzzles would have looked out of place."),
                        new P("This puzzle is partly inspired by ", new A { href = "https://puzzlepotluck.com/2/8" }._("Office"), " from Puzzle Potluck 2 and ", new A { href = "https://www.mumspuzzlehunt.com/puzzles/V.1%20No.%20102.pdf" }._("No. 102"), " from MUMS 2019."),
                        new P("I found it very difficult, even with the help of an algorithm, to find suitable words that can be extended to other words with the addition of specific letters, which is why many of these clues sound weird. I wonder how the author of Puzzle Potluck 2’s Office did it."),
                        new P("This was the first puzzle that meaningfully incorporates one of AuLeaf’s interests that they listed in the spreadsheet. I found the Wikipedia page about the podcast and the list of episode titles and guests was just too alluring to pass up!")),

                    new H2("False Friends"),
                    new DIV { class_ = "puzzle" }._(
                        new P("This type of puzzle is pretty common. I was running out of ideas and this was one of the last puzzles I made. In hindsight, I regret having restricted myself to words of a specific length because that forced me to use more obscure words than would otherwise have been necessary. I initially thought having consistent word lengths would make it easier to find the harder ones, but this was more than offset by having significantly harder words...")),

                    new H2("THE RADIO"),
                    new DIV { class_ = "puzzle" }._(
                        new P("I’m very pleased with how this puzzle turned out, even if the solver won’t really fully appreciate how the wording is meticulously crafted to have all the right vowels in the right places. Not inspired by any prior puzzle I’ve seen, I consider this fully my own idea."),
                        new P("I was initially going to involve more letter encodings (Pigpen cipher, Semaphores, whatever) but I couldn’t find a good way to work them in, but in hindsight I think the puzzle works very well the way it is."),
                        new P("The idea of using the title to provide the order of the vowels was of course directly derived from CYBORG above. This was also where I had the idea that puzzle titles that matter for solving the puzzle should be all uppercase, while the remaining puzzles are in small-caps. Just a subtle thing that most readers won’t notice."),
                        new P("The very first puzzle I created for this hunt turned out terrible and I discarded it, but the cryptic clue used to clue the final solution word is a remnant of it.")),

                    new H2("Notebook"),
                    new DIV { class_ = "puzzle" }._(
                        new P("Very vaguely inspired by MUMS’s ", new A { href = "https://www.mumspuzzlehunt.com/puzzles/III.2%20A%20Noteworthy%20Puzzle.pdf" }._("A Noteworthy Puzzle"), " but only insofar as that I wanted my own puzzle with musical notation. I wish I had thought up something more elaborate, but I think the puzzle is reasonably challenging already.")),

                    new H2("Scramble-Ed"),
                    new DIV { class_ = "puzzle" }._(
                        new P("Another attempt at incorporating one of AuLeaf’s listed interests (educational YouTube channels), but otherwise this is a straight-up remake of a common puzzle type that I really enjoy solving myself. I first encountered this in Smogon 2 (", new A { href = "http://spo.ink/2cl3_scrambleforthestars" }._("Scramble for the Stars"), "), so consider this an homage to that.")),

                    new H2("Slitherword"),
                    new DIV { class_ = "puzzle" }.Data("solution", "DINGO")._(
                        new P("This puzzle is a remake of an earlier puzzle of my own, ", new A { href = "https://ktane.timwi.de/puzzles/2019-Timwi/Nibbles.html" }._("Nibbles"), ", that I created for the KTANE Puzzle Hustle. I wrote a complete software for constructing this type of puzzle and it was delightful to be able to re-use it! I enjoyed trying to fit words/categories into the grid that belong to AuLeaf’s stated interests."),
                        new P("In hindsight I think I should have made the grid bigger so that each category could have more elements in it. It’s not like I was pressed for time making this. But neither my testsolver nor the recipient complained, so maybe it wasn’t an issue.")),

                    new DIV { class_ = "thanks" }._("Thank you for reading!")
                )).ToString());
        }
    }
}