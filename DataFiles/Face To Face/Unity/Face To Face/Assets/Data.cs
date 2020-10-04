using UnityEngine;

namespace Assets
{
    static class Data
    {
        public static readonly FaceData[] Faces = /*Faces-start*/new[] { new FaceData { CarpetColor = "AQUA", CarpetLength = 0, SongSnippet = "Magnolia", ItemInBox = "a pile of trash", Edges = new[] { new Edge { CyanNumber = 25, PinkNumber = 41, Label = null, Face = 1 }, new Edge { CyanNumber = 36, PinkNumber = 40, Label = "Itinerary (5)", Face = null }, new Edge { CyanNumber = 24, PinkNumber = 46, Label = "+0", Face = null }, new Edge { CyanNumber = 23, PinkNumber = 58, Label = null, Face = 11 }, new Edge { CyanNumber = 16, PinkNumber = 47, Label = null, Face = 3 } } }, new FaceData { CarpetColor = "ONYX", CarpetLength = 1, SongSnippet = "Holding Out for a Hero", ItemInBox = "a body spray", Edges = new[] { new Edge { CyanNumber = 12, PinkNumber = 41, Label = "+7", Face = null }, new Edge { CyanNumber = 30, PinkNumber = 22, Label = null, Face = 15 }, new Edge { CyanNumber = 29, PinkNumber = 23, Label = null, Face = 17 }, new Edge { CyanNumber = 33, PinkNumber = 33, Label = null, Face = 18 }, new Edge { CyanNumber = 25, PinkNumber = 40, Label = null, Face = 0 } } }, new FaceData { CarpetColor = "GAMBOGE", CarpetLength = 3, SongSnippet = "The Elements", ItemInBox = "a bucket of orange paint", Edges = new[] { new Edge { CyanNumber = 3, PinkNumber = 41, Label = "Require (4)", Face = null }, new Edge { CyanNumber = 27, PinkNumber = 36, Label = null, Face = 20 }, new Edge { CyanNumber = 16, PinkNumber = 46, Label = null, Face = 14 }, new Edge { CyanNumber = 20, PinkNumber = 27, Label = "−10", Face = null }, new Edge { CyanNumber = 12, PinkNumber = 22, Label = null, Face = null } } }, new FaceData { CarpetColor = "JADE", CarpetLength = 2, SongSnippet = "Everybody Wants to Rule the World", ItemInBox = "a worm", Edges = new[] { new Edge { CyanNumber = 16, PinkNumber = 41, Label = null, Face = 0 }, new Edge { CyanNumber = 11, PinkNumber = 47, Label = "−1", Face = null }, new Edge { CyanNumber = 26, PinkNumber = 55, Label = null, Face = 23 }, new Edge { CyanNumber = 28, PinkNumber = 52, Label = null, Face = 20 }, new Edge { CyanNumber = 3, PinkNumber = 36, Label = "Beginning (5)", Face = null } } }, new FaceData { CarpetColor = "FUCHSIA", CarpetLength = 0, SongSnippet = "Shape of You", ItemInBox = "a toupee", Edges = new[] { new Edge { CyanNumber = 9, PinkNumber = 59, Label = null, Face = 5 }, new Edge { CyanNumber = 21, PinkNumber = 61, Label = null, Face = 22 }, new Edge { CyanNumber = 20, PinkNumber = 49, Label = "+1", Face = null }, new Edge { CyanNumber = 12, PinkNumber = 55, Label = null, Face = 9 }, new Edge { CyanNumber = 11, PinkNumber = 51, Label = null, Face = 7 } } }, new FaceData { CarpetColor = "GAMBOGE", CarpetLength = 0, SongSnippet = "Blinding Lights", ItemInBox = "a copy of the album \"So Much Fun\"", Edges = new[] { new Edge { CyanNumber = 18, PinkNumber = 59, Label = "+3", Face = null }, new Edge { CyanNumber = 17, PinkNumber = 36, Label = null, Face = 13 }, new Edge { CyanNumber = 30, PinkNumber = 50, Label = "Agreement or dish out cards (4)", Face = null }, new Edge { CyanNumber = 22, PinkNumber = 62, Label = null, Face = null }, new Edge { CyanNumber = 9, PinkNumber = 61, Label = null, Face = 4 } } }, new FaceData { CarpetColor = "GAMBOGE", CarpetLength = 2, SongSnippet = "Imagine", ItemInBox = "a beaker of alkaline solution", Edges = new[] { new Edge { CyanNumber = 20, PinkNumber = 59, Label = null, Face = 7 }, new Edge { CyanNumber = 19, PinkNumber = 21, Label = null, Face = 16 }, new Edge { CyanNumber = 34, PinkNumber = 32, Label = "+3", Face = null }, new Edge { CyanNumber = 25, PinkNumber = 31, Label = "Ovum (3)", Face = null }, new Edge { CyanNumber = 18, PinkNumber = 36, Label = null, Face = null } } }, new FaceData { CarpetColor = "FUCHSIA", CarpetLength = 3, SongSnippet = "Royals", ItemInBox = "a bottle of salty water", Edges = new[] { new Edge { CyanNumber = 11, PinkNumber = 59, Label = null, Face = 4 }, new Edge { CyanNumber = 15, PinkNumber = 51, Label = null, Face = 9 }, new Edge { CyanNumber = 27, PinkNumber = 47, Label = "−10", Face = null }, new Edge { CyanNumber = 13, PinkNumber = 30, Label = null, Face = null }, new Edge { CyanNumber = 20, PinkNumber = 21, Label = null, Face = 6 } } }, new FaceData { CarpetColor = "FUCHSIA", CarpetLength = 2, SongSnippet = "Space Oddity", ItemInBox = "a nail", Edges = new[] { new Edge { CyanNumber = 24, PinkNumber = 75, Label = null, Face = 9 }, new Edge { CyanNumber = 20, PinkNumber = 55, Label = "−5", Face = null }, new Edge { CyanNumber = 33, PinkNumber = 49, Label = null, Face = 22 }, new Edge { CyanNumber = 40, PinkNumber = 55, Label = "Loathe (4)", Face = null }, new Edge { CyanNumber = 25, PinkNumber = 63, Label = null, Face = 11 } } }, new FaceData { CarpetColor = "JADE", CarpetLength = 0, SongSnippet = "Eternal Flame", ItemInBox = "a box of matches", Edges = new[] { new Edge { CyanNumber = 18, PinkNumber = 75, Label = null, Face = 10 }, new Edge { CyanNumber = 28, PinkNumber = 54, Label = "+10", Face = null }, new Edge { CyanNumber = 15, PinkNumber = 47, Label = null, Face = 7 }, new Edge { CyanNumber = 12, PinkNumber = 51, Label = null, Face = 4 }, new Edge { CyanNumber = 24, PinkNumber = 55, Label = null, Face = 8 } } }, new FaceData { CarpetColor = "PINK", CarpetLength = 3, SongSnippet = "Africa", ItemInBox = "a copy of the funnies with their beginning and end torn off and the rest scrambled", Edges = new[] { new Edge { CyanNumber = 19, PinkNumber = 75, Label = null, Face = 11 }, new Edge { CyanNumber = 24, PinkNumber = 58, Label = "+0", Face = null }, new Edge { CyanNumber = 32, PinkNumber = 46, Label = null, Face = 18 }, new Edge { CyanNumber = 30, PinkNumber = 44, Label = null, Face = null }, new Edge { CyanNumber = 18, PinkNumber = 54, Label = null, Face = 9 } } }, new FaceData { CarpetColor = "ONYX", CarpetLength = 2, SongSnippet = "Lemon Tree", ItemInBox = "a piercing", Edges = new[] { new Edge { CyanNumber = 25, PinkNumber = 75, Label = null, Face = 8 }, new Edge { CyanNumber = 33, PinkNumber = 63, Label = "For the length of (5)", Face = null }, new Edge { CyanNumber = 11, PinkNumber = 55, Label = "+0", Face = null }, new Edge { CyanNumber = 23, PinkNumber = 47, Label = null, Face = 0 }, new Edge { CyanNumber = 19, PinkNumber = 58, Label = null, Face = 10 } } }, new FaceData { CarpetColor = "AQUA", CarpetLength = 2, SongSnippet = "Yellow Submarine", ItemInBox = "a bottle of anticonvulsants", Edges = new[] { new Edge { CyanNumber = 33, PinkNumber = 41, Label = "+13", Face = null }, new Edge { CyanNumber = 34, PinkNumber = 31, Label = null, Face = null }, new Edge { CyanNumber = 27, PinkNumber = 32, Label = null, Face = null }, new Edge { CyanNumber = 39, PinkNumber = 36, Label = null, Face = 17 }, new Edge { CyanNumber = 40, PinkNumber = 33, Label = null, Face = 15 } } }, new FaceData { CarpetColor = "VIOLET", CarpetLength = 3, SongSnippet = "Bohemian Rhapsody", ItemInBox = "a copy of the third Super Smash Bros. game", Edges = new[] { new Edge { CyanNumber = 27, PinkNumber = 41, Label = null, Face = 14 }, new Edge { CyanNumber = 37, PinkNumber = 40, Label = null, Face = 21 }, new Edge { CyanNumber = 17, PinkNumber = 50, Label = null, Face = 5 }, new Edge { CyanNumber = 25, PinkNumber = 36, Label = "+8", Face = null }, new Edge { CyanNumber = 33, PinkNumber = 31, Label = null, Face = null } } }, new FaceData { CarpetColor = "GAMBOGE", CarpetLength = 4, SongSnippet = "I'm Still Standing", ItemInBox = "a kitchen sink", Edges = new[] { new Edge { CyanNumber = 34, PinkNumber = 41, Label = null, Face = 15 }, new Edge { CyanNumber = 16, PinkNumber = 27, Label = null, Face = 2 }, new Edge { CyanNumber = 41, PinkNumber = 46, Label = "+14", Face = null }, new Edge { CyanNumber = 40, PinkNumber = 57, Label = null, Face = 21 }, new Edge { CyanNumber = 27, PinkNumber = 40, Label = null, Face = 13 } } }, new FaceData { CarpetColor = "FUCHSIA", CarpetLength = 4, SongSnippet = "Saturnz Barz", ItemInBox = "a painting of Queen Victoria", Edges = new[] { new Edge { CyanNumber = 40, PinkNumber = 41, Label = null, Face = 12 }, new Edge { CyanNumber = 37, PinkNumber = 33, Label = "−9", Face = null }, new Edge { CyanNumber = 30, PinkNumber = 23, Label = null, Face = 1 }, new Edge { CyanNumber = 20, PinkNumber = 22, Label = null, Face = null }, new Edge { CyanNumber = 34, PinkNumber = 27, Label = null, Face = 14 } } }, new FaceData { CarpetColor = "PINK", CarpetLength = 0, SongSnippet = "Still Alive", ItemInBox = "a plush toy", Edges = new[] { new Edge { CyanNumber = 24, PinkNumber = 46, Label = null, Face = 17 }, new Edge { CyanNumber = 27, PinkNumber = 36, Label = "+14", Face = null }, new Edge { CyanNumber = 19, PinkNumber = 32, Label = null, Face = 6 }, new Edge { CyanNumber = 13, PinkNumber = 21, Label = null, Face = null }, new Edge { CyanNumber = 26, PinkNumber = 30, Label = null, Face = 19 } } }, new FaceData { CarpetColor = "AZURE", CarpetLength = 3, SongSnippet = "Take Me Home, Country Roads", ItemInBox = "a toy railway car", Edges = new[] { new Edge { CyanNumber = 40, PinkNumber = 46, Label = null, Face = 18 }, new Edge { CyanNumber = 29, PinkNumber = 33, Label = null, Face = 1 }, new Edge { CyanNumber = 37, PinkNumber = 23, Label = "+10", Face = null }, new Edge { CyanNumber = 39, PinkNumber = 33, Label = null, Face = 12 }, new Edge { CyanNumber = 24, PinkNumber = 36, Label = null, Face = 16 } } }, new FaceData { CarpetColor = "VIOLET", CarpetLength = 5, SongSnippet = "Angels", ItemInBox = "a frozen cube", Edges = new[] { new Edge { CyanNumber = 42, PinkNumber = 46, Label = null, Face = 19 }, new Edge { CyanNumber = 32, PinkNumber = 44, Label = null, Face = 10 }, new Edge { CyanNumber = 36, PinkNumber = 46, Label = "+7", Face = null }, new Edge { CyanNumber = 33, PinkNumber = 40, Label = null, Face = 1 }, new Edge { CyanNumber = 40, PinkNumber = 33, Label = null, Face = 17 } } }, new FaceData { CarpetColor = "VIOLET", CarpetLength = 0, SongSnippet = "Mr. Blue Sky", ItemInBox = "a piece of banister", Edges = new[] { new Edge { CyanNumber = 26, PinkNumber = 46, Label = null, Face = 16 }, new Edge { CyanNumber = 27, PinkNumber = 30, Label = "Leaving or functioning (5)", Face = null }, new Edge { CyanNumber = 28, PinkNumber = 47, Label = "+16", Face = null }, new Edge { CyanNumber = 30, PinkNumber = 54, Label = null, Face = null }, new Edge { CyanNumber = 42, PinkNumber = 44, Label = null, Face = 18 } } }, new FaceData { CarpetColor = "WHITE", CarpetLength = 0, SongSnippet = "Barbie Girl", ItemInBox = "a rat", Edges = new[] { new Edge { CyanNumber = 51, PinkNumber = 86, Label = null, Face = 21 }, new Edge { CyanNumber = 41, PinkNumber = 57, Label = "+4", Face = null }, new Edge { CyanNumber = 27, PinkNumber = 46, Label = null, Face = 2 }, new Edge { CyanNumber = 28, PinkNumber = 36, Label = null, Face = 3 }, new Edge { CyanNumber = 50, PinkNumber = 52, Label = null, Face = 23 } } }, new FaceData { CarpetColor = "ONYX", CarpetLength = 3, SongSnippet = "God's Plan", ItemInBox = "a book of jokes", Edges = new[] { new Edge { CyanNumber = 42, PinkNumber = 86, Label = "X (3)", Face = null }, new Edge { CyanNumber = 30, PinkNumber = 62, Label = "+6", Face = null }, new Edge { CyanNumber = 37, PinkNumber = 50, Label = null, Face = 13 }, new Edge { CyanNumber = 40, PinkNumber = 40, Label = null, Face = 14 }, new Edge { CyanNumber = 51, PinkNumber = 57, Label = null, Face = 20 } } }, new FaceData { CarpetColor = "AQUA", CarpetLength = 1, SongSnippet = "Rasputin", ItemInBox = "a pile of mud", Edges = new[] { new Edge { CyanNumber = 41, PinkNumber = 86, Label = null, Face = 23 }, new Edge { CyanNumber = 33, PinkNumber = 55, Label = null, Face = 8 }, new Edge { CyanNumber = 21, PinkNumber = 49, Label = null, Face = 4 }, new Edge { CyanNumber = 22, PinkNumber = 61, Label = "Apportioning (7)", Face = null }, new Edge { CyanNumber = 42, PinkNumber = 62, Label = "−2", Face = null } } }, new FaceData { CarpetColor = "AZURE", CarpetLength = 1, SongSnippet = "Wannabe", ItemInBox = "a bottle of spirits", Edges = new[] { new Edge { CyanNumber = 50, PinkNumber = 86, Label = null, Face = 20 }, new Edge { CyanNumber = 26, PinkNumber = 52, Label = null, Face = 3 }, new Edge { CyanNumber = 33, PinkNumber = 55, Label = "Sister (3)", Face = null }, new Edge { CyanNumber = 40, PinkNumber = 63, Label = "−5", Face = null }, new Edge { CyanNumber = 41, PinkNumber = 55, Label = null, Face = 22 } } } }/*Faces-end*/;
        public static readonly PosAndDir[] CameraPositions = /*CameraPositions-start*/new[] { new PosAndDir { From = vec(-0.106155360099494, 0.22, -0.627897844431435), To = vec(0.500298290461587, 0.2, -0.393987669960254) }, new PosAndDir { From = vec(0.578205942822172, 0.22, -1.12963497493154), To = vec(0.620382493944185, 0.2, -0.481004773248441) }, new PosAndDir { From = vec(1.32266550054035, 0.22, -0.719118700110655), To = vec(0.751610070716591, 0.2, -0.408642137277325) }, new PosAndDir { From = vec(1.2624518908724, 0.22, 0.1288895346262), To = vec(0.74099608100617, 0.2, -0.259161800066463) }, new PosAndDir { From = vec(0.469270841534359, 0.22, 0.430475946836905), To = vec(0.602675316707923, 0.2, -0.205686961547155) } }/*CameraPositions-end*/;
        public static readonly PosAndDir[] InCameraPositions = /*InCameraPositions-start*/new[] { new PosAndDir { From = vec(0.500298290461587, 0.22, -0.393987669960254), To = vec(-0.106155360099494, 0.2, -0.627897844431435) }, new PosAndDir { From = vec(0.620382493944185, 0.22, -0.481004773248441), To = vec(0.578205942822172, 0.2, -1.12963497493154) }, new PosAndDir { From = vec(0.751610070716591, 0.22, -0.408642137277325), To = vec(1.32266550054035, 0.2, -0.719118700110655) }, new PosAndDir { From = vec(0.74099608100617, 0.22, -0.259161800066463), To = vec(1.2624518908724, 0.2, 0.1288895346262) }, new PosAndDir { From = vec(0.602675316707923, 0.22, -0.205686961547155), To = vec(0.469270841534359, 0.2, 0.430475946836905) } }/*InCameraPositions-start*/;
        public static readonly PosAndDir[] CyanNumbersPositions = /*CyanNumbersPositions-start*/new[] { new PosAndDir { From = vec(0.153935278851433, 0.28, -0.392129637015384), To = vec(0.0582928731298692, 0.28, -0.429019070838524) }, new PosAndDir { From = vec(0.599457084757511, 0.28, -0.802815163618874), To = vec(0.592805518210561, 0.28, -0.905109135887388) }, new PosAndDir { From = vec(1.03493263312254, 0.28, -0.562681501505203), To = vec(1.12499246713982, 0.28, -0.611646043745272) }, new PosAndDir { From = vec(0.999710364273492, 0.28, -0.0666346078740985), To = vec(1.08194795668793, 0.28, -0.00543592738279854) }, new PosAndDir { From = vec(0.412802130451163, 0.28, 0.0840007344959902), To = vec(0.391763218528022, 0.28, 0.18432851870899) } }/*CyanNumbersPositions-end*/;
        public static readonly PosAndDir[] PinkNumbers1Positions = /*PinkNumbers1Positions-start*/new[] { new PosAndDir { From = vec(9.33005616247817E-05, 0.22, 3.5986180687874E-05), To = vec(-0.0933005616247817, 0.22, -0.035986180687874) }, new PosAndDir { From = vec(0.303193358209474, 0.22, -0.785965991039501), To = vec(0.296698169336684, 0.22, -0.885855042098698) }, new PosAndDir { From = vec(0.895313712575544, 0.22, -0.824526202253627), To = vec(0.983256248768403, 0.22, -0.87233959292996) }, new PosAndDir { From = vec(1.17879378002914, 0.22, -0.303246569714639), To = vec(1.25909797474854, 0.22, -0.243486664171969) }, new PosAndDir { From = vec(0.82459449164409, 0.22, 0.172816732852364), To = vec(0.804050202467361, 0.22, 0.27078582074351) } }/*PinkNumbers1Positions-end*/;
        public static readonly PosAndDir[] PinkNumbers2Positions = /*PinkNumbers2Positions-start*/new[] { new PosAndDir { From = vec(0.303280170070926, 0.22, -0.786029794120611), To = vec(0.20988630788452, 0.22, -0.822051960989173) }, new PosAndDir { From = vec(0.895408055957228, 0.22, -0.824474178616881), To = vec(0.888912867084438, 0.22, -0.924363229676078) }, new PosAndDir { From = vec(1.17878614931838, 0.22, -0.30313910388425), To = vec(1.26672868551124, 0.22, -0.350952494560583) }, new PosAndDir { From = vec(0.82449374390793, 0.22, 0.172854903863701), To = vec(0.904797938627329, 0.22, 0.232614809406372) }, new PosAndDir { From = vec(2.05237654113175E-05, 0.22, -9.78712166744708E-05), To = vec(-0.0205237654113175, 0.22, 0.0978712166744708) } }/*PinkNumbers2Positions-end*/;
        public static readonly PosAndDir[] DoorPositions = /*DoorPositions-start*/new[] { new PosAndDir { From = vec(0.151593434754651, 0.175, -0.393032890150649), To = vec(-0.781412181493166, 0.175, -0.75289469702939) }, new PosAndDir { From = vec(0.599294218383178, 0.175, -0.805319874089989), To = vec(0.534407216657003, 0.175, -1.80321249206398) }, new PosAndDir { From = vec(1.03713778562847, 0.175, -0.56388041869399), To = vec(1.91568460074195, 0.175, -1.04153666920681) }, new PosAndDir { From = vec(1.00172398593928, 0.175, -0.0651361327201314), To = vec(1.80396369342579, 0.175, 0.531865920653197) }, new PosAndDir { From = vec(0.412286983939339, 0.175, 0.0864573020345194), To = vec(0.207049329826165, 0.175, 1.06516946877923) } }/*DoorPositions-end*/;

