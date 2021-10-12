using UnityEngine;

namespace Assets
{
    static class Data
    {
        public static readonly FaceData[] Faces = /*Faces-start*/new[] { new FaceData { CarpetColor = "gamboge", CarpetLength = 3, ItemInBox = "a pile of trash", LampBro = 1, Edges = new[] { new Edge { PinkNumber = 41, Face = 1 }, new Edge { PinkNumber = 40, Label = "Itinerary\n(5)", LabelFontSize = 14 }, new Edge { Label = "+0", LabelFontSize = 32 }, new Edge { Face = 11 }, new Edge { PinkNumber = 47, Face = 3 } } }, new FaceData { CarpetColor = "pink", CarpetLength = 1, ItemInBox = "a body spray", LampBro = 14, Edges = new[] { new Edge { PinkNumber = 41, Label = "+7", LabelFontSize = 32 }, new Edge { PinkNumber = 22, Face = 15 }, new Edge { Face = 17 }, new Edge { PinkNumber = 33, Face = 18 }, new Edge { PinkNumber = 40, Face = 0 } } }, new FaceData { CarpetColor = "white", CarpetLength = 5, ItemInBox = "a bucket of orange paint", LampBro = 20, Edges = new[] { new Edge { PinkNumber = 41, Label = "Require\n(4)", LabelFontSize = 18 }, new Edge { PinkNumber = 36, Face = 20 }, new Edge { PinkNumber = 46, Face = 14 }, new Edge { Label = "-10", LabelFontSize = 32 }, new Edge { PinkNumber = 22 } } }, new FaceData { CarpetColor = "jade", CarpetLength = 3, ItemInBox = "a worm", LampBro = 11, Edges = new[] { new Edge { PinkNumber = 41, Face = 0 }, new Edge { PinkNumber = 47, Label = "-1", LabelFontSize = 32 }, new Edge { Face = 23 }, new Edge { Face = 20 }, new Edge { PinkNumber = 36, Label = "Beginning\n(5)", LabelFontSize = 16 } } }, new FaceData { CarpetColor = "fuchsia", CarpetLength = 1, ItemInBox = "a doormat", LampBro = 6, Edges = new[] { new Edge { PinkNumber = 59, Face = 5 }, new Edge { Face = 22 }, new Edge { PinkNumber = 49, Label = "+1", LabelFontSize = 32 }, new Edge { PinkNumber = 55, Face = 9 }, new Edge { Face = 7 } } }, new FaceData { CarpetColor = "gamboge", CarpetLength = 1, ItemInBox = "a copy of the album \"So Much Fun\"", LampBro = 8, Edges = new[] { new Edge { PinkNumber = 59, Label = "+3", LabelFontSize = 32 }, new Edge { PinkNumber = 36, Face = 13 }, new Edge { PinkNumber = 50, Label = "Agreement or dish\nout cards (4)", LabelFontSize = 9 }, new Edge {  }, new Edge { Face = 4 } } }, new FaceData { CarpetColor = "violet", CarpetLength = 1, ItemInBox = "a beaker of alkaline solution", LampBro = 2, Edges = new[] { new Edge { PinkNumber = 59, Face = 7 }, new Edge { PinkNumber = 21, Face = 16 }, new Edge { PinkNumber = 32, Label = "+3", LabelFontSize = 32 }, new Edge { PinkNumber = 31, Label = "Ovum (3)", LabelFontSize = 18 }, new Edge { PinkNumber = 36 } } }, new FaceData { CarpetColor = "white", CarpetLength = 2, ItemInBox = "a bottle of salty water", LampBro = 4, Edges = new[] { new Edge { PinkNumber = 59, Face = 4 }, new Edge { Face = 9 }, new Edge { Label = "-10", LabelFontSize = 32 }, new Edge { PinkNumber = 30 }, new Edge { PinkNumber = 21, Face = 6 } } }, new FaceData { CarpetColor = "violet", CarpetLength = 4, ItemInBox = "a nail", LampBro = 3, Edges = new[] { new Edge { Face = 9 }, new Edge { PinkNumber = 55, Label = "-5", LabelFontSize = 32 }, new Edge { PinkNumber = 49, Face = 22 }, new Edge { PinkNumber = 55, Label = "Loathe\n(4)", LabelFontSize = 17 }, new Edge { Face = 11 } } }, new FaceData { CarpetColor = "jade", CarpetLength = 1, ItemInBox = "a box of matches", LampBro = 16, Edges = new[] { new Edge { Face = 10 }, new Edge { Label = "+10", LabelFontSize = 32 }, new Edge { Face = 7 }, new Edge { Face = 4 }, new Edge { PinkNumber = 55, Face = 8 } } }, new FaceData { CarpetColor = "pink", CarpetLength = 4, ItemInBox = "a copy of the funnies with their beginning and end torn off and the rest scrambled", LampBro = 12, Edges = new[] { new Edge { Face = 11 }, new Edge { Label = "+0", LabelFontSize = 32 }, new Edge { Face = 18 }, new Edge { PinkNumber = 44 }, new Edge { Face = 9 } } }, new FaceData { CarpetColor = "aqua", CarpetLength = 1, ItemInBox = "a piercing", LampBro = 25, Edges = new[] { new Edge { Face = 8 }, new Edge { Label = "For the length\nof (5)", LabelFontSize = 11 }, new Edge { Label = "+0", LabelFontSize = 32 }, new Edge { PinkNumber = 47, Face = 0 }, new Edge { Face = 10 } } }, new FaceData { CarpetColor = "azure", CarpetLength = 4, ItemInBox = "a bottle of anticonvulsants", LampBro = 21, Edges = new[] { new Edge { PinkNumber = 41, Label = "+13", LabelFontSize = 32 }, new Edge { PinkNumber = 31 }, new Edge { PinkNumber = 32 }, new Edge { PinkNumber = 36, Face = 17 }, new Edge { Face = 15 } } }, new FaceData { CarpetColor = "onyx", CarpetLength = 2, ItemInBox = "a copy of the third Super Smash Bros. game", LampBro = 22, Edges = new[] { new Edge { PinkNumber = 41, Face = 14 }, new Edge { PinkNumber = 40, Face = 21 }, new Edge { PinkNumber = 50, Face = 5 }, new Edge { PinkNumber = 36, Label = "+8", LabelFontSize = 32 }, new Edge { PinkNumber = 31 } } }, new FaceData { CarpetColor = "white", CarpetLength = 3, ItemInBox = "a kitchen sink", LampBro = 15, Edges = new[] { new Edge { PinkNumber = 41, Face = 15 }, new Edge { Face = 2 }, new Edge { PinkNumber = 46, Label = "+14", LabelFontSize = 32 }, new Edge { PinkNumber = 57, Face = 21 }, new Edge { PinkNumber = 40, Face = 13 } } }, new FaceData { CarpetColor = "gamboge", CarpetLength = 4, ItemInBox = "a painting of Queen Victoria", LampBro = 19, Edges = new[] { new Edge { PinkNumber = 41, Face = 12 }, new Edge { Label = "-9", LabelFontSize = 32 }, new Edge { Face = 1 }, new Edge { PinkNumber = 22 }, new Edge { Face = 14 } } }, new FaceData { CarpetColor = "violet", CarpetLength = 3, ItemInBox = "a plush toy", LampBro = 13, Edges = new[] { new Edge { PinkNumber = 46, Face = 17 }, new Edge { PinkNumber = 36, Label = "+14", LabelFontSize = 32 }, new Edge { PinkNumber = 32, Face = 6 }, new Edge { PinkNumber = 21 }, new Edge { PinkNumber = 30, Face = 19 } } }, new FaceData { CarpetColor = "onyx", CarpetLength = 3, ItemInBox = "a toy railway car", LampBro = 18, Edges = new[] { new Edge { PinkNumber = 46, Face = 18 }, new Edge { PinkNumber = 33, Face = 1 }, new Edge { Label = "+10", LabelFontSize = 32 }, new Edge { Face = 12 }, new Edge { PinkNumber = 36, Face = 16 } } }, new FaceData { CarpetColor = "white", CarpetLength = 4, ItemInBox = "a frozen cube", LampBro = 10, Edges = new[] { new Edge { PinkNumber = 46, Face = 19 }, new Edge { PinkNumber = 44, Face = 10 }, new Edge { Label = "+7", LabelFontSize = 32 }, new Edge { PinkNumber = 40, Face = 1 }, new Edge { PinkNumber = 33, Face = 17 } } }, new FaceData { CarpetColor = "fuchsia", CarpetLength = 5, ItemInBox = "a piece of banister", LampBro = 23, Edges = new[] { new Edge { PinkNumber = 46, Face = 16 }, new Edge { PinkNumber = 30, Label = "Leaving or\nfunctioning (5)", LabelFontSize = 11 }, new Edge { Label = "+16", LabelFontSize = 32 }, new Edge {  }, new Edge { PinkNumber = 44, Face = 18 } } }, new FaceData { CarpetColor = "aqua", CarpetLength = 3, ItemInBox = "a rat", LampBro = 26, Edges = new[] { new Edge { PinkNumber = 86, Face = 21 }, new Edge { PinkNumber = 57, Label = "+4", LabelFontSize = 32 }, new Edge { PinkNumber = 46, Face = 2 }, new Edge { PinkNumber = 36, Face = 3 }, new Edge { Face = 23 } } }, new FaceData { CarpetColor = "onyx", CarpetLength = 4, ItemInBox = "a book of jokes", LampBro = 7, Edges = new[] { new Edge { PinkNumber = 86, Label = "X (3)", LabelFontSize = 18 }, new Edge { Label = "+6", LabelFontSize = 32 }, new Edge { PinkNumber = 50, Face = 13 }, new Edge { PinkNumber = 40, Face = 14 }, new Edge { PinkNumber = 57, Face = 20 } } }, new FaceData { CarpetColor = "aqua", CarpetLength = 2, ItemInBox = "a pile of mud", LampBro = 17, Edges = new[] { new Edge { PinkNumber = 86, Face = 23 }, new Edge { PinkNumber = 55, Face = 8 }, new Edge { PinkNumber = 49, Face = 4 }, new Edge { Label = "Apportioning\n(7)", LabelFontSize = 14 }, new Edge { Label = "-2", LabelFontSize = 32 } } }, new FaceData { CarpetColor = "azure", CarpetLength = 2, ItemInBox = "a bottle of spirits", LampBro = 24, Edges = new[] { new Edge { PinkNumber = 86, Face = 20 }, new Edge { Face = 3 }, new Edge { Label = "Sister (3)", LabelFontSize = 16 }, new Edge { Label = "-5", LabelFontSize = 32 }, new Edge { PinkNumber = 55, Face = 22 } } } }/*Faces-end*/;
        public static readonly PosAndDir[] CameraPositions = /*CameraPositions-start*/new[] { new PosAndDir { From = vec(-0.0732657134536217, 0.22, -0.588122089712853), To = vec(0.449217431645156, 0.2, -0.386599477860758) }, new PosAndDir { From = vec(0.581125857899849, 0.22, -1.08472980712271), To = vec(0.617462578866507, 0.2, -0.525909941057271) }, new PosAndDir { From = vec(1.28313089386025, 0.22, -0.697624168837578), To = vec(0.791144677396698, 0.2, -0.430136668550402) }, new PosAndDir { From = vec(1.2263511040355, 0.22, 0.102024442224401), To = vec(0.777096867843063, 0.2, -0.232296707664663) }, new PosAndDir { From = vec(0.453769316933092, 0.22, 0.381246461211322), To = vec(0.568702403236469, 0.2, -0.166832352165714) } }/*CameraPositions-end*/;
        public static readonly PosAndDir[] InCameraPositions = /*InCameraPositions-start*/new[] { new PosAndDir { From = vec(0.654478667219675, 0.275, -0.307429880347435), To = vec(-0.278526949028141, 0.13, -0.667291687226175) }, new PosAndDir { From = vec(0.631737719246266, 0.275, -0.306373565102993), To = vec(0.566850717520091, 0.13, -1.30426618307698) }, new PosAndDir { From = vec(0.597864378071732, 0.275, -0.325052293437582), To = vec(1.47641119318521, 0.13, -0.802708543950398) }, new PosAndDir { From = vec(0.600604132196032, 0.275, -0.363637159406796), To = vec(1.40284383968253, 0.13, 0.233364893966533) }, new PosAndDir { From = vec(0.613854687141368, 0.275, -0.38214902884955), To = vec(0.408617033028193, 0.13, 0.596563137895158) } }/*InCameraPositions-end*/;
        public static readonly PosAndDir[] CyanNumbers1Positions = /*CyanNumbers1Positions-start*/new[] { new PosAndDir { From = vec(9.33005616247817E-05, 0.365, 3.5986180687874E-05), To = vec(-0.0933005616247817, 0.365, -0.035986180687874) }, new PosAndDir { From = vec(0.303193358209474, 0.365, -0.785965991039501), To = vec(0.296698169336684, 0.365, -0.885855042098698) }, new PosAndDir { From = vec(0.895313712575544, 0.365, -0.824526202253627), To = vec(0.983256248768403, 0.365, -0.87233959292996) }, new PosAndDir { From = vec(1.17879378002914, 0.365, -0.303246569714639), To = vec(1.25909797474854, 0.365, -0.243486664171969) }, new PosAndDir { From = vec(0.82459449164409, 0.365, 0.172816732852364), To = vec(0.804050202467361, 0.365, 0.27078582074351) } }/*CyanNumbers1Positions-end*/;
        public static readonly PosAndDir[] CyanNumbers2Positions = /*CyanNumbers2Positions-start*/new[] { new PosAndDir { From = vec(2.05237654113175E-05, 0.365, -9.78712166744708E-05), To = vec(-0.0205237654113175, 0.365, 0.0978712166744708) }, new PosAndDir { From = vec(0.303280170070926, 0.365, -0.786029794120611), To = vec(0.20988630788452, 0.365, -0.822051960989173) }, new PosAndDir { From = vec(0.895408055957228, 0.365, -0.824474178616881), To = vec(0.888912867084438, 0.365, -0.924363229676078) }, new PosAndDir { From = vec(1.17878614931838, 0.365, -0.30313910388425), To = vec(1.26672868551124, 0.365, -0.350952494560583) }, new PosAndDir { From = vec(0.82449374390793, 0.365, 0.172854903863701), To = vec(0.904797938627329, 0.365, 0.232614809406372) } }/*CyanNumbers2Positions-end*/;
        public static readonly PosAndDir[] DoorPositions = /*DoorPositions-start*/new[] { new PosAndDir { From = vec(0.220363421714854, 0.175, -0.571331289249109), To = vec(-0.712642194532963, 0.175, -0.931193096127849) }, new PosAndDir { From = vec(0.689104554000838, 0.175, -0.811159704245344), To = vec(0.624217552274663, 0.175, -1.80905232221934) }, new PosAndDir { From = vec(1.08012684817463, 0.175, -0.484811205333777), To = vec(1.9586736632881, 0.175, -0.962467455846593) }, new PosAndDir { From = vec(0.947993801135684, 0.175, 0.00706544095365381), To = vec(1.75023350862219, 0.175, 0.604067494326982) }, new PosAndDir { From = vec(0.423151765077757, 0.175, 0.0887356656526183), To = vec(0.217914110964582, 0.175, 1.06744783239733) } }/*DoorPositions-end*/;
        public static readonly Vector3[] WallPositions = new[] { /*WallPositions-start*/vec(0.187975859095767, 0, -0.487360783786805), vec(0.599294218383178, 0, -0.805319874089989), vec(1.03713778562847, 0, -0.56388041869399), vec(1.00172398593928, 0, -0.0651361327201314), vec(0.511235860084781, 0, 0.107207054522804)/*WallPositions-end*/ };

