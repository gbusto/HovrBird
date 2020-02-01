using UnityEngine;

public class Ground2Behavior : MonoBehaviour
{
    public bool shiftGround = false;

    void OnBecameInvisible()
    {
        shiftGround = true;

        // Change pipe object vertical positions
    }
}
