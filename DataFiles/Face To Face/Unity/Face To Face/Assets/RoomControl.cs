using System.Linq;
using Assets;
using UnityEngine;

public partial class RoomControl : MonoBehaviour
{
    public AudioClip[] AudioClips;
    public GameObject[] Walls;
    public TextMesh[] CyanNumbers;
    public TextMesh[] PinkNumbers1;
    public TextMesh[] PinkNumbers2;
    public GameObject[] Doors;
    public GameObject Box;
    public GameObject Radio;
    public AudioSource RadioAudio;
    public Material RadioMaterial;
    public Light Light;

    private float[] _doorRotations;

    void Awake()
    {
        _doorRotations = Enumerable.Range(0, 5).Select(ix => Doors[ix].transform.localEulerAngles.y).ToArray();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Do Stuff/Set camera &0")] public static void Camera0() { FindObjectOfType<RoomControl>().SetRoom(0, 0, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &1")] public static void Camera1() { FindObjectOfType<RoomControl>().SetRoom(0, 1, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &2")] public static void Camera2() { FindObjectOfType<RoomControl>().SetRoom(0, 2, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &3")] public static void Camera3() { FindObjectOfType<RoomControl>().SetRoom(0, 3, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &4")] public static void Camera4() { FindObjectOfType<RoomControl>().SetRoom(0, 4, setCamera: true); }

    [UnityEditor.MenuItem("Do Stuff/Set objects &5")]
    public static void SetObjects()
    {
        var t = FindObjectOfType<RoomControl>();
        for (var ix = 0; ix < 5; ix++)
        {
            t.CyanNumbers[ix].transform.parent = t.transform;
            t.CyanNumbers[ix].transform.Set(Data.CyanNumbersPositions[ix], new Vector3(.01f, .01f, .01f));
            t.CyanNumbers[ix].transform.parent = t.transform.Find("Wall " + ix).Find("Door " + ix);
            t.PinkNumbers1[ix].transform.Set(Data.PinkNumbers1Positions[ix], new Vector3(.013f, .013f, .013f));
            t.PinkNumbers2[ix].transform.Set(Data.PinkNumbers2Positions[ix], new Vector3(.013f, .013f, .013f));
            t.Doors[ix].transform.Set(Data.DoorPositions[ix], new Vector3(1, 1, 1));
        }
    }
#endif

    public void SetRoom(int faceIx, int edgeIx, bool setCamera)
    {
        foreach (var wall in Walls)
            wall.SetActive(true);
        Walls[edgeIx].SetActive(false);

        for (var ix = 0; ix < 5; ix++)
        {
            CyanNumbers[ix].text = Data.Faces[faceIx].Edges[ix].CyanNumber.ToString();
            PinkNumbers1[ix].text = Data.Faces[faceIx].Edges[ix].PinkNumber.ToString();
            PinkNumbers2[ix].text = Data.Faces[faceIx].Edges[ix].PinkNumber.ToString();
            Doors[ix].transform.localEulerAngles = new Vector3(0, _doorRotations[ix], 0);
        }

        RadioAudio.clip = AudioClips.FirstOrDefault(ac => ac.name == Data.Faces[faceIx].SongSnippet);
        Light.intensity = Data.LightIntensity;

        if (setCamera)
            Camera.main.transform.Set(Data.CameraPositions[edgeIx], new Vector3(1, 1, 1));
    }
}

