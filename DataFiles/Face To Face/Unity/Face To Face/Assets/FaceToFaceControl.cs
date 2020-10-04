using System;
using System.Linq;
using UnityEngine;

using Rnd = UnityEngine.Random;

public class FaceToFaceControl : MonoBehaviour
{
    public RoomControl Room;

    void Start()
    {
    }

    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        RaycastHit hit;
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        if (Physics.Raycast(ray, out hit) && _doors.Contains(hit.transform.parent.gameObject))
    //        {
    //            Debug.LogFormat(@"You clicked on door #{0}", Array.IndexOf(_doors, hit.transform.parent.gameObject));
    //        }
    //    }
    //}

    //private void SetWall(int wallIx)
    //{
    //    Walls[wallIx].SetActive(true);
    //    wallIx = (wallIx + 1) % cameraPositions.Length;
    //    Camera.main.transform.position = cameraPositions[wallIx].From;
    //    Camera.main.transform.rotation = Quaternion.LookRotation(cameraPositions[wallIx].To - cameraPositions[wallIx].From);
    //    Walls[wallIx].SetActive(false);
    //}
}
