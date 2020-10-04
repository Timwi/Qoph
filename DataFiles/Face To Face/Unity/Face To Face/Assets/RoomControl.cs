using System.Linq;
using System.Xml;
using Assets;
using UnityEngine;

public class RoomControl : MonoBehaviour
{
    public GameObject[] Walls;
    public TextMesh[] CyanNumbers;
    public TextMesh[] PinkNumbers1;
    public TextMesh[] PinkNumbers2;
    public GameObject[] Doors;

    struct PosAndDir
    {
        public Vector3 From;
        public Vector3 To;
    }

    private static Vector3 vec(double x, double y, double z)
    {
        return new Vector3((float) x, (float) y, (float) z);
    }

    private static readonly PosAndDir[] cameraPositions = /*%%*/new[] { new PosAndDir { From = vec(-0.106155360099494, 0.22, -0.627897844431435), To = vec(0.500298290461587, 0.2, -0.393987669960254) }, new PosAndDir { From = vec(0.578205942822172, 0.22, -1.12963497493154), To = vec(0.620382493944185, 0.2, -0.481004773248441) }, new PosAndDir { From = vec(1.32266550054035, 0.22, -0.719118700110655), To = vec(0.751610070716591, 0.2, -0.408642137277325) }, new PosAndDir { From = vec(1.2624518908724, 0.22, 0.1288895346262), To = vec(0.74099608100617, 0.2, -0.259161800066463) }, new PosAndDir { From = vec(0.469270841534359, 0.22, 0.430475946836905), To = vec(0.602675316707923, 0.2, -0.205686961547155) } }/*%%%*/;
    private static readonly PosAndDir[] cyanNumbersPositions = /*##*/new[] { new PosAndDir { From = vec(0.153935278851433, 0.28, -0.392129637015384), To = vec(0.0582928731298692, 0.28, -0.429019070838524) }, new PosAndDir { From = vec(0.599457084757511, 0.28, -0.802815163618874), To = vec(0.592805518210561, 0.28, -0.905109135887388) }, new PosAndDir { From = vec(1.03493263312254, 0.28, -0.562681501505203), To = vec(1.12499246713982, 0.28, -0.611646043745272) }, new PosAndDir { From = vec(0.999710364273492, 0.28, -0.0666346078740985), To = vec(1.08194795668793, 0.28, -0.00543592738279854) }, new PosAndDir { From = vec(0.412802130451163, 0.28, 0.0840007344959902), To = vec(0.391763218528022, 0.28, 0.18432851870899) } }/*###*/;
    private static readonly PosAndDir[] pinkNumbers1Positions = /*&&*/new[] { new PosAndDir { From = vec(9.33005616247817E-05, 0.22, 3.5986180687874E-05), To = vec(-0.0933005616247817, 0.22, -0.035986180687874) }, new PosAndDir { From = vec(0.303193358209474, 0.22, -0.785965991039501), To = vec(0.296698169336684, 0.22, -0.885855042098698) }, new PosAndDir { From = vec(0.895313712575544, 0.22, -0.824526202253627), To = vec(0.983256248768403, 0.22, -0.87233959292996) }, new PosAndDir { From = vec(1.17879378002914, 0.22, -0.303246569714639), To = vec(1.25909797474854, 0.22, -0.243486664171969) }, new PosAndDir { From = vec(0.82459449164409, 0.22, 0.172816732852364), To = vec(0.804050202467361, 0.22, 0.27078582074351) } }/*&&&*/;
    private static readonly PosAndDir[] pinkNumbers2Positions = /*@@*/new[] { new PosAndDir { From = vec(0.303280170070926, 0.22, -0.786029794120611), To = vec(0.20988630788452, 0.22, -0.822051960989173) }, new PosAndDir { From = vec(0.895408055957228, 0.22, -0.824474178616881), To = vec(0.888912867084438, 0.22, -0.924363229676078) }, new PosAndDir { From = vec(1.17878614931838, 0.22, -0.30313910388425), To = vec(1.26672868551124, 0.22, -0.350952494560583) }, new PosAndDir { From = vec(0.82449374390793, 0.22, 0.172854903863701), To = vec(0.904797938627329, 0.22, 0.232614809406372) }, new PosAndDir { From = vec(2.05237654113175E-05, 0.22, -9.78712166744708E-05), To = vec(-0.0205237654113175, 0.22, 0.0978712166744708) } }/*@@@*/;
    private static readonly PosAndDir[] doorPositions = /*::*/new[] { new PosAndDir { From = vec(0.151593434754651, 0.175, -0.393032890150649), To = vec(-0.781412181493166, 0.175, -0.75289469702939) }, new PosAndDir { From = vec(0.599294218383178, 0.175, -0.805319874089989), To = vec(0.534407216657003, 0.175, -1.80321249206398) }, new PosAndDir { From = vec(1.03713778562847, 0.175, -0.56388041869399), To = vec(1.91568460074195, 0.175, -1.04153666920681) }, new PosAndDir { From = vec(1.00172398593928, 0.175, -0.0651361327201314), To = vec(1.80396369342579, 0.175, 0.531865920653197) }, new PosAndDir { From = vec(0.412286983939339, 0.175, 0.0864573020345194), To = vec(0.207049329826165, 0.175, 1.06516946877923) } }/*:::*/;

    [UnityEditor.MenuItem("Do Stuff/Set camera &0")] public static void Camera0() { FindObjectOfType<RoomControl>().SetRoom(0, 0); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &1")] public static void Camera1() { FindObjectOfType<RoomControl>().SetRoom(0, 1); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &2")] public static void Camera2() { FindObjectOfType<RoomControl>().SetRoom(0, 2); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &3")] public static void Camera3() { FindObjectOfType<RoomControl>().SetRoom(0, 3); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &4")] public static void Camera4() { FindObjectOfType<RoomControl>().SetRoom(0, 4); }

    [UnityEditor.MenuItem("Do Stuff/Set objects &5")]
    public static void SetObjects()
    {
        var t = FindObjectOfType<RoomControl>();
        for (var ix = 0; ix < 5; ix++)
        {
            Set(t.CyanNumbers[ix].transform, cyanNumbersPositions[ix], new Vector3(.01f, .01f, .01f));
            Set(t.PinkNumbers1[ix].transform, pinkNumbers1Positions[ix], new Vector3(.013f, .013f, .013f));
            Set(t.PinkNumbers2[ix].transform, pinkNumbers2Positions[ix], new Vector3(.013f, .013f, .013f));
            Set(t.Doors[ix].transform, doorPositions[ix], new Vector3(1, 1, 1));
        }
    }

    private static void Set(Transform tr, PosAndDir position, Vector3 scale)
    {
        tr.localPosition = position.From;
        tr.localRotation = Quaternion.LookRotation(position.To - position.From);
        tr.localScale = scale;
    }

    public void SetRoom(int faceIx, int edgeIx)
    {
        Set(Camera.main.transform, cameraPositions[edgeIx], new Vector3(1, 1, 1));
        foreach (var wall in Walls)
            wall.SetActive(true);
        Walls[edgeIx].SetActive(false);

        for (var ix = 0; ix < 5; ix++)
        {
            CyanNumbers[ix].text = FaceToFaceData.Data[faceIx].CyanNumbers[ix].ToString();
            PinkNumbers1[ix].text = FaceToFaceData.Data[faceIx].PinkNumbers[ix].ToString();
            PinkNumbers2[ix].text = FaceToFaceData.Data[faceIx].PinkNumbers[ix].ToString();
        }
    }
}

