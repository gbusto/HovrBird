using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGameModeBehavior : MonoBehaviour
{
    public Camera gameCamera;
    private CameraBehavior cameraScript;

    public GameObject bird;
    private NewBirdBehavior newBirdScript;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        Screen.orientation = ScreenOrientation.Landscape;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        cameraScript = gameCamera.GetComponent<CameraBehavior>();
        cameraScript.moveWithBird = true;

        newBirdScript = bird.GetComponent<NewBirdBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        Touch screenTap;
        bool mouseClick = false;

        if (Input.touchCount > 0 || ((mouseClick = Input.GetMouseButtonDown(0)) != false))
        {
            if (mouseClick)
            {
                Vector3 point = gameCamera.ScreenToWorldPoint(Input.mousePosition);

                newBirdScript.Flap(point.x);
            }
            else
            {
                screenTap = Input.GetTouch(0);
                if (screenTap.phase == TouchPhase.Began)
                {
                    Vector3 point = gameCamera.ScreenToWorldPoint(screenTap.position);
                    newBirdScript.Flap(point.x);
                }
            }
        }
    }
}
