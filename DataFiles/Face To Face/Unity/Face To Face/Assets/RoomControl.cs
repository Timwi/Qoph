using System.Collections;
using System.Linq;
using Assets;
using UnityEngine;

public partial class RoomControl : MonoBehaviour
{
    public AudioClip[] AudioClips;
    public AudioClip SmashClip;
    public GameObject[] Walls;
    public TextMesh[] CyanNumbers1;
    public TextMesh[] CyanNumbers2;
    public GameObject[] DoorSigns;
    public TextMesh[] DoorSignTexts;
    public GameObject[] Doors;
    public GameObject Box;
    public GameObject Radio;
    public AudioSource RadioAudio;
    public AudioSource SmashAudio;
    public MeshRenderer RadioLight;
    public Material RadioLightOn;
    public Material RadioLightOff;
    public Light[] Lights;
    public float[] DoorRotationsY;
    public Material[] CarpetMaterials;
    public MeshRenderer[] Carpets;
    public GameObject[] LampBroModels;
    public bool[] OwnLamp;
    public GameObject NormalLampStand;
    public GameObject Floor;
    public GameObject Bubble;
    public FaceToFaceControl FFControl;

    public void SetRoom(int faceIx, int edgeIx, bool setCamera, bool setAllWalls = false)
    {
        foreach (var wall in Walls)
            wall.SetActive(true);
        if (!setAllWalls)
            Walls[edgeIx].SetActive(false);

        for (var i = 0; i < 5; i++)
        {
            CyanNumbers1[i].gameObject.SetActive(Data.Faces[faceIx].Edges[i].CyanNumber != null);
            CyanNumbers1[i].text = Data.Faces[faceIx].Edges[i].CyanNumber.ToString();
            CyanNumbers2[i].gameObject.SetActive(Data.Faces[faceIx].Edges[i].CyanNumber != null);
            CyanNumbers2[i].text = Data.Faces[faceIx].Edges[i].CyanNumber.ToString();
            Doors[i].transform.localEulerAngles = new Vector3(0, DoorRotationsY[i], 0);
            DoorSigns[i].SetActive(Data.Faces[faceIx].Edges[i].Label != null);
            DoorSignTexts[i].text = Data.Faces[faceIx].Edges[i].Label ?? "";
            DoorSignTexts[i].fontSize = Data.Faces[faceIx].Edges[i].LabelFontSize ?? 18;
        }

        for (var i = 0; i < LampBroModels.Length; i++)
            if (LampBroModels[i] != null)
            {
                foreach (var rb in LampBroModels[i].GetComponentsInChildren<Rigidbody>())
                    Destroy(rb);
                LampBroModels[i].gameObject.SetActive(Data.Faces[faceIx].LampBro == i);
                if (Data.Faces[faceIx].LampBro == i)
                    foreach (var lc in LampBroModels[i].GetComponentsInChildren<LampCollider>())
                        lc.gameObject.SetActive(!lc.Smashed);
            }
        NormalLampStand.gameObject.SetActive(!OwnLamp[Data.Faces[faceIx].LampBro]);

        RadioAudio.clip = AudioClips[faceIx];
        for (var i = 0; i < Lights.Length; i++)
            Lights[i].intensity = Data.LightIntensities[i];

        for (var i = 0; i < Data.Faces[faceIx].CarpetLength; i++)
        {
            Carpets[i].sharedMaterial = CarpetMaterials.First(m => m.name == "Carpet-" + Data.Faces[faceIx].CarpetColor);
            Carpets[i].transform.localPosition = new Vector3(.3f + .04f * (6 - Data.Faces[faceIx].CarpetLength) + .08f * i, 0, 0);
            Carpets[i].gameObject.SetActive(true);
        }
        for (var i = Data.Faces[faceIx].CarpetLength; i < Carpets.Length; i++)
            Carpets[i].gameObject.SetActive(false);

        RadioLight.sharedMaterial = RadioLightOff;

        if (setCamera)
            Camera.main.transform.Set(Data.CameraPositions[edgeIx], new Vector3(1, 1, 1));
    }

    public void Smash(Vector3 contactPoint, GameObject lamp)
    {
        Bubble.SetActive(true);
        Bubble.transform.position = Vector3.Lerp(contactPoint, Camera.main.transform.position, .5f);
        Bubble.transform.rotation = Quaternion.LookRotation(contactPoint - Camera.main.transform.position, Vector3.up);
        StartCoroutine(SmashAnimation(lamp));
        SmashAudio.Play();
    }

    private IEnumerator SmashAnimation(GameObject lamp)
    {
        var duration = .4f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            Bubble.transform.localScale = new Vector3(1, 1, 1) * BackOut(elapsed, 0, 0.05f, duration, 2);
            yield return null;
            elapsed += Time.deltaTime;
        }
        Bubble.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        lamp.SetActive(false);

        duration = .25f;
        elapsed = 0f;
        while (elapsed < duration)
        {
            Bubble.transform.localScale = new Vector3(1, 1, 1) * Mathf.Lerp(0.05f, 0, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        Bubble.SetActive(false);
    }

    private static float BackOut(float time, float start, float end, float duration, float overshoot = 1)
    {
        var t = time / duration - 1f;
        var val = t * t * ((t + 1) * overshoot + t) + 1;
        return (end - start) * val + start;
    }
}

