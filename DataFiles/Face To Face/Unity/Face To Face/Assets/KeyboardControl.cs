using UnityEngine;

public class KeyboardControl : MonoBehaviour
{
    struct CameraPos
    {
        public Vector3 From;
        public Vector3 To;
    }

    private static Vector3 vec(double x, double y, double z)
    {
        return new Vector3((float) x, (float) y, (float) z);
    }

    private static readonly CameraPos[] cameraPositions = /*%%*/new[] { new CameraPos { From = vec(-0.106155360099494, 0.22, -0.627897844431435), To = vec(0.500298290461587, 0.2, -0.393987669960254) }, new CameraPos { From = vec(0.578205942822172, 0.22, -1.12963497493154), To = vec(0.620382493944185, 0.2, -0.481004773248441) }, new CameraPos { From = vec(1.32266550054035, 0.22, -0.719118700110655), To = vec(0.751610070716591, 0.2, -0.408642137277325) }, new CameraPos { From = vec(1.2624518908724, 0.22, 0.1288895346262), To = vec(0.74099608100617, 0.2, -0.259161800066463) }, new CameraPos { From = vec(0.469270841534359, 0.22, 0.430475946836905), To = vec(0.602675316707923, 0.2, -0.205686961547155) } }/*%%%*/;
    private static readonly CameraPos[] cyanNumbers = /*##*/new[] { new CameraPos { From = vec(0.148654866621183, 0.28, -0.385136246166949), To = vec(0.0552610044347761, 0.28, -0.42115841303551) }, new CameraPos { From = vec(0.593378560105874, 0.28, -0.804835002952417), To = vec(0.586883371233083, 0.28, -0.904724054011614) }, new CameraPos { From = vec(1.03421520657953, 0.28, -0.569046524052632), To = vec(1.12215774277239, 0.28, -0.616859914728965) }, new CameraPos { From = vec(1.00518676232975, 0.28, -0.0699568476612522), To = vec(1.08549095704915, 0.28, -0.010196942118582) }, new CameraPos { From = vec(0.420553247383537, 0.28, 0.0880885768585353), To = vec(0.400008958206809, 0.28, 0.186057664749681) } }/*###*/;
    private static readonly CameraPos[] pinkNumbers1 = /*&&*/new[] { new CameraPos { From = vec(9.33005616247817E-05, 0.28, 3.5986180687874E-05), To = vec(-0.0933005616247817, 0.28, -0.035986180687874) }, new CameraPos { From = vec(0.303193358209474, 0.28, -0.785965991039501), To = vec(0.296698169336684, 0.28, -0.885855042098698) }, new CameraPos { From = vec(0.895313712575544, 0.28, -0.824526202253627), To = vec(0.983256248768403, 0.28, -0.87233959292996) }, new CameraPos { From = vec(1.17879378002914, 0.28, -0.303246569714639), To = vec(1.25909797474854, 0.28, -0.243486664171969) }, new CameraPos { From = vec(0.82459449164409, 0.28, 0.172816732852364), To = vec(0.804050202467361, 0.28, 0.27078582074351) } }/*&&&*/;
    private static readonly CameraPos[] pinkNumbers2 = /*@@*/new[] { new CameraPos { From = vec(0.303280170070926, 0.28, -0.786029794120611), To = vec(0.20988630788452, 0.28, -0.822051960989173) }, new CameraPos { From = vec(0.895408055957228, 0.28, -0.824474178616881), To = vec(0.888912867084438, 0.28, -0.924363229676078) }, new CameraPos { From = vec(1.17878614931838, 0.28, -0.30313910388425), To = vec(1.26672868551124, 0.28, -0.350952494560583) }, new CameraPos { From = vec(0.82449374390793, 0.28, 0.172854903863701), To = vec(0.904797938627329, 0.28, 0.232614809406372) }, new CameraPos { From = vec(2.05237654113175E-05, 0.28, -9.78712166744708E-05), To = vec(-0.0205237654113175, 0.28, 0.0978712166744708) } }/*@@@*/;

    private int curCamera = cameraPositions.Length - 1;

    public GameObject[] Walls;
    public GameObject CyanNumberTemplate;
    public GameObject PinkNumberTemplate;
    public GameObject Sphere;

    [UnityEditor.MenuItem("Face To Face/Set camera &0")] public static void Camera0() { SetCamera(0); }
    [UnityEditor.MenuItem("Face To Face/Set camera &1")] public static void Camera1() { SetCamera(1); }
    [UnityEditor.MenuItem("Face To Face/Set camera &2")] public static void Camera2() { SetCamera(2); }
    [UnityEditor.MenuItem("Face To Face/Set camera &3")] public static void Camera3() { SetCamera(3); }
    [UnityEditor.MenuItem("Face To Face/Set camera &4")] public static void Camera4() { SetCamera(4); }
    public static void SetCamera(int ix)
    {
        var m = FindObjectOfType<KeyboardControl>();
        var c = FindObjectOfType<Camera>();
        c.transform.position = cameraPositions[ix].From;
        c.transform.rotation = Quaternion.LookRotation(cameraPositions[ix].To - cameraPositions[ix].From);
    }

    void Start()
    {
        for (var i = 0; i < cyanNumbers.Length; i++)
        {
            var cyanNumber = Instantiate(CyanNumberTemplate);
            cyanNumber.transform.parent = Walls[i].transform;
            cyanNumber.transform.localScale = new Vector3(1, 1, 1) * .01f;
            cyanNumber.transform.localPosition = cyanNumbers[i].From;
            cyanNumber.transform.localRotation = Quaternion.LookRotation(cyanNumbers[i].To - cyanNumbers[i].From, Vector3.up);
            cyanNumber.SetActive(true);
            cyanNumber.GetComponent<TextMesh>().text = "" + Random.Range(30, 50);

            var pinkNumber1 = Instantiate(PinkNumberTemplate);
            pinkNumber1.transform.parent = Walls[i].transform;
            pinkNumber1.transform.localScale = new Vector3(1, 1, 1) * .01f;
            pinkNumber1.transform.localPosition = pinkNumbers1[i].From;
            pinkNumber1.transform.localRotation = Quaternion.LookRotation(pinkNumbers1[i].To - pinkNumbers1[i].From, Vector3.up);
            pinkNumber1.SetActive(true);
            pinkNumber1.GetComponent<TextMesh>().text = "" + Random.Range(30, 50);

            var pinkNumber2 = Instantiate(PinkNumberTemplate);
            pinkNumber2.transform.parent = Walls[i].transform;
            pinkNumber2.transform.localScale = new Vector3(1, 1, 1) * .01f;
            pinkNumber2.transform.localPosition = pinkNumbers2[i].From;
            pinkNumber2.transform.localRotation = Quaternion.LookRotation(pinkNumbers2[i].To - pinkNumbers2[i].From, Vector3.up);
            pinkNumber2.SetActive(true);
            pinkNumber2.GetComponent<TextMesh>().text = pinkNumber1.GetComponent<TextMesh>().text;
        }
        Sphere.SetActive(false);
        CyanNumberTemplate.SetActive(false);
        PinkNumberTemplate.SetActive(false);

        NextWall();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            NextWall();
    }

    private void NextWall()
    {
        Walls[curCamera].SetActive(true);
        curCamera = (curCamera + 1) % cameraPositions.Length;
        Camera.main.transform.position = cameraPositions[curCamera].From;
        Camera.main.transform.rotation = Quaternion.LookRotation(cameraPositions[curCamera].To - cameraPositions[curCamera].From);
        Walls[curCamera].SetActive(false);
    }
}
