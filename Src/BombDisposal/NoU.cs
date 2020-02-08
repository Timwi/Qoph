using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace PuzzleStuff.BombDisposal
{
    static class NoU
    {
        public static void FindPairs()
        {
            var lines = @"WARSCHAUERSTRAßE,SCHLESISCHESTOR,GÖRLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRAßE,HALLESCHESTOR,MÖCKERNBRÜCKE,GLEISDREIECK,KURFÜRSTENSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,KURFÜRSTENDAMM,UHLANDSTRAßE
PANKOW,VINETASTRAßE,SCHÖNHAUSERALLEE,EBERSWALDERSTRAßE,SENEFELDERPLATZ,ROSALUXEMBURGPLATZ,ALEXANDERPLATZ,KLOSTERSTRAßE,MÄRKISCHESMUSEUM,SPITTELMARKT,HAUSVOGTEIPLATZ,STADTMITTE,MOHRENSTRAßE,POTSDAMERPLATZ,MENDELSSOHNBARTHOLDYPARK,GLEISDREIECK,BÜLOWSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,ZOOLOGISCHERGARTEN,ERNSTREUTERPLATZ,DEUTSCHEOPER,BISMARCKSTRAßE,SOPHIECHARLOTTEPLATZ,KAISERDAMM,THEODORHEUSSPLATZ,NEUWESTEND,OLYMPIASTADION,RUHLEBEN
WARSCHAUERSTRAßE,SCHLESISCHESTOR,GÖRLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRAßE,HALLESCHESTOR,MÖCKERNBRÜCKE,GLEISDREIECK,KURFÜRSTENSTRAßE,NOLLENDORFPLATZ,WITTENBERGPLATZ,AUGSBURGERSTRAßE,SPICHERNSTRAßE,HOHENZOLLERNPLATZ,FEHRBELLINERPLATZ,HEIDELBERGERPLATZ,RÜDESHEIMERPLATZ,BREITENBACHPLATZ,PODBIELSKIALLEE,DAHLEMDORF,FREIEUNIVERSITÄT,OSKARHELENEHEIM,ONKELTOMSHÜTTE,KRUMMELANKE
NOLLENDORFPLATZ,VIKTORIALUISEPLATZ,BAYERISCHERPLATZ,RATHAUSSCHÖNEBERG,INNSBRUCKERPLATZ
BERLINHAUPTBAHNHOF,BUNDESTAG,BRANDENBURGERTOR,UNTERDENLINDEN,FRANZÖSISCHESTRAßE,MUSEUMSINSEL,BERLINERRATHAUS,ALEXANDERPLATZ,SCHILLINGSTRAßE,STRAUSBERGERPLATZ,WEBERWIESE,FRANKFURTERTOR,SAMARITERSTRAßE,FRANKFURTERALLEE,MAGDALENENSTRAßE,LICHTENBERG,FRIEDRICHSFELDE,TIERPARK,BIESDORFSÜD,ELSTERWERDAERPLATZ,WUHLETAL,KAULSDORFNORD,KIENBERG,COTTBUSSERPLATZ,HELLERSDORF,LOUISLEWINSTRAßE,HÖNOW
ALTTEGEL,BORSIGWERKE,HOLZHAUSERSTRAßE,OTISSTRAßE,SCHARNWEBERSTRAßE,KURTSCHUMACHERPLATZ,AFRIKANISCHESTRAßE,REHBERGE,SEESTRAßE,LEOPOLDPLATZ,WEDDING,REINICKENDORFERSTRAßE,SCHWARTZKOPFFSTRAßE,NATURKUNDEMUSEUM,ORANIENBURGERTOR,FRIEDRICHSTRAßE,UNTERDENLINDEN,FRANZÖSISCHESTRAßE,STADTMITTE,KOCHSTRAßE,HALLESCHESTOR,LANDWEHRCANAL,MEHRINGDAMM,PLATZDERLUFTBRÜCKE,PARADESTRAßE,TEMPELHOF,ALTTEMPELHOF,KAISERINAUGUSTASTRAßE,ULLSTEINSTRAßE,WESTPHALWEG,ALTMARIENDORF
RATHAUSSPANDAU,ALTSTADTSPANDAU,ZITADELLE,HASELHORST,PAULSTERNSTRAßE,ROHRDAMM,SIEMENSDAMM,HALEMWEG,JAKOBKAISERPLATZ,JUNGFERNHEIDE,MIERENDORFFPLATZ,RICHARDWAGNERPLATZ,BISMARCKSTRAßE,WILMERSDORFERSTRAßE,ADENAUERPLATZ,KONSTANZERSTRAßE,FEHRBELLINERPLATZ,BLISSESTRAßE,BERLINERSTRAßE,BAYERISCHERPLATZ,EISENACHERSTRAßE,KLEISTPARK,YORCKSTRAßE,MÖCKERNBRÜCKE,MEHRINGDAMM,GNEISENAUSTRAßE,SÜDSTERN,HERMANNPLATZ,RATHAUSNEUKÖLLN,KARLMARXSTRAßE,NEUKÖLLN,GRENZALLEE,BLASCHKOALLEE,PARCHIMERALLEE,BRITZSÜD,JOHANNISTHALERCHAUSSEE,LIPSCHITZALLEE,WUTZKYALLEE,ZWICKAUERDAMM,RUDOW
WITTENAU,RATHAUSREINICKENDORF,KARLBONHOEFFERNERVENKLINIK,LINDAUERALLEE,PARACELSUSBAD,RESIDENZSTRAßE,FRANZNEUMANNPLATZ,OSLOERSTRAßE,PANKSTRAßE,GESUNDBRUNNEN,VOLTASTRAßE,BERNAUERSTRAßE,ROSENTHALERPLATZ,WEINMEISTERSTRAßE,ALEXANDERPLATZ,JANNOWITZBRÜCKE,HEINRICHHEINESTRAßE,MORITZPLATZ,KOTTBUSSERTOR,SCHÖNLEINSTRAßE,HERMANNPLATZ,BODDINSTRAßE,LEINESTRAßE,HERMANNSTRAßE
OSLOERSTRAßE,NAUENERPLATZ,LEOPOLDPLATZ,AMRUMERSTRAßE,WESTHAFEN,BIRKENSTRAßE,TURMSTRAßE,HANSAPLATZ,ZOOLOGISCHERGARTEN,KURFÜRSTENDAMM,SPICHERNSTRAßE,GÜNTZELSTRAßE,BERLINERSTRAßE,BUNDESPLATZ,FRIEDRICHWILHELMPLATZ,WALTHERSCHREIBERPLATZ,SCHLOßSTRAßE,RATHAUSSTEGLITZ"
                .Replace("\r", "").Split('\n')
                .Zip(@"WARSCHAUERSTRASSE,SCHLESISCHESTOR,GOERLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRASSE,HALLESCHESTOR,MOECKERNBRUECKE,GLEISDREIECK,KURFUERSTENSTRASSE,NOLLENDORFPLATZ,WITTENBERGPLATZ,KURFUERSTENDAMM,UHLANDSTRASSE
PANKOW,VINETASTRASSE,SCHOENHAUSERALLEE,EBERSWALDERSTRASSE,SENEFELDERPLATZ,ROSALUXEMBURGPLATZ,ALEXANDERPLATZ,KLOSTERSTRASSE,MAERKISCHESMUSEUM,SPITTELMARKT,HAUSVOGTEIPLATZ,STADTMITTE,MOHRENSTRASSE,POTSDAMERPLATZ,MENDELSSOHNBARTHOLDYPARK,GLEISDREIECK,BUELOWSTRASSE,NOLLENDORFPLATZ,WITTENBERGPLATZ,ZOOLOGISCHERGARTEN,ERNSTREUTERPLATZ,DEUTSCHEOPER,BISMARCKSTRASSE,SOPHIECHARLOTTEPLATZ,KAISERDAMM,THEODORHEUSSPLATZ,NEUWESTEND,OLYMPIASTADION,RUHLEBEN
WARSCHAUERSTRASSE,SCHLESISCHESTOR,GOERLITZERBAHNHOF,KOTTBUSSERTOR,PRINZENSTRASSE,HALLESCHESTOR,MOECKERNBRUECKE,GLEISDREIECK,KURFUERSTENSTRASSE,NOLLENDORFPLATZ,WITTENBERGPLATZ,AUGSBURGERSTRASSE,SPICHERNSTRASSE,HOHENZOLLERNPLATZ,FEHRBELLINERPLATZ,HEIDELBERGERPLATZ,RUEDESHEIMERPLATZ,BREITENBACHPLATZ,PODBIELSKIALLEE,DAHLEMDORF,FREIEUNIVERSITAET,OSKARHELENEHEIM,ONKELTOMSHUETTE,KRUMMELANKE
NOLLENDORFPLATZ,VIKTORIALUISEPLATZ,BAYERISCHERPLATZ,RATHAUSSCHOENEBERG,INNSBRUCKERPLATZ
BERLINHAUPTBAHNHOF,BUNDESTAG,BRANDENBURGERTOR,UNTERDENLINDEN,FRANZOESISCHESTRASSE,MUSEUMSINSEL,BERLINERRATHAUS,ALEXANDERPLATZ,SCHILLINGSTRASSE,STRAUSBERGERPLATZ,WEBERWIESE,FRANKFURTERTOR,SAMARITERSTRASSE,FRANKFURTERALLEE,MAGDALENENSTRASSE,LICHTENBERG,FRIEDRICHSFELDE,TIERPARK,BIESDORFSUED,ELSTERWERDAERPLATZ,WUHLETAL,KAULSDORFNORD,KIENBERG,COTTBUSSERPLATZ,HELLERSDORF,LOUISLEWINSTRASSE,HOENOW
ALTTEGEL,BORSIGWERKE,HOLZHAUSERSTRASSE,OTISSTRASSE,SCHARNWEBERSTRASSE,KURTSCHUMACHERPLATZ,AFRIKANISCHESTRASSE,REHBERGE,SEESTRASSE,LEOPOLDPLATZ,WEDDING,REINICKENDORFERSTRASSE,SCHWARTZKOPFFSTRASSE,NATURKUNDEMUSEUM,ORANIENBURGERTOR,FRIEDRICHSTRASSE,UNTERDENLINDEN,FRANZOESISCHESTRASSE,STADTMITTE,KOCHSTRASSE,HALLESCHESTOR,LANDWEHRCANAL,MEHRINGDAMM,PLATZDERLUFTBRUECKE,PARADESTRASSE,TEMPELHOF,ALTTEMPELHOF,KAISERINAUGUSTASTRASSE,ULLSTEINSTRASSE,WESTPHALWEG,ALTMARIENDORF
RATHAUSSPANDAU,ALTSTADTSPANDAU,ZITADELLE,HASELHORST,PAULSTERNSTRASSE,ROHRDAMM,SIEMENSDAMM,HALEMWEG,JAKOBKAISERPLATZ,JUNGFERNHEIDE,MIERENDORFFPLATZ,RICHARDWAGNERPLATZ,BISMARCKSTRASSE,WILMERSDORFERSTRASSE,ADENAUERPLATZ,KONSTANZERSTRASSE,FEHRBELLINERPLATZ,BLISSESTRASSE,BERLINERSTRASSE,BAYERISCHERPLATZ,EISENACHERSTRASSE,KLEISTPARK,YORCKSTRASSE,MOECKERNBRUECKE,MEHRINGDAMM,GNEISENAUSTRASSE,SUEDSTERN,HERMANNPLATZ,RATHAUSNEUKOELLN,KARLMARXSTRASSE,NEUKOELLN,GRENZALLEE,BLASCHKOALLEE,PARCHIMERALLEE,BRITZSUED,JOHANNISTHALERCHAUSSEE,LIPSCHITZALLEE,WUTZKYALLEE,ZWICKAUERDAMM,RUDOW
WITTENAU,RATHAUSREINICKENDORF,KARLBONHOEFFERNERVENKLINIK,LINDAUERALLEE,PARACELSUSBAD,RESIDENZSTRASSE,FRANZNEUMANNPLATZ,OSLOERSTRASSE,PANKSTRASSE,GESUNDBRUNNEN,VOLTASTRASSE,BERNAUERSTRASSE,ROSENTHALERPLATZ,WEINMEISTERSTRASSE,ALEXANDERPLATZ,JANNOWITZBRUECKE,HEINRICHHEINESTRASSE,MORITZPLATZ,KOTTBUSSERTOR,SCHOENLEINSTRASSE,HERMANNPLATZ,BODDINSTRASSE,LEINESTRASSE,HERMANNSTRASSE
OSLOERSTRASSE,NAUENERPLATZ,LEOPOLDPLATZ,AMRUMERSTRASSE,WESTHAFEN,BIRKENSTRASSE,TURMSTRASSE,HANSAPLATZ,ZOOLOGISCHERGARTEN,KURFUERSTENDAMM,SPICHERNSTRASSE,GUENTZELSTRASSE,BERLINERSTRASSE,BUNDESPLATZ,FRIEDRICHWILHELMPLATZ,WALTHERSCHREIBERPLATZ,SCHLOSSSTRASSE,RATHAUSSTEGLITZ"
                    .Replace("\r", "").Split('\n'),
                    (line1, line2) => line1.Split(',').Zip(line2.Split(','), (stop1, stop2) => (de: stop1, en: stop2)).ToArray()).ToArray();

            var solution = @"TOPOLOGY";

            // Collect all the pairs of stations that surround a station
            var allPairs = new List<(string stop1, string stop2, string actualStop)>();
            for (int line = 0; line < lines.Length; line++)
                for (int stopIx = 1; stopIx < lines[line].Length - 1; stopIx++)
                    allPairs.Add((lines[line][stopIx - 1].en, lines[line][stopIx + 1].en, lines[line][stopIx].de));
            allPairs.Sort(CustomComparer<(string stop1, string stop2, string actualStop)>.By(p => p.stop1.Length + p.stop2.Length));

            // Find out which stations can be used for each solution letter
            var usablePairs = new List<int>[solution.Length];
            for (int solIx = 0; solIx < solution.Length; solIx++)
            {
                usablePairs[solIx] = new List<int>();
                for (int pairIx = 0; pairIx < allPairs.Count; pairIx++)
                {
                    const int maxStopLength = 15;
                    var (stop1, stop2, actualStop) = allPairs[pairIx];
                    var p = actualStop.IndexOf(solution[solIx]);
                    var inv = actualStop.IndexOf(ch => ch < 'A' || ch > 'Z');
                    if (p >= 0 && p < lines.Length && (inv < 0 || p < inv))
                        if (stop1.Length < maxStopLength && stop2.Length < maxStopLength)
                            usablePairs[solIx].Add(pairIx);
                }
            }

            // Find a set of matching pairs
            IEnumerable<(string stop1, string stop2, string actualStop, int stopIx, int solIx)[]> recurse((string stop1, string stop2, string actualStop, int stopIx, int solIx)[] sofar, int[] lengthsUnaccountedFor, bool[] solutionLettersUsed, int[] pairsUsed)
            {
                if (lengthsUnaccountedFor.Length == 0 && solutionLettersUsed.All(b => b))
                {
                    yield return sofar.ToArray();
                    yield break;
                }

                if (lengthsUnaccountedFor.Length > 0)
                {
                    var len = lengthsUnaccountedFor[0];
                    for (int i = 0; i < solution.Length; i++)
                        if (!solutionLettersUsed[i])
                            for (var j = 0; j < usablePairs[i].Count; j++)
                            {
                                if (pairsUsed.Contains(usablePairs[i][j]))
                                    continue;
                                var (stop1, stop2, actualStop) = allPairs[usablePairs[i][j]];
                                var len1 = stop1.Length;
                                var len2 = stop2.Length;
                                if ((len1 != len && len2 != len) || len1 == len2)
                                    continue;
                                if (sofar.Any(sf => sf.stop1 == stop1 || sf.stop2 == stop1 || sf.stop1 == stop2 || sf.stop2 == stop2))
                                    continue;

                                for (int asIx = 0; asIx < actualStop.Length && asIx < lines.Length; asIx++)
                                    if (actualStop[asIx] == solution[i] && !sofar.Any(sf => sf.stopIx == asIx))
                                    {
                                        var otherLen = len1 == len ? len2 : len1;
                                        var u = lengthsUnaccountedFor.IndexOf(otherLen);
                                        var newSoFar = sofar.Insert(sofar.Length, (stop1, stop2, actualStop, asIx, i));
                                        var newLengthsUnaccountedFor = u == -1
                                            ? lengthsUnaccountedFor.Insert(lengthsUnaccountedFor.Length, otherLen).Remove(0, 1)
                                            : lengthsUnaccountedFor.Remove(u, 1).Remove(0, 1);
                                        solutionLettersUsed[i] = true;
                                        foreach (var result in recurse(newSoFar, newLengthsUnaccountedFor, solutionLettersUsed, pairsUsed.Insert(0, usablePairs[i][j])))
                                            yield return result;
                                        solutionLettersUsed[i] = false;
                                    }
                            }
                }
                else
                {
                    var i = solutionLettersUsed.IndexOf(b => !b);
                    for (var j = 0; j < usablePairs[i].Count; j++)
                    {
                        if (pairsUsed.Contains(usablePairs[i][j]))
                            continue;
                        var (stop1, stop2, actualStop) = allPairs[usablePairs[i][j]];
                        if (sofar.Any(sf => sf.stop1 == stop1 || sf.stop2 == stop1 || sf.stop1 == stop2 || sf.stop2 == stop2))
                            continue;

                        for (int asIx = 0; asIx < actualStop.Length && asIx < lines.Length; asIx++)
                            if (actualStop[asIx] == solution[i] && !sofar.Any(sf => sf.stopIx == asIx))
                            {
                                var newLengthsUnaccountedFor = stop1.Length == stop2.Length
                                ? lengthsUnaccountedFor
                                : new[] { stop1.Length, stop2.Length };
                                solutionLettersUsed[i] = true;
                                foreach (var result in recurse(sofar.Insert(sofar.Length, (stop1, stop2, actualStop, asIx, i)), newLengthsUnaccountedFor, solutionLettersUsed, pairsUsed.Insert(0, usablePairs[i][j])))
                                    yield return result;
                                solutionLettersUsed[i] = false;
                            }
                    }
                }
            }

            var minTotalLength = int.MaxValue;
            foreach (var result in recurse(new (string stop1, string stop2, string actualStop, int stopIx, int solIx)[0], new int[0], new bool[solution.Length], new int[0]))
            {
                var totalLength = result.Sum(r => r.stop1.Length + r.stop2.Length);
                if (totalLength < minTotalLength && result.All(tup => tup.stopIx != 2))
                {
                    minTotalLength = totalLength;
                    ConsoleUtil.WriteLine($"{totalLength / 2}".Color(ConsoleColor.White));

                    var tt = new TextTable { ColumnSpacing = 2 };
                    var order = Enumerable.Range(0, result.Length).OrderBy(i => result[i].solIx).ToArray();
                    for (int i = 0; i < result.Length; i++)
                    {
                        var (stop1, stop2, actualStop, stopIx, solIx) = result[order[i]];
                        tt.SetCell(0, i, stop1.Length.ToString(), alignment: HorizontalTextAlignment.Right);
                        tt.SetCell(1, i, stop1.Color((ConsoleColor) (stop1.Length % 15 + 1)), alignment: HorizontalTextAlignment.Left);
                        tt.SetCell(2, i, stop2.Length.ToString(), alignment: HorizontalTextAlignment.Right);
                        tt.SetCell(3, i, stop2.Color((ConsoleColor) (stop2.Length % 15 + 1)), alignment: HorizontalTextAlignment.Left);
                        tt.SetCell(4, i, (stopIx + 1).ToString().Color(stopIx == 8 ? ConsoleColor.White : ConsoleColor.DarkGray), alignment: HorizontalTextAlignment.Right);
                        tt.SetCell(5, i, actualStop.Insert(stopIx + 1, "]").Insert(stopIx, "[").ColorSubstring(stopIx, 3, ConsoleColor.White, ConsoleColor.DarkBlue), alignment: HorizontalTextAlignment.Left);
                    }
                    tt.WriteToConsole();
                    Console.WriteLine();
                }
            }
        }

        public static void GenerateWords_OLD_VERSION()
        {
            // Generates sets of words where the first letter spells out one station, and another station is shuffled with an A-Z indexer column

            var word1 = "WESTHAFEN";
            var word2 = "NEUKOELLN";
            var inputLen = word1.Length;
            if (word2.Length != inputLen)
                Debugger.Break();

            static BigInteger b(int n) => ~(BigInteger.MinusOne << n);

            //*
            var allWords = File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist.txt")
                .Select(row => row.Split(';'))
                .Select(row => (word: row[0].Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: int.Parse(row[1])))
                .Where(w => w.score >= 75)
                .Concat(File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Select((w, wIx) => (word: w.Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: 60024 - wIx)))
                .ToArray();
            /*/
            var allWords = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Select(w => (word: w.Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: 100)).ToArray();
            /**/

            const int maxCol = 5;
            var pairs = Enumerable.Range(1, maxCol)
                .SelectMany(az => Enumerable.Range(1, maxCol).Where(w2 => w2 != az).Select(w2 => (azIx: az, w2Ix: w2)))
                .ToArray().Shuffle();

            foreach (var (azIx, w2Ix) in new[] { (3, 4) })
            {
                IEnumerable<BigInteger> recurse(BigInteger available, BigInteger used, int ix)
                {
                    if (ix == inputLen)
                    {
                        yield return used;
                        yield break;
                    }

                    var row = (available >> (inputLen * ix)) & ~(BigInteger.MinusOne << inputLen);
                    if (row.IsZero)
                        yield break;
                    var availableWithoutRow = available & ~((~(BigInteger.MinusOne << inputLen)) << (inputLen * ix));
                    if (ix < inputLen - 1 && availableWithoutRow.IsZero)
                        yield break;
                    var colMask = ~((~(BigInteger.MinusOne << (inputLen * inputLen))) / (~(BigInteger.MinusOne << inputLen)));
                    for (int i = 0; i < inputLen; i++)
                    {
                        if (!row.IsEven)
                            foreach (var solution in recurse(availableWithoutRow & (colMask << i), used | (BigInteger.One << (inputLen * ix + i)), ix + 1))
                                yield return solution;
                        row >>= 1;
                    }
                }

                var translations = Enumerable.Range(0, inputLen).ToArray().Shuffle();
                var translationsRev = new int[inputLen];
                for (int i = 0; i < inputLen; i++)
                    translationsRev[translations[i]] = i;

                var combinationsPossible = BigInteger.Zero;
                for (int w = 0; w < allWords.Length; w++)
                {
                    var (word, score) = allWords[w];
                    if (word.Length <= azIx || word.Length <= w2Ix || word[azIx] - 'A' >= inputLen || word2[word[azIx] - 'A'] != word[w2Ix])
                        continue;
                    for (int w1x = 0; w1x < word1.Length; w1x++)
                        if (word1[w1x] == word[0])
                            combinationsPossible |= BigInteger.One << (w1x + inputLen * translations[word[azIx] - 'A']);
                }

                var bestScore = 0;
                if (Enumerable.Range(0, inputLen).All(row => !(combinationsPossible & (b(inputLen) << (inputLen * row))).IsZero) &&
                    Enumerable.Range(0, inputLen).All(col => !(combinationsPossible & ((b(inputLen * inputLen) / b(inputLen)) << col)).IsZero))
                {
                    foreach (var solution in recurse(combinationsPossible, BigInteger.Zero, 0))
                    {
                        (string word, int score) getWord(int w1x, int w2x) => allWords.Where(aw => aw.word.Length > azIx && aw.word.Length > w2Ix && aw.word[0] == word1[w1x] && aw.word[azIx] - 'A' == w2x && aw.word[w2Ix] == word2[w2x]).MaxElement(w => w.score);

                        var words = Enumerable.Range(0, inputLen * inputLen)
                            .Where(i => !(solution >> i).IsEven)
                            .OrderBy(i => i % inputLen)
                            .Select(i => getWord(i % inputLen, translationsRev[i / inputLen]).Apply(tup => (ix: i, tup.word, tup.score)))
                            .ToArray();
                        var score = words.Sum(w => w.score);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            ConsoleUtil.WriteLine($"azIx={azIx}, w2Ix={w2Ix}, score={score.ToString().Color(ConsoleColor.Yellow)}", null);
                            ConsoleUtil.WriteLine(words.Select(w => new ConsoleColoredString($@"{w.score.ToString().PadLeft(5)} {w.word
                                    .ColorSubstring(0, 1, ConsoleColor.White, ConsoleColor.DarkBlue)
                                    .ColorSubstring(azIx, 1, ConsoleColor.White, ConsoleColor.DarkGreen)
                                    .ColorSubstring(w2Ix, 1, ConsoleColor.White, ConsoleColor.DarkRed)}"))
                                .JoinColoredString("\n"));
                            Console.WriteLine();
                            //goto next;
                        }
                    }
                }
            }
        }

        public static void GenerateWords_3D_OBSOLETE()
        {
            // Generates sets of words where the first letter spells out one station, and two other stations are shuffled, each with an A-Z indexer column
            // Turns out this doesn’t find any matches for anything

            //*
            var word1 = "WUHLETAL";
            var word2 = "KIENBERG";
            var word3 = "RUHLEBEN";
            /*/
            // simple test case that suggests that the algorithm does work
            var word1 = "SGD";
            var word2 = "MUE";
            var word3 = "RSY";
            /**/
            var inputLen = word1.Length;

            //var targetLen = 7;
            for (var targetLen = 5; targetLen < 50; targetLen++)
            {
                ConsoleUtil.WriteLine($"TARGET LENGTH: {targetLen}".Color(ConsoleColor.White));

                if (word2.Length != inputLen || word2.Length != inputLen)
                    Debugger.Break();
                var allWords = File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt")
                    .Concat(File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist.txt").Select(row => row.Split(';')).Where(row => int.Parse(row[1]) >= 50).Select(row => row[0]))
                    .Select(w => w.Where(ch => ch >= 'A' && ch <= 'Z').JoinString())
                    .Where(w => w.Length == targetLen)
                    .ToArray();

                var quadruplets = (
                    from az2Ix in Enumerable.Range(1, targetLen - 1)
                    from az3Ix in Enumerable.Range(1, targetLen - 1)
                    where az3Ix != az2Ix
                    from w2Ix in Enumerable.Range(1, targetLen - 1)
                    where w2Ix != az2Ix && w2Ix != az3Ix
                    from w3Ix in Enumerable.Range(1, targetLen - 1)
                    where w3Ix != az2Ix && w3Ix != az3Ix && w3Ix != w2Ix
                    select (az2Ix, az3Ix, w2Ix, w3Ix)
                )
                    .ToList().Shuffle();

                static BigInteger b(int n) => ~(BigInteger.MinusOne << n);

                foreach (var (az2Ix, az3Ix, w2Ix, w3Ix) in quadruplets)
                {
                    string getWord(int w1x, int w2x, int w3x) => allWords.FirstOrDefault(aw => aw[0] == word1[w1x] && aw[az2Ix] - 'A' == w2x && aw[w2Ix] == word2[w2x] && aw[az3Ix] - 'A' == w3x && aw[w3Ix] == word3[w3x]);

                    IEnumerable<BigInteger> recurse(BigInteger available, BigInteger used, int ix)
                    {
                        if (ix == inputLen)
                        {
                            yield return used;
                            yield break;
                        }

                        var wall = (available >> (inputLen * inputLen * ix)) & b(inputLen * inputLen);
                        if (wall.IsZero)
                            yield break;
                        var availableWithoutWall = available & ~(b(inputLen) << (inputLen * inputLen * ix));
                        if (ix < inputLen * inputLen - 1 && availableWithoutWall.IsZero)
                            yield break;
                        var alleyMask = b(inputLen * inputLen * inputLen) / b(inputLen);
                        var shelfMask = b(inputLen * inputLen * inputLen) / b(inputLen * inputLen) * b(inputLen);
                        for (int i = 0; i < inputLen * inputLen; i++)
                        {
                            if (ix == 0)
                                Console.Write($"azs={az2Ix}/{az3Ix}, wixs={w2Ix}/{w3Ix}, i={i}            \r");
                            if (!wall.IsEven)
                                foreach (var solution in recurse(availableWithoutWall & ~(alleyMask << (i % inputLen)) & ~(shelfMask << (inputLen * (i / inputLen))), used | (BigInteger.One << (inputLen * inputLen * ix + i)), ix + 1))
                                    yield return solution;
                            wall >>= 1;
                        }
                    }

                    var combinationsPossible = BigInteger.Zero;
                    for (int w = 0; w < allWords.Length; w++)
                    {
                        var aw = allWords[w];
                        if (aw[az2Ix] - 'A' >= inputLen || word2[aw[az2Ix] - 'A'] != aw[w2Ix] || aw[az3Ix] - 'A' >= inputLen || word3[aw[az3Ix] - 'A'] != aw[w3Ix])
                            continue;
                        for (int w1x = 0; w1x < word1.Length; w1x++)
                            if (word1[w1x] == aw[0])
                                combinationsPossible |= BigInteger.One << (w1x + inputLen * (aw[az2Ix] - 'A' + inputLen * (aw[az3Ix] - 'A')));
                    }

                    if (
                        Enumerable.Range(0, inputLen).All(wall => !(combinationsPossible & (b(inputLen * inputLen) << (wall * inputLen * inputLen))).IsZero) &&
                        Enumerable.Range(0, inputLen).All(shelf => !(combinationsPossible & ((b(inputLen * inputLen * inputLen) / b(inputLen * inputLen) * b(inputLen)) << (shelf * inputLen))).IsZero) &&
                        Enumerable.Range(0, inputLen).All(alley => !(combinationsPossible & ((b(inputLen * inputLen * inputLen) / b(inputLen)) << alley)).IsZero))
                    {
                        foreach (var solution in recurse(combinationsPossible, BigInteger.Zero, 0))
                        {
                            Console.WriteLine($"azs={az2Ix}/{az3Ix}, wixs={w2Ix}/{w3Ix}            ");
                            ConsoleUtil.WriteLine(Enumerable.Range(0, inputLen * inputLen * inputLen)
                                .Where(i => !(solution >> i).IsEven)
                                .OrderBy(i => i % inputLen)
                                .Select(ix => getWord(ix % inputLen, (ix / inputLen) % inputLen, ix / inputLen / inputLen)
                                    .ColorSubstring(0, 1, ConsoleColor.White, ConsoleColor.DarkBlue)
                                    .ColorSubstring(az2Ix, 1, ConsoleColor.White, ConsoleColor.DarkGreen)
                                    .ColorSubstring(w2Ix, 1, ConsoleColor.White, ConsoleColor.DarkCyan)
                                    .ColorSubstring(az3Ix, 1, ConsoleColor.White, ConsoleColor.DarkRed)
                                    .ColorSubstring(w3Ix, 1, ConsoleColor.White, ConsoleColor.DarkMagenta)
                                )
                                .JoinColoredString("\n"));
                            Console.WriteLine();
                            break;
                        }
                    }
                }
                Console.WriteLine($"                             ");
            }
        }

        public static void GenerateWords()
        {
            var allWords = File.ReadAllLines(@"D:\Daten\Wordlists\peter_broda_wordlist.txt")
                .Select(row => row.Split(';'))
                .Select(row => (word: row[0].Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: int.Parse(row[1])))
                .Where(w => w.score >= 75)
                .Concat(File.ReadAllLines(@"D:\Daten\Wordlists\English 60000.txt").Select((w, wIx) => (word: w.Where(ch => ch >= 'A' && ch <= 'Z').JoinString(), score: 60024 - wIx)))
                .ToArray();

            var word1 = "NEUWESTEND";
            var word2 = "STADTMITTE";

            var len = word1.Length;
            if (word1.Length != word2.Length)
                Debugger.Break();

            var tt = new TextTable { ColumnSpacing = 2, RowSpacing = 1 };
            for (var azIx = 2; azIx < 8; azIx++)
                tt.SetCell(azIx - 1, 0, $"azIx = {azIx}".Color(ConsoleColor.White));
            for (var w2Ix = 2; w2Ix < 8; w2Ix++)
                tt.SetCell(0, w2Ix - 1, $"w2Ix = {w2Ix}".Color(ConsoleColor.White));

            var tableData = new Dictionary<(int azIx, int w2Ix), (int score, ConsoleColoredString data)>();
            var indexCombinations = Enumerable.Range(2, 6).SelectMany(azIx =>
                 Enumerable.Range(2, 6).Where(w2Ix => w2Ix != azIx).Select(w2Ix => (azIx, w2Ix))).ToArray();

            var lastUpdate = DateTime.UtcNow;
            void output()
            {
                lock (tableData)
                {
                    var now = DateTime.UtcNow;
                    if ((now - lastUpdate).TotalSeconds < 15)
                        return;
                    lastUpdate = now;
                    var top5 = tableData.OrderByDescending(kvp => kvp.Value.score).Take(5).Select(kvp => kvp.Key).ToArray();
                    foreach (var kvp in tableData)
                        tt.SetCell(kvp.Key.azIx - 1, kvp.Key.w2Ix - 1, kvp.Value.data, noWrap: true, background: top5.IndexOf(kvp.Key).Apply(a => a == -1 ? ConsoleColor.Black : a == 0 ? ConsoleColor.DarkGreen : ConsoleColor.DarkBlue));
                    Console.Clear();
                    tt.WriteToConsole();
                }
            }

            indexCombinations.ParallelForEach(tup =>
            {
                var (azIx, w2Ix) = tup;

                lock (tableData)
                    tt.SetCell(azIx - 1, w2Ix - 1, "Working...".Color(ConsoleColor.Yellow), noWrap: true);

                var bestScore = 0;
                ConsoleColoredString bestData = null;
                foreach (var swapped in new[] { false })
                {
                    var w1 = swapped ? word2 : word1;
                    var w2 = swapped ? word1 : word2;

                    var matrix = Ut.NewArray(len, len, (a, b) => allWords
                        .Where(tup => tup.word.Length > azIx && tup.word.Length > w2Ix && tup.word.StartsWith(w1[a]) && tup.word[azIx] - 'A' == b && tup.word[w2Ix] == w2[b])
                        .MaxElementOrDefault(tup => tup.score));

                    void recurse(int[] ixsSoFar, int ix)
                    {
                        if (ix == ixsSoFar.Length)
                        {
                            var score = Enumerable.Range(0, len).Sum(i => matrix[i][ixsSoFar[i]].score);
                            if (score < bestScore)
                                return;
                            bestScore = score;
                            var longestWord = Enumerable.Range(0, len).Max(i => matrix[i][ixsSoFar[i]].word.Length);

                            bestData = Enumerable.Range(0, len).Select(i =>
                                new ConsoleColoredString($@"{matrix[i][ixsSoFar[i]].word.PadRight(longestWord, '\u00a0')
                                    .ColorSubstring(0, 1, ConsoleColor.White, ConsoleColor.Blue)
                                    .ColorSubstring(azIx, 1, ConsoleColor.White, ConsoleColor.Red)
                                    .ColorSubstring(w2Ix, 1, ConsoleColor.White, ConsoleColor.Green)}{matrix[i][ixsSoFar[i]].score.ToString().PadLeft(7, '\u00a0').Color(ConsoleColor.DarkMagenta)}")).JoinColoredString("\n") + "\n" +
                                score.ToString().PadLeft(longestWord + 7, '\u00a0').Color(ConsoleColor.Magenta);
                            lock (tableData)
                                tableData[tup] = (bestScore, bestData);
                            output();
                            return;
                        }

                        foreach (var i in Enumerable.Range(0, len).Where(i => !ixsSoFar.Take(ix).Contains(i) && matrix[ix][i].word != null).OrderByDescending(i => matrix[ix][i].score))
                        {
                            ixsSoFar[ix] = i;
                            recurse(ixsSoFar, ix + 1);
                        }
                    }
                    recurse(new int[len], 0);
                }

                lock (tableData)
                    tableData[tup] = bestData == null ? (0, "NOTHING".Color(ConsoleColor.Red)) : (bestScore, bestData);
                output();
            });
            Console.Clear();
            tt.WriteToConsole();
        }

        public static void GenerateSvg()
        {
            var svg = XDocument.Parse(File.ReadAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\No, U\Graphics v2 template.svg"));
            var sb = new StringBuilder();

            void fillBox(int id, int x2, string s1, string s2, int azIx, int w2Ix, params (string answer, string clue)[] clues)
            {
                var s1t = clues.Select(c => c.answer[0]).JoinString();
                if (s1t != s1)
                    Debugger.Break();
                if (!clues.OrderBy(c => c.answer[azIx]).Select(c => c.answer[azIx] - 'A').SequenceEqual(Enumerable.Range(0, clues.Length)))
                    Debugger.Break();
                var s2t = clues.OrderBy(c => c.answer[azIx]).Select(c => c.answer[w2Ix]).JoinString();
                if (s2t != s2)
                    Debugger.Break();

                var box = svg.Root.ElementsI("rect").FirstOrDefault(e => e.AttributeI("id").Value == $"box{id}");
                var x = int.Parse(box.AttributeI("x").Value);
                var y = int.Parse(box.AttributeI("y").Value);

                void el(string name, string content, params (string attr, object value)[] attrs)
                {
                    var e = new XElement(XName.Get(name, box.Name.NamespaceName));
                    foreach (var (attr, value) in attrs)
                        e.Add(new XAttribute(attr, value));
                    if (content != null)
                        e.Add(content);
                    svg.Root.Add(e);
                }

                // Grey line behind the dots
                el("line", null, ("x1", x + x2), ("y1", y + 1), ("x2", x + x2), ("y2", y + 3 + 4 * clues.Length), ("stroke", "#808080"), ("stroke-width", 1));

                for (var i = 0; i < clues.Length; i++)
                {
                    var (answer, clue) = clues[i];
                    var cl = clue.EndsWith(')') ? clue : $"{clue.Trim()} ({answer.Length})";
                    el("text", cl, ("x", x + x2 - 3), ("y", y + 5 + 4 * i));
                    el("circle", null, ("cx", x + x2), ("cy", y + 4 + 4 * i), ("r", 1), ("stroke", "#000"), ("stroke-width", ".5"), ("fill", "#fff"));
                    for (var j = 0; j < answer.Length; j++)
                        el("line", null, ("x1", x + x2 + 3 + 4 * j), ("y1", y + 6 + 4 * i), ("x2", x + x2 + 5 + 4 * j), ("y2", y + 6 + 4 * i), ("stroke", "#000"), ("stroke-width", ".5"), ("stroke-linecap", "square"));
                    sb.AppendLine(cl);
                }
                sb.AppendLine();

                var width = x2 + 3 + 4 * clues.Max(tup => tup.answer.Length);
                box.AttributeI("width").Value = width.ToString();
                box.AttributeI("height").Value = (clues.Length * 4 + 4).ToString();
                //el("text", label, ("x", x + width), ("y", y - 2));
            }

            fillBox(1, 60, "WESTHAFEN", "NEUKOELLN", 6, 3,
                ("WEEKEND", "Saturday and Sunday"),
                ("ENGLISH", "Language or spin"),
                ("SIGNIFICANT", "Type of figures to round to"),
                ("TIMETABLE", "Train schedule"),
                ("HYPOTHESIS", "Conjecture or assumption"),
                ("ACCURACY", "Precision partner"),
                ("FEELING", "Emotion"),
                ("EXONERATE", "Prove innocent of a crime"),
                ("NINETYFIVE", "Number of Martin Luther’s theses (6, 4)"));

            fillBox(2, 66, "MEHRINGDAMM", "TURMSTRASSE", 4, 2,
                ("MIRAGE", "Illusory desert sight"),
                ("EXTRA", "Additional"),
                ("HOSPITAL", "Building where sick people get cured"),
                ("REACH", "Achieve (a goal) or attain (a place) "),
                ("IMMEDIATELY", "Without delay"),
                ("NOSEJOB", "Rhinoplasty, familiarly (4, 3)"),
                ("GREEK", "Hellenic language"),
                ("DRUMBEAT", "Rhythm from a percussive instrument (4, 4)"),
                ("ARTIFICIAL", "Man-made or contrived"),
                ("MYSTERY", "Enigma or Agatha Christie genre"),
                ("MIRACLES", "What Hot Chocolate believes in"));

            fillBox(3, 64, "RUHLEBEN", "WUHLETAL", 2, 5,
                ("RICOCHET", "Bounce or Trevor Mann"),
                ("UNHEALTHY", "Medically inadvisable"),
                ("HABITUAL", "Made routine by long or frequent use"),
                ("LOGICAL", "Consistent according to deduction"),
                ("EFFECT", "Cause partner"),
                ("BOARDWALK", "$400 property (not Mayfair) "),
                ("ELDERLY", "Aged or senior (people) "),
                ("NEEDLE", "Main indicator in a VU meter"));

            fillBox(4, 60, "HALEMWEG", "KIENBERG", 5, 4,
                ("HORRIBLE", "Hägar, according to Dik Browne"),
                ("APPRECIATE", "Esteem or comprehend"),
                ("LAFORGE", "Geordi from Star Trek TNG"),
                ("ENOUGH", "Sufficiently"),
                ("MIRANDARIGHTS", "What the police may inform you of (7, 6)"),
                ("WESTBENGAL", "Kolkata’s state (4, 6)"),
                ("EUREKA", "Archimedes’s exclamation"),
                ("GRATEFULDEAD", "Touch of Grey band (8, 4)"));

            fillBox(5, 76, "POTSDAMERPLATZ", "LIPSCHITZALLEE", 3, 6,
                ("PROMOTE", "Endorse, or give a higher position to"),
                ("OARFISH", "Regalecidae"),
                ("TALLTALE", "Entertaining, but likely fictitious story (4, 4)"),
                ("SANJAYA", "American Idol sixth-season finalist Malakar"),
                ("DUTCHOPEN", "Netherlands badminton or darts tournament (5, 4)"),
                ("ASPHALT", "Bitumen"),
                ("MAXIMIZE", "Set a window to fill the whole screen"),
                ("ETHANOL", "C₂H₅OH"),
                ("RESEARCH", "Investigate scientifically"),
                ("PROBABILITY", "Likelihood"),
                ("LOUDNESS", "Volume of a sound"),
                ("AMENDMENT", "The fourth prohibits searches and seizures"),
                ("TRAGEDIES", "Plays like Romeo and Juliet"),
                ("ZAKKWYLDE", "Ozzy Osbourne guitarist (4, 5)"));

            fillBox(6, 68, "NEUWESTEND", "STADTMITTE", 2, 6,
                ("NIGHTTIME", "Period of the curious incident of the dog (5, 4)"),
                ("ENJOYMENT", "Pleasure, liking or fun"),
                ("UPDATED", "Installed a newer version of"),
                ("WEAPONS", "Crossbows, slingshots and trebuchets"),
                ("EMIGRATE", "Leave a country permanently"),
                ("SUBSTITUTE", "Ersatz or replacement"),
                ("THEORETICAL", "Hypothesized, or applied partner"),
                ("EXHAUST", "Pipe at the back of a car"),
                ("NICOSIA", "Cyprus’s capital"),
                ("DEFRAGMENT", "Arrange files on a hard disk contiguously"));

            fillBox(7, 70, "PARADESTRASSE", "JUNGFERNHEIDE", 4, 3,
                ("PROFESSOR", "Juniper, Layton, Moriarty, Snape, etc."),
                ("ASUNCION", "Paraguay’s capital"),
                ("REMEMBER", "Retain in one’s mind"),
                ("ANNEJACKSON", "The Shining and Zig Zag actress (4, 7)"),
                ("DOWNHILL", "Steeply declining, or type of ski competition"),
                ("ENERGY", "Strength, liveliness or power"),
                ("STRIKE", "Knock down all pins in bowling"),
                ("TROJANWAR", "Conflict that is the subject of the Iliad (6, 3)"),
                ("ROSEFINCH", "Carpodacus"),
                ("AMYGDALA", "Almond-shaped part of the brain"),
                ("SADDLE", "Leather between horse and rider"),
                ("SCHUBERT", "Ave Maria and Impromptus composer Franz"),
                ("ETCHING", "Carving into stone"));

            fillBox(8, 68, "BLASCHKOALLEE", "ZWICKAUERDAMM", 2, 4,
                ("BREAK", "Rest period or render defective"),
                ("LUDACRIS", "Rollout and Act a Fool rapper"),
                ("ABKHAZIA", "Sukhumi’s state"),
                ("SWAYZE", "Dirty Dancing actor Patrick"),
                ("COLUMN", "Pillar or newspaper section"),
                ("HOMEMADE", "Prepared by oneself or Jake Owen song"),
                ("KAFKA", "The Trial and Metamorphosis author Franz"),
                ("OCHSENKNECHT", "Das Boot actor Uwe"),
                ("ABJAD", "Writing system such as Arabic or Hebrew"),
                ("LUCKILY", "Fortunately"),
                ("LUGDUNUM", "Lyon, France, but when it was Gaul"),
                ("ELBOW", "Joint connecting the humerus and radius"),
                ("ERITREA", "Asmara’s country"));

            File.WriteAllText(@"D:\c\PuzzleStuff\DataFiles\Bomb Disposal Puzzle Hunt\No, U\Graphics v2.svg", svg.ToString(SaveOptions.DisableFormatting));
            Clipboard.SetText(sb.ToString());
        }
    }
}
