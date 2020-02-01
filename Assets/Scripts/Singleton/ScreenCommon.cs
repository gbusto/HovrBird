using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCommon : MonoBehaviour
{
    public const float ASPECT_RATIO_4_3 = (float)4 / 3; // 0.75, 1.3333
    public const float ASPECT_RATIO_3_2 = (float)3 / 2; // 0.667, 1.5
    public const float ASPECT_RATIO_5_3 = (float)5 / 3; // 0.6, 1.667
    public const float ASPECT_RATIO_16_9 = (float)16 / 9; // 0.5625, 1.778
    public const float ASPECT_RATIO_18_9 = (float)18 / 9; // 0.5, 2

    // Portrait devices are devices in which everything with the exception of playing
    // levels should be in portrait mode
    public const int PORTRAIT_DEVICE = 0x1;
    // Landscape devices are devices in which everything can be loaded in landscape mode
    public const int LANDSCAPE_DEVICE = 0x2;

    public static int optimalOrientation;

    public static int GetOptimalDeviceOrientation()
    {
        if (0 == optimalOrientation)
        {
            Resolution r = Screen.currentResolution;
            int tallSide = r.height;
            int shortSide = r.width;
            if (tallSide < shortSide)
            {
                int tmp = tallSide;
                tallSide = shortSide;
                shortSide = tmp;
            }
            float ar = (float)tallSide / shortSide;
            if (ar >= ASPECT_RATIO_5_3)
            {
                optimalOrientation = PORTRAIT_DEVICE;
            }
            else
            {
                optimalOrientation = LANDSCAPE_DEVICE;
            }
        }

        return optimalOrientation;
    }
}