        public static readonly Vector3[] RotateRoomAbout = { /*RotateRoomAbout-start*/new Vector3(0f, 0, 0f), new Vector3(0.895401567257055f, 0, -0.824573967878678f), new Vector3(0.895401567257055f, 0, -0.824573967878678f), new Vector3(1.00172398593928f, 0, -0.0651361327201314f), new Vector3(0f, 0, 0f)/*RotateRoomAbout-end*/ };
        public static readonly float[] RotateRoomBy = { /*RotateRoomBy-start*/80.7517020883924f, -114.812074477902f, 114.812074477902f, 180f, -80.7517020883924f/*RotateRoomBy-end*/ };
        public static readonly Vector3[] TiltRoomAbout = { /*TiltRoomAbout-start*/new Vector3(0.303186869509302f, 0, -0.786065780301299f), new Vector3(0.592214697747754f, 0, -0.0385081875773795f), new Vector3(0.283472436742833f, 0, 0.521387098369377f), new Vector3(-0.177150018060605f, 0, 0.23805073678917f), new Vector3(-0.824573967878678f, 0, -0.172914604069039f)/*TiltRoomAbout-end*/ };
        public const float TiltAngle = /*TiltAngle-start*/-19.819052188761f/*TiltAngle-end*/;
        public const float LightIntensity = .2f;

        public static readonly Vector3 Midpoint = /*Midpoint-start*/new Vector3(0.640407281728985f, 0.275f, -0.348182402724048f)/*Midpoint-end*/;

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
        public string ItemInBox;
        public int LampBro;
        public Edge[] Edges;
    }

    sealed class Edge
    {
        public int? PinkNumber;
        public string Label;
        public int? LabelFontSize;
        public int? Face;
    }
}
