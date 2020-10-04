using System;
using System.Collections;
using System.Collections.Generic;
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

    [UnityEditor.MenuItem("Do Stuff/Set sphere")]
    public static void DoStuff()
    {
        var m = FindObjectOfType<FaceToFaceControl>();
        m.Sphere.transform.localPosition = Data.Midpoint;
    }

    private static Vector3 vec(double x, double y, double z)
    {
        return new Vector3((float) x, (float) y, (float) z);
    }

    void Start()
    {
        _faceIx = Rnd.Range(0, Data.Faces.Length);
        _edgeIx = Rnd.Range(0, 5);

        Room.RadioMaterial.DisableKeyword("_EMISSION");
        Room.SetRoom(_faceIx, _edgeIx);
        Camera.main.transform.Set(Data.CameraPositions[_edgeIx], new Vector3(1, 1, 1));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
                    var newRoom = Instantiate(Room);
                    rotationParent.transform.localPosition = Data.RotateRoomAbout[edgeIx];
                    rotationParent.transform.localRotation = Quaternion.identity;
                    newRoom.transform.parent = rotationParent.transform;
                    rotationParent.transform.localRotation = Quaternion.AngleAxis(Data.TiltAngle, Data.TiltRoomAbout[edgeIx]) * Quaternion.AngleAxis(Data.RotateRoomBy[edgeIx], Vector3.up);
                    newRoom.transform.parent = null;
                    newRoom.SetRoom(newFaceIx, newEdgeIx);
                    newRoom.Light.intensity = 0;
                    Destroy(rotationParent);

                    StartCoroutine(WalkToRoom(newRoom, newFaceIx, newEdgeIx, Data.InCameraPositions[edgeIx]));
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

    private static Vector3 Bézier(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, float t)
    {
        return Mathf.Pow(1 - t, 3) * start + 3 * Mathf.Pow(1 - t, 2) * t * control1 + 3 * (1 - t) * t * t * control2 + Mathf.Pow(t, 3) * end;
    }

    private IEnumerator WalkToRoom(RoomControl newRoom, int newFaceIx, int newEdgeIx, PosAndDir inCameraPos)
    {
        var oldPos = Camera.main.transform.localPosition;
        var oldRot = Camera.main.transform.localRotation;
        var newPos = inCameraPos.From;
        var newRot = Quaternion.LookRotation(inCameraPos.To - inCameraPos.From);
        var duration = 2f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            Camera.main.transform.localPosition = Bézier(oldPos, Data.Midpoint, Data.Midpoint, newPos, elapsed / duration);
            Camera.main.transform.localRotation = Quaternion.Slerp(oldRot, newRot, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        Camera.main.transform.localPosition = newPos;
        Camera.main.transform.localRotation = newRot;
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
