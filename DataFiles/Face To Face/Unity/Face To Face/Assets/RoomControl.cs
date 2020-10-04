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
            t.CyanNumbers[ix].transform.Set(Data.CyanNumbersPositions[ix], new Vector3(.01f, .01f, .01f));
            t.PinkNumbers1[ix].transform.Set(Data.PinkNumbers1Positions[ix], new Vector3(.013f, .013f, .013f));
            t.PinkNumbers2[ix].transform.Set(Data.PinkNumbers2Positions[ix], new Vector3(.013f, .013f, .013f));
            t.Doors[ix].transform.Set(Data.DoorPositions[ix], new Vector3(1, 1, 1));
        }
    }

    public void SetRoom(int faceIx, int edgeIx)
    {
        foreach (var wall in Walls)
            wall.SetActive(true);
        Walls[edgeIx].SetActive(false);

        for (var ix = 0; ix < 5; ix++)
        {
            CyanNumbers[ix].text = Data.Faces[faceIx].Edges[ix].CyanNumber.ToString();
            PinkNumbers1[ix].text = Data.Faces[faceIx].Edges[ix].PinkNumber.ToString();
            PinkNumbers2[ix].text = Data.Faces[faceIx].Edges[ix].PinkNumber.ToString();
        }

        RadioAudio.clip = AudioClips.FirstOrDefault(ac => ac.name == Data.Faces[faceIx].SongSnippet);
    }
}