        public static readonly Vector3[] RotateRoomAbout = { /*RotateRoomAbout-start*/new Vector3(0f, 0, 0f), new Vector3(-0.895401567257055f, 0, -0.824573967878678f), new Vector3(-0.895401567257055f, 0, -0.824573967878678f), new Vector3(-1.00172398593928f, 0, -0.0651361327201314f), new Vector3(0f, 0, 0f)/*RotateRoomAbout-end*/ };
        public static readonly float[] RotateRoomBy = { /*RotateRoomBy-start*/80.7517020883924f, -114.812074477902f, 114.812074477902f, 180f, -80.7517020883924f/*RotateRoomBy-end*/ };
        public static readonly Vector3[] TiltRoomAbout = { /*TiltRoomAbout-start*/new Vector3(0.303186869509302f, 0, -0.786065780301299f), new Vector3(0.592214697747754f, 0, -0.0385081875773795f), new Vector3(0.283472436742833f, 0, 0.521387098369377f), new Vector3(-0.177150018060605f, 0, 0.23805073678917f), new Vector3(-0.824573967878678f, 0, -0.172914604069039f)/*TiltRoomAbout-end*/ };
        public const float TiltAngle = /*TiltAngle-start*/-19.819052188761f/*TiltAngle-end*/;

        public static readonly Vector3 Midpoint = /*Midpoint-start*/new Vector3(0.640407281728985f, .22f, -0.348182402724048f)/*Midpoint-end*/;

        private static Vector3 vec(double x, double y, double z)
        {
            return new Vector3((float) x, (float) y, (float) z);
        }

        public static void Set(this Transform tr, PosAndDir position, Vector3 scale)
        {
            tr.localPosition = position.From;
            tr.localRotation = Quaternion.LookRotation(position.To - position.From);
            tr.localScale = scale;
        }
    }

    sealed class FaceData
    {
        public string CarpetColor;
        public int CarpetLength;
        public string SongSnippet;
        public string ItemInBox;
        public Edge[] Edges;
    }

    sealed class Edge
    {
        public int CyanNumber;
        public int PinkNumber;
        public string Label;
        public int? Face;
    }
}
