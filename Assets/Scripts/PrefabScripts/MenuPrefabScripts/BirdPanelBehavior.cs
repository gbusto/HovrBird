using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BirdPanelBehavior : MonoBehaviour
{
    public RawImage eggHatchingDisplay;
    public VideoPlayer videoPlayer;
    public Sprite mysterySprite;
    public Sprite lockedEggSprite;
    public Sprite unlockedEggSprite;

    public Image birdImage;
    public Text birdNameText;
    public Button birdButton;
    public Text birdButtonText;

    public Canvas birdReqsCanvas;
    public Image req1Image;
    public Image req2Image;
    public Image req3Image;
    public Image req4Image;
    public Image req5Image;

    public Text req1Text;
    public Text req2Text;
    public Text req3Text;
    public Text req4Text;
    public Text req5Text;

    public VideoClip samHatchingVideoClip;
    public VideoClip nigelHatchingVideoClip;
    public VideoClip stevenHatchingVideoClip;

    public GameObject notificationCanvasPrefab;
    private GameObject buttonNotification;

    private uint birdId;
    private Sprite birdSprite;

    private List<(Image, Text)> birdReqsList;
    private InventoryData inventoryData;
    private bool configured;

    private GameObject parentObject;

    private NotificationManager notificationMgr;

    private bool videoPreparationComplete;

    /*
     * Different cases are:
     * 1. Mystery bird panel
     * 2. Locked egg panel; show the requirements canvas
     * 3. Unlocked egg panel; activate the button and change button text to Hatch
     * 4. Hatched bird panel; sprite, name, activate button
     */

    /*
     * Functions to add:
     * 1. Init for mystery
     * 2. Init for locked egg
     * 3. Init for unlocked egg
     * 4. Init for hatched bird
     * 5. Hatch button functionality
     * 6. Fly button functionality
     */

    private void Start()
    {
        videoPlayer.aspectRatio = VideoAspectRatio.FitVertically;
        videoPlayer.loopPointReached += VideoPlayEnded;
        videoPlayer.prepareCompleted += VideoCompletedPrepare;
        videoPlayer.Prepare();

        eggHatchingDisplay.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (null == birdReqsList && null == inventoryData)
        {
            birdReqsList = new List<(Image, Text)>
            {
                (req1Image, req1Text),
                (req2Image, req2Text),
                (req3Image, req3Text),
                (req4Image, req4Text),
                (req5Image, req5Text),
            };

            inventoryData = InventoryData.Instance();
        }

        if (configured)
        {
            RunInventoryCheck();
        }

        if (PlayerPrefsCommon.GetActiveBirdId() == birdId)
        {
            // Disable the play button if we are the currently active bird
            // and send a message to the inventory canvas so it knows who the active bird is
            FlyButtonClicked();
        }
    }

    public void OnDestroy()
    {
        notificationMgr.Unsubscribe(NotificationManager.canHatchNotificationId, buttonNotification);
    }

    private void VideoCompletedPrepare(VideoPlayer vp)
    {
        eggHatchingDisplay.texture = vp.texture;
        videoPreparationComplete = true;
    }

    private void VideoPlayEnded(VideoPlayer vp)
    {
        // Update the game to say the user has hatched the bird, spend the currency,
        // and save the updated collectible amounts
        inventoryData.HatchEggWithId(birdId);

        // Change the bird sprite
        birdImage.sprite = birdSprite;

        // Show the bird's name now
        birdNameText.gameObject.SetActive(true);

        // Change the button text to say Fly and check the functionality of the button
        birdButtonText.text = "Fly";
        birdButton.onClick.RemoveAllListeners();
        birdButton.onClick.AddListener(FlyButtonClicked);

        eggHatchingDisplay.gameObject.SetActive(false);

        // Rerun egg hatch checks on the other bird panels after spending currency
        // to hatch this egg
        parentObject.SendMessage("RunHatchChecks", birdId);
    }

    public void InitBirdPanel(uint birdId, Sprite birdSprite, string name,
                              List<(Sprite, int)> reqsConfig, Dictionary<string, int> reqsDict,
                              GameObject parentObject)
    {
        // XXX This code below here should really be called in Start()
        // but that doesn't get called until the script gets attached and the
        // object is activated for the first time
        notificationMgr = NotificationManager.Instance();

        buttonNotification = Instantiate(notificationCanvasPrefab);
        buttonNotification.transform.SetParent(birdButton.transform, false);
        Vector3 scale = buttonNotification.transform.localScale;
        buttonNotification.transform.localScale = new Vector3(scale.x * 0.5f, scale.y * 0.5f, scale.y);
        buttonNotification.gameObject.SetActive(false);
        // End of Start() code

        this.birdId = birdId;
        this.birdSprite = birdSprite;
        birdNameText.text = name;
        this.parentObject = parentObject;

        switch (birdId)
        {
            case InventoryData.ANGRY_KOKO_ID:
                // XXX Make a video clip for angry Koko hatching
                videoPlayer.clip = samHatchingVideoClip;
                break;

            case InventoryData.SAM_ID:
                videoPlayer.clip = samHatchingVideoClip;
                break;

            case InventoryData.ANGRY_SAM_ID:
                // XXX Make a video clip for angry Sam hatching
                videoPlayer.clip = samHatchingVideoClip;
                break;

            case InventoryData.NIGEL_ID:
                videoPlayer.clip = nigelHatchingVideoClip;
                break;

            case InventoryData.ANGRY_NIGEL_ID:
                // XXX Make a video clip for angry Nigel hatching
                videoPlayer.clip = nigelHatchingVideoClip;
                break;

            case InventoryData.STEVEN_ID:
                videoPlayer.clip = stevenHatchingVideoClip;
                break;

            case InventoryData.ANGRY_STEVEN_ID:
                // XXX Make a video clip for angry Steven hatching
                videoPlayer.clip = stevenHatchingVideoClip;
                break;

            default:
                videoPlayer.clip = null;
                break;
        }

        for (int i = 0; i < birdReqsList.Count; ++i)
        {
            if (i < reqsConfig.Count)
            {
                birdReqsList[i].Item1.sprite = reqsConfig[i].Item1;
                birdReqsList[i].Item2.text = reqsConfig[i].Item2.ToString();
            }
            else
            {
                birdReqsList[i].Item1.gameObject.SetActive(false);
                birdReqsList[i].Item2.gameObject.SetActive(false);
            }
        }

        birdNameText.gameObject.SetActive(false);
        birdButton.interactable = false;
        birdButton.gameObject.SetActive(false);
        birdReqsCanvas.gameObject.SetActive(false);

        configured = true;
    }

    public bool HasBeenConfigured()
    {
        return configured;
    }

    public void DeactivateBird()
    {
        /*
         * Enable the fly button again
         */

        birdButton.interactable = true;
    }

    public uint GetBirdId()
    {
        return birdId;
    }

    public void RunInventoryCheck()
    {
        if (inventoryData.HasEggHatched(birdId))
        {
            ConfigPanelHatchedBird();
        }
        else
        {
            if (inventoryData.IsBirdInInventory(birdId))
            {
                if (inventoryData.CanHatchEggWithId(birdId))
                {
                    notificationMgr.Subscribe(NotificationManager.canHatchNotificationId, buttonNotification);
                    ConfigPanelUnlockedEgg();
                }
                else
                {
                    ConfigPanelLockedEgg();
                }
            }
        }
    }

    private void ConfigPanelLockedEgg()
    {
        birdImage.sprite = lockedEggSprite;

        // Show the requirements canvas
        birdReqsCanvas.gameObject.SetActive(true);
        birdButton.onClick.RemoveAllListeners();
        birdButton.gameObject.SetActive(false);
    }

    private void ConfigPanelUnlockedEgg()
    {
        birdImage.sprite = unlockedEggSprite;

        // Show the button, make it interactable, and change text to Hatch
        birdButton.gameObject.SetActive(true);
        birdButton.interactable = true;
        birdButtonText.text = "Hatch";
        birdButton.onClick.AddListener(HatchButtonClicked);
    }

    private void ConfigPanelHatchedBird()
    {
        birdImage.sprite = birdSprite;

        // Show the bird image, show the button, change text to Fly, make button
        // interactable, set onclick listener
        birdNameText.gameObject.SetActive(true);
        birdButton.gameObject.SetActive(true);
        birdButton.interactable = true;
        birdButton.onClick.AddListener(FlyButtonClicked);
    }

    private void HatchButtonClicked()
    {
        notificationMgr.NotificationChange(NotificationManager.canHatchNotificationId,
                                           NotificationManager.CLEAR_NOTIFICATION);

        /*
         * 1. Change the bird sprite to the appropriate bird sprite
         * 2. Change button text to Fly
         * 3. Change the button onclick function
         */

        if (videoPreparationComplete && videoPlayer.isPrepared)
        {
            print("Video player is ready!");
            eggHatchingDisplay.gameObject.SetActive(true);
            videoPlayer.Play();
        }
        else
        {
            print("Video player isn't ready!");
        }
    }

    private void FlyButtonClicked()
    {
        /*
         * 1. Make the button non-interactable
         * 2. Set active bird in PlayerPrefs
         */

        birdButton.interactable = false;

        parentObject.SendMessage("SwitchActiveBird", gameObject);
    }
}
