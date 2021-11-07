using System.Linq;
using UnityEngine;

public class LampCollider : MonoBehaviour
{
    public RoomControl Room;
    public bool Smashed;

    void OnCollisionEnter(Collision collision)
    {
        if (Smashed)
            return;
        var cp = collision.contacts.Select(p => (ContactPoint?) p).FirstOrDefault(p => p.Value.otherCollider.gameObject == Room.Floor);
        if (cp != null)
        {
            Smashed = true;
            var p = cp.Value.point;
            p.y += .1f;
            Room.Smash(p, gameObject);
        }
    }
}
