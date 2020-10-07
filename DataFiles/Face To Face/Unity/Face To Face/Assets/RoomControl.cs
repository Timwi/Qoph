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
    public GameObject[] DoorSigns;
    public TextMesh[] DoorSignTexts;
    public GameObject[] Doors;
    public GameObject Box;
    public GameObject Radio;
    public AudioSource RadioAudio;
    public Material RadioMaterial;
    public Light Light;
    public float[] DoorRotationsY;
    public Material[] CarpetMaterials;
    public MeshRenderer[] Carpets;

    public void SetRoom(int faceIx, int edgeIx, bool setCamera)
    {
        foreach (var wall in Walls)
            wall.SetActive(true);
        Walls[edgeIx].SetActive(false);

        for (var i = 0; i < 5; i++)
        {
            CyanNumbers[i].text = Data.Faces[faceIx].Edges[i].CyanNumber.ToString();
            PinkNumbers1[i].text = Data.Faces[faceIx].Edges[i].PinkNumber.ToString();
            PinkNumbers2[i].text = Data.Faces[faceIx].Edges[i].PinkNumber.ToString();
            Doors[i].transform.localEulerAngles = new Vector3(0, DoorRotationsY[i], 0);
            DoorSigns[i].SetActive(Data.Faces[faceIx].Edges[i].Label != null);
            DoorSignTexts[i].text = Data.Faces[faceIx].Edges[i].Label ?? "";
            DoorSignTexts[i].fontSize = Data.Faces[faceIx].Edges[i].LabelFontSize ?? 18;
        }

        RadioAudio.clip = AudioClips.FirstOrDefault(ac => ac.name == Data.Faces[faceIx].SongSnippet);
        Light.intensity = Data.LightIntensity;

        for (var i = 0; i < Data.Faces[faceIx].CarpetLength; i++)
        {
            Carpets[i].sharedMaterial = CarpetMaterials.First(m => m.name == "Carpet-" + Data.Faces[faceIx].CarpetColor);
            Carpets[i].transform.localPosition = new Vector3(.3f + .04f * (6 - Data.Faces[faceIx].CarpetLength) + .08f * i, 0, 0);
            Carpets[i].gameObject.SetActive(true);
        }
        for (var i = Data.Faces[faceIx].CarpetLength; i < Carpets.Length; i++)
            Carpets[i].gameObject.SetActive(false);

        if (setCamera)
            Camera.main.transform.Set(Data.CameraPositions[edgeIx], new Vector3(1, 1, 1));
    }
}

