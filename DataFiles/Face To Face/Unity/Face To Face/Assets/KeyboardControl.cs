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

    private static readonly CameraPos[] cameraPositions = /*%%*/new[] { new CameraPos { From = vec(-0.11664967549372, 0.34, -0.586795246381976), To = vec(0.480473918904882, 0.3, -0.356483689979582) }, new CameraPos { From = vec(0.579828117865326, 0.34, -1.10468765948219), To = vec(0.618760318901031, 0.3, -0.505952088697791) }, new CameraPos { From = vec(1.30070183016252, 0.34, -0.707177293847835), To = vec(0.773573741094428, 0.3, -0.420583543540145) }, new CameraPos { From = vec(1.24239589818523, 0.34, 0.113964483291867), To = vec(0.761052073693333, 0.3, -0.24423674873213) }, new CameraPos { From = vec(0.429068331410991, 0.34, 0.41693665579973), To = vec(0.560420430043423, 0.3, -0.209439130916883) } }/*%%%*/;
    private static readonly CameraPos[] cyanNumbers = /*##*/new[] { new CameraPos { From = vec(0.151686735316276, 0.4, -0.392996903969962), To = vec(0.0582928731298692, 0.4, -0.429019070838524) }, new CameraPos { From = vec(0.599300707083351, 0.4, -0.805220084828191), To = vec(0.592805518210561, 0.4, -0.905109135887388) }, new CameraPos { From = vec(1.03704993094696, 0.4, -0.563832653068939), To = vec(1.12499246713982, 0.4, -0.611646043745272) }, new CameraPos { From = vec(1.00164376196853, 0.4, -0.0651958329254687), To = vec(1.08194795668793, 0.4, -0.00543592738279854) }, new CameraPos { From = vec(0.412307507704751, 0.4, 0.0863594308178449), To = vec(0.391763218528022, 0.4, 0.18432851870899) } }/*###*/;
    private static readonly CameraPos[] pinkNumbers1 = /*&&*/new[] { new CameraPos { From = vec(9.33005616247817E-05, 0.4, 3.5986180687874E-05), To = vec(-0.0933005616247817, 0.4, -0.035986180687874) }, new CameraPos { From = vec(0.303193358209474, 0.4, -0.785965991039501), To = vec(0.296698169336684, 0.4, -0.885855042098698) }, new CameraPos { From = vec(0.895313712575544, 0.4, -0.824526202253627), To = vec(0.983256248768403, 0.4, -0.87233959292996) }, new CameraPos { From = vec(1.17879378002914, 0.4, -0.303246569714639), To = vec(1.25909797474854, 0.4, -0.243486664171969) }, new CameraPos { From = vec(0.82459449164409, 0.4, 0.172816732852364), To = vec(0.804050202467361, 0.4, 0.27078582074351) } }/*&&&*/;
    private static readonly CameraPos[] pinkNumbers2 = /*@@*/new[] { new CameraPos { From = vec(0.303280170070926, 0.4, -0.786029794120611), To = vec(0.20988630788452, 0.4, -0.822051960989173) }, new CameraPos { From = vec(0.895408055957228, 0.4, -0.824474178616881), To = vec(0.888912867084438, 0.4, -0.924363229676078) }, new CameraPos { From = vec(1.17878614931838, 0.4, -0.30313910388425), To = vec(1.26672868551124, 0.4, -0.350952494560583) }, new CameraPos { From = vec(0.82449374390793, 0.4, 0.172854903863701), To = vec(0.904797938627329, 0.4, 0.232614809406372) }, new CameraPos { From = vec(2.05237654113175E-05, 0.4, -9.78712166744708E-05), To = vec(-0.0205237654113175, 0.4, 0.0978712166744708) } }/*@@@*/;

    private int curCamera = cameraPositions.Length - 1;

    public GameObject[] Walls;
    public GameObject CyanNumberTemplate;
    public GameObject PinkNumberTemplate;
    public GameObject Sphere;

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
