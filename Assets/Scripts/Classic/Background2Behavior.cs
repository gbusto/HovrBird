using UnityEngine;

public class Background2Behavior : MonoBehaviour
{
    public bool shiftBackground = false;

    private void OnBecameInvisible()
    {
        // No longer in view of the camera, so shift this background
        shiftBackground = true;
    }
}
