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

    private int _faceIx;
    private int _edgeIx;
    private Coroutine _talkTextCoroutine;
    private Coroutine _radioCoroutine;
    private bool _interactionDisabled = false;

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Do Stuff/Set sphere")]
    public static void DoStuff()
    {
        var m = FindObjectOfType<FaceToFaceControl>();
        m.Sphere.transform.localPosition = Data.Midpoint;
    }
#endif

    private static Vector3 vec(double x, double y, double z)
    {
        return new Vector3((float) x, (float) y, (float) z);
    }

    void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
#endif

        _faceIx = Rnd.Range(0, Data.Faces.Length);
        _edgeIx = Rnd.Range(0, 5);

        Room.RadioMaterial.DisableKeyword("_EMISSION");
        Room.SetRoom(_faceIx, _edgeIx, setCamera: true);
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

                if (Data.Faces[_faceIx].Edges[edgeIx].Face == null)
                    Talk(Data.Faces[_faceIx].Edges[edgeIx].Label == null ? "That door is locked." : string.Format("That door is locked. The sign on the door says: {0}", Data.Faces[_faceIx].Edges[edgeIx].Label));
                else
                {
                    var newFaceIx = Data.Faces[_faceIx].Edges[edgeIx].Face.Value;
                    var newEdgeIx = new[] { 4, 2, 1, 3, 0 }[edgeIx];

                    var rotationParent = new GameObject();
                    rotationParent.transform.localPosition = Data.RotateRoomAbout[edgeIx];
                    rotationParent.transform.localRotation = Quaternion.identity;
                    rotationParent.transform.localScale = new Vector3(1, 1, 1);
                    var cameraDummy = new GameObject();
                    cameraDummy.transform.Set(Data.CameraPositions[newEdgeIx], new Vector3(1, 1, 1));
                    cameraDummy.transform.parent = rotationParent.transform;
                    var newRoom = Instantiate(Room);
                    newRoom.transform.parent = rotationParent.transform;
                    rotationParent.transform.localRotation = Quaternion.AngleAxis(Data.TiltAngle, Data.TiltRoomAbout[edgeIx]) * Quaternion.AngleAxis(Data.RotateRoomBy[edgeIx], Vector3.up);
                    newRoom.SetRoom(newFaceIx, newEdgeIx, setCamera: false);

                    StartCoroutine(WalkTo(Data.InCameraPositions[edgeIx]));
                    StartCoroutine(RestoreWall(Room.Walls[_edgeIx]));
                    StartCoroutine(OpenDoor(edgeIx));
                    StartCoroutine(DimLight(Room.Light));
                    StartCoroutine(RaiseLight(newRoom.Light));
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
                    Room.RadioMaterial.DisableKeyword("_EMISSION");
                    Room.RadioAudio.Stop();
                    Talk("That's enough music for today.");
                }
                else
                    _radioCoroutine = StartCoroutine(Radio());
            }
            else if (hit.transform.gameObject == Room.Box)
                Talk(string.Format("This box contains {0}.", Data.Faces[_faceIx].ItemInBox));
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
        _faceIx = newFaceIx;
        _edgeIx = newEdgeIx;
        _interactionDisabled = false;
    }

    private IEnumerator RaiseLight(Light light)
    {
        light.intensity = 0;
        yield return new WaitForSeconds(1.5f);
        var duration = 1.5f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            light.intensity = Mathf.Lerp(0, Data.LightIntensity, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        light.intensity = Data.LightIntensity;
    }

    private IEnumerator DimLight(Light light)
    {
        yield return new WaitForSeconds(1.5f);
        var duration = 1.5f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            light.intensity = Mathf.Lerp(Data.LightIntensity, 0, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        light.intensity = 0;
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
        Room.RadioMaterial.EnableKeyword("_EMISSION");
        Room.RadioAudio.Play();
        yield return new WaitUntil(() => !Room.RadioAudio.isPlaying);
        Room.RadioMaterial.DisableKeyword("_EMISSION");
        _radioCoroutine = null;
    }
}
