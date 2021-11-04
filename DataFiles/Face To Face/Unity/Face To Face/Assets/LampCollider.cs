using System.Linq;
using UnityEngine;

public class LampCollider : MonoBehaviour
{
    public RoomControl Room;

    void OnCollisionEnter(Collision collision)
    {
        if (Room.FFControl.Smashed[Room.FFControl.FaceIx])
            return;
        var cp = collision.contacts.Select(p => (ContactPoint?) p).FirstOrDefault(p => p.Value.otherCollider.gameObject == Room.Floor);
        if (cp != null)
            Room.Smash(cp.Value.point, gameObject);
    }
}
