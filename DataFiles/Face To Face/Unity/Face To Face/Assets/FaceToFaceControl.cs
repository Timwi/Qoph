using System;
using System.Collections;
using System.Linq;
using Assets;
using UnityEngine;
using UnityEngine.UI;

using Rnd = UnityEngine.Random;

public class FaceToFaceControl : MonoBehaviour
{
    public RoomControl Room;
    public GameObject Sphere;
    public Text TalkText;
    public bool[] Smashed = new bool[24];

    public int FaceIx;
    private int _edgeIx;
    private Coroutine _talkTextCoroutine;
    private Coroutine _radioCoroutine;
    private bool _interactionDisabled = false;

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Do Stuff/Set camera &5")] public static void Camera0() { var t = FindObjectOfType<FaceToFaceControl>(); t.Room.SetRoom(t.FaceIx, 0, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &1")] public static void Camera1() { var t = FindObjectOfType<FaceToFaceControl>(); t.Room.SetRoom(t.FaceIx, 1, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &2")] public static void Camera2() { var t = FindObjectOfType<FaceToFaceControl>(); t.Room.SetRoom(t.FaceIx, 2, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &3")] public static void Camera3() { var t = FindObjectOfType<FaceToFaceControl>(); t.Room.SetRoom(t.FaceIx, 3, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set camera &4")] public static void Camera4() { var t = FindObjectOfType<FaceToFaceControl>(); t.Room.SetRoom(t.FaceIx, 4, setCamera: true); }
    [UnityEditor.MenuItem("Do Stuff/Set all walls &6")] public static void SetAllWalls() { var t = FindObjectOfType<FaceToFaceControl>(); t.Room.SetRoom(t.FaceIx, 4, setCamera: true, setAllWalls: true); }

    [UnityEditor.MenuItem("Do Stuff/Set objects &0")]
    public static void SetObjects()
    {
        var t = FindObjectOfType<FaceToFaceControl>();
        for (var ix = 0; ix < 5; ix++)
        {
            t.Room.Doors[ix].transform.Set(Data.DoorPositions[ix], new Vector3(1, 1, 1));
            t.Room.CyanNumbers1[ix].transform.Set(Data.CyanNumbers1Positions[ix], new Vector3(.013f, .013f, .013f));
            t.Room.CyanNumbers2[ix].transform.Set(Data.CyanNumbers2Positions[ix], new Vector3(.013f, .013f, .013f));
        }
    }
#endif

    void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
#endif

        do
        {
            FaceIx = Rnd.Range(0, Data.Faces.Length);
            _edgeIx = Rnd.Range(0, 5);
        }
        while (Data.Faces[FaceIx].Edges[_edgeIx].Face == null);

        Room.FFControl = this;
        Room.SetRoom(FaceIx, _edgeIx, setCamera: true, smashed: Smashed[FaceIx]);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_interactionDisabled)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit))
                return;

            if (Room.Doors.Contains(hit.transform.gameObject))
            {
                var edgeIx = Array.IndexOf(Room.Doors, hit.transform.gameObject);

                if (Data.Faces[FaceIx].Edges[edgeIx].Face == null)
                    Talk(Data.Faces[FaceIx].Edges[edgeIx].Label == null ? "That door is locked." : string.Format("That door is locked. The sign on the door says: {0}", Data.Faces[FaceIx].Edges[edgeIx].Label.Replace("\n", " ")));
                else
                {
                    var newFaceIx = Data.Faces[FaceIx].Edges[edgeIx].Face.Value;
                    var newEdgeIx = new[] { 4, 2, 1, 3, 0 }[edgeIx];

                    var rotationParent = new GameObject();
                    rotationParent.transform.localPosition = Data.RotateRoomAbout[edgeIx];
                    rotationParent.transform.localRotation = Quaternion.identity;
                    rotationParent.transform.localScale = new Vector3(1, 1, 1);
                    var cameraDummy = new GameObject();
                    cameraDummy.transform.Set(Data.CameraPositions[newEdgeIx], new Vector3(1, 1, 1));
                    cameraDummy.transform.parent = rotationParent.transform;
                    var newRoom = Instantiate(Room);
                    newRoom.FFControl = this;
                    newRoom.transform.parent = rotationParent.transform;
                    rotationParent.transform.localRotation = Quaternion.AngleAxis(Data.TiltAngle, Data.TiltRoomAbout[edgeIx]) * Quaternion.AngleAxis(Data.RotateRoomBy[edgeIx], Vector3.up);
                    newRoom.SetRoom(newFaceIx, newEdgeIx, setCamera: false);

                    StartCoroutine(WalkTo(Data.InCameraPositions[edgeIx]));
                    StartCoroutine(RestoreWall(Room.Walls[_edgeIx]));
                    StartCoroutine(OpenDoor(edgeIx));
                    StartCoroutine(DimLights(Room.Lights));
                    StartCoroutine(RaiseLights(newRoom.Lights));
                    StartCoroutine(StraightenCameraAndWallThenSetRoom(rotationParent, cameraDummy, Room.Walls[edgeIx], newRoom.gameObject, edgeIx, newFaceIx, newEdgeIx));
                    _interactionDisabled = true;
                }
            }
            else if (hit.transform.gameObject == Room.Radio)
            {
                if (_radioCoroutine != null)
                    StopCoroutine(_radioCoroutine);
                if (Room.RadioAudio.isPlaying)
                {
                    Room.RadioLight.sharedMaterial = Room.RadioLightOff;
                    Room.RadioAudio.Stop();
                    Talk("That's enough music for today.");
                }
                else
                    _radioCoroutine = StartCoroutine(Radio());
            }
            else if (hit.transform.gameObject == Room.Box)
                Talk(string.Format("This box contains {0}.", Data.Faces[FaceIx].ItemInBox));
            else if (Room.Carpets.Any(c => c.gameObject == hit.transform.gameObject))
                Talk(string.Format("What a nice {0} carpet design!", Data.Faces[FaceIx].CarpetColor));
            else if (Room.LampBroModels.Contains(hit.transform.gameObject) && !Smashed[FaceIx])
                hit.transform.gameObject.GetComponent<Rigidbody>().AddForce(5 * hit.transform.position);
        }
    }

    private IEnumerator RestoreWall(GameObject wall)
    {
        yield return new WaitForSeconds(.75f);
        wall.SetActive(true);
    }

    private IEnumerator StraightenCameraAndWallThenSetRoom(GameObject rotationParent, GameObject cameraDummy, GameObject wall, GameObject newRoom, int edgeIx, int newFaceIx, int newEdgeIx)
    {
        yield return new WaitForSeconds(2.1f);

        Camera.main.transform.parent = rotationParent.transform;
        Room.transform.parent = rotationParent.transform;

        var wallSizer = new GameObject();
        wallSizer.transform.parent = wall.transform.parent;
        wallSizer.transform.localPosition = Data.WallPositions[edgeIx];
        wallSizer.transform.localScale = new Vector3(1, 1, 1);
        wallSizer.transform.localRotation = Quaternion.identity;
        wall.transform.parent = wallSizer.transform;

        var cameraOldPos = Camera.main.transform.localPosition;
        var cameraOldRot = Camera.main.transform.localRotation;
        var rotParOldPos = rotationParent.transform.localPosition;
        var rotParOldRot = rotationParent.transform.localRotation;
        var duration = 1f;
        var elapsed = 0f;
        const float wallFactor = 3.5f;
        while (elapsed < duration)
        {
            Camera.main.transform.localPosition = Vector3.Lerp(cameraOldPos, cameraDummy.transform.localPosition, Easing.InOutQuad(elapsed, 0, 1, duration));
            Camera.main.transform.localRotation = Quaternion.Slerp(cameraOldRot, cameraDummy.transform.localRotation, Easing.InOutQuad(elapsed, 0, 1, duration));

            rotationParent.transform.localPosition = Vector3.Lerp(rotParOldPos, Data.RotateRoomAbout[edgeIx], Easing.InOutQuad(elapsed, 0, 1, duration));
            rotationParent.transform.localRotation = Quaternion.Slerp(rotParOldRot, Quaternion.identity, Easing.InOutQuad(elapsed, 0, 1, duration));

            const float wallSizeDelay = .5f;
            wallSizer.transform.localScale = new Vector3(1, 1, 1) * (elapsed < wallSizeDelay ? 1 : Easing.InQuad(elapsed - wallSizeDelay, 1, wallFactor, duration - wallSizeDelay));

            yield return null;
            elapsed += Time.deltaTime;
        }

        wall.transform.parent = Room.transform;
        wall.transform.localPosition = new Vector3(0, 0, 0);
        wall.transform.localScale = new Vector3(1, 1, 1);
        wall.transform.localRotation = Quaternion.identity;

        Camera.main.transform.parent = null;
        Room.transform.parent = null;
        Room.transform.localPosition = new Vector3(0, 0, 0);
        Room.transform.localScale = new Vector3(1, 1, 1);
        Room.transform.localRotation = Quaternion.identity;

        Destroy(cameraDummy);
        Destroy(newRoom);
        Destroy(rotationParent);
        Destroy(wallSizer);
        Room.gameObject.SetActive(true);
        Room.SetRoom(newFaceIx, newEdgeIx, setCamera: true);
        FaceIx = newFaceIx;
        _edgeIx = newEdgeIx;
        _interactionDisabled = false;
    }

    private IEnumerator RaiseLights(Light[] lights)
    {
        for (var i = 0; i < lights.Length; i++)
            lights[i].intensity = 0;
        yield return new WaitForSeconds(1.5f);
        var duration = 1.5f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            for (var i = 0; i < lights.Length; i++)
                lights[i].intensity = Mathf.Lerp(0, Data.LightIntensities[i], elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        for (var i = 0; i < lights.Length; i++)
            lights[i].intensity = Data.LightIntensities[i];
    }

    private IEnumerator DimLights(Light[] lights)
    {
        yield return new WaitForSeconds(1.5f);
        var duration = 1.5f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            for (var i = 0; i < lights.Length; i++)
                lights[i].intensity = Mathf.Lerp(Data.LightIntensities[i], 0, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        for (var i = 0; i < lights.Length; i++)
            lights[i].intensity = 0;
    }

    private static Vector3 Bézier(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, float t)
    {
        return Mathf.Pow(1 - t, 3) * start + 3 * Mathf.Pow(1 - t, 2) * t * control1 + 3 * (1 - t) * t * t * control2 + Mathf.Pow(t, 3) * end;
    }

    private IEnumerator WalkTo(PosAndDir inCameraPos)
    {
        var oldPos = Camera.main.transform.localPosition;
        var oldRot = Camera.main.transform.localRotation;
        var newPos = inCameraPos.From;
        var newRot = Quaternion.LookRotation(inCameraPos.To - inCameraPos.From);
        var duration = 2f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            Camera.main.transform.localPosition = Bézier(oldPos, Data.Midpoint, Data.Midpoint, newPos, Easing.InOutQuad(elapsed, 0, 1, duration));
            Camera.main.transform.localRotation = Quaternion.Slerp(oldRot, newRot, elapsed < duration * .9f ? Easing.InOutQuad(elapsed, 0, 1, duration * .9f) : 1);
            yield return null;
            elapsed += Time.deltaTime;
        }
        Camera.main.transform.localPosition = newPos;
        Camera.main.transform.localRotation = newRot;
    }

    private IEnumerator OpenDoor(int ix)
    {
        yield return new WaitForSeconds(1.5f);
        var oldRot = Room.Doors[ix].transform.localEulerAngles.y;
        var duration = 1f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            Room.Doors[ix].transform.localEulerAngles = new Vector3(0, oldRot - Easing.InOutQuad(elapsed, 0, 130, duration), 0);
            yield return null;
            elapsed += Time.deltaTime;
        }
        Room.Doors[ix].transform.localEulerAngles = new Vector3(0, oldRot - 130, 0);
    }

    private void Talk(string text)
    {
        if (_talkTextCoroutine != null)
            StopCoroutine(_talkTextCoroutine);
        _talkTextCoroutine = StartCoroutine(ShowTalkText(text));
    }

    private IEnumerator ShowTalkText(string text)
    {
        TalkText.text = text;
        yield return new WaitForSeconds(3f);
        TalkText.text = "";
        _talkTextCoroutine = null;
    }

    private IEnumerator Radio()
    {
        Talk("Let's put on some music!");
        Room.RadioLight.sharedMaterial = Room.RadioLightOn;
        Room.RadioAudio.Play();
        yield return new WaitUntil(() => !Room.RadioAudio.isPlaying);
        Room.RadioLight.sharedMaterial = Room.RadioLightOff;
        _radioCoroutine = null;
    }
}
