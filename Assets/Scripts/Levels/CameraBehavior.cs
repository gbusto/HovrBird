using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public GameObject background;
    private Renderer backgroundRenderer;
    private Camera mainCam;

    // This should not be initialized outside of this script!
    public GameObject bird;

    public bool moveWithBird;

    // Start is called before the first frame update
    void Start()
    {
        backgroundRenderer = background.GetComponent<Renderer>();
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveWithBird)
        {
            Vector3 pos = bird.transform.position;
            Vector3 thisPos = transform.position;
            float cameraHalfHeight = mainCam.orthographicSize;
            float skyMinY = backgroundRenderer.bounds.min.y;
            float skyMaxY = backgroundRenderer.bounds.max.y;
            // Don't adjust the camera if it's top or bottom edge will exceed
            // the background's top or bottom edge
            if (((pos.y + cameraHalfHeight) < skyMaxY) && ((pos.y - cameraHalfHeight) > skyMinY))
            {
                transform.position = new Vector3(thisPos.x, pos.y, thisPos.z);
            }
        }
    }
}
