#define TESTING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameMenuBehavior : MonoBehaviour
{
    public Button classicButton;
    public Button adventureButton;

    public GameObject testingButtons;
#if TESTING
    public Button deleteDataButton;
    public Button resetPrefsButton;
    public Button enableAllLevelsButton;
    public Button getAllCollectibles;
#endif

    public Button inventoryButton;

    public GameObject hintMessageCanvas;
    private HintMessageCanvasBehavior hintMessageCanvasScript;

    // Inventory popup
    public GameObject inventoryCanvasPrefab;
    private GameObject inventoryCanvas;
    private InventoryCanvasBehavior inventoryCanvasScript;

    // Notification for the inventory popup
    public GameObject notificationCanvasPrefab;
    private GameObject inventoryNotification;

    public GameObject levelChanger;
    private LevelChanger levelChangerScript;

    private NotificationManager notificationMgr;


    public void Awake()
    {
        // Initialize and get Firebase going
        FirebaseManager.InitializeFirebase();

        notificationMgr = NotificationManager.Instance();

        levelChangerScript = levelChanger.GetComponent<LevelChanger>();

        inventoryCanvas = Instantiate(inventoryCanvasPrefab);
        inventoryCanvasScript = inventoryCanvas.GetComponent<InventoryCanvasBehavior>();
        inventoryCanvasScript.inventoryDismissButton.onClick.AddListener(InventoryCanvasDismissed);
        inventoryCanvas.gameObject.SetActive(false);

        inventoryNotification = Instantiate(notificationCanvasPrefab);
        inventoryNotification.transform.SetParent(inventoryButton.transform, false);
        Vector3 iScale = inventoryNotification.transform.localScale;
        inventoryNotification.transform.localScale = new Vector3(iScale.x * 1.5f, iScale.y * 1.5f, iScale.z);
        inventoryNotification.gameObject.SetActive(false);

        hintMessageCanvasScript = hintMessageCanvas.GetComponent<HintMessageCanvasBehavior>();
        hintMessageCanvas.gameObject.SetActive(false);

        notificationMgr.Subscribe(NotificationManager.newEggNotificationId, inventoryNotification);
        notificationMgr.Subscribe(NotificationManager.canHatchNotificationId, inventoryNotification);
        notificationMgr.Subscribe(NotificationManager.firstTimeItemsNotificationId, inventoryNotification);
    }

    public void OnDestroy()
    {
        notificationMgr.Unsubscribe(NotificationManager.newEggNotificationId, inventoryNotification);
        notificationMgr.Unsubscribe(NotificationManager.canHatchNotificationId, inventoryNotification);
        notificationMgr.Unsubscribe(NotificationManager.firstTimeItemsNotificationId, inventoryNotification);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (ScreenCommon.GetOptimalDeviceOrientation() == ScreenCommon.PORTRAIT_DEVICE)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
        }
        else
        {
            Screen.orientation = ScreenOrientation.Landscape;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }

        classicButton.onClick.AddListener(ClassicButtonClicked);
        adventureButton.onClick.AddListener(AdventureButtonClicked);
        inventoryButton.onClick.AddListener(InventoryButtonClicked);

#if TESTING
        deleteDataButton.onClick.AddListener(DeleteDataButtonClicked);
        resetPrefsButton.onClick.AddListener(ResetPrefsButtonClicked);
        enableAllLevelsButton.onClick.AddListener(EnableAllLevelsButtonClicked);
        getAllCollectibles.onClick.AddListener(GetAllCollectibesButtonClicked);
#else
        // Hide all the testing buttons if we're not in testing mode
        Destroy(testingButtons);
#endif

        InventoryData id = InventoryData.Instance();
        LevelData ld = LevelData.Instance();
        if (false == PlayerPrefsCommon.GetMoreToCome() && ld.levelData.level5Complete)
        {
            string message = "Congratulations on completing the first five levels in Adventure mode!\n\n";
            message += "More adventures to come soon! Keep checking back for updates!";
            hintMessageCanvasScript.dismissHintButton.onClick.AddListener(DismissMoreToComePopup);
            hintMessageCanvasScript.ShowMessage(message);
        }
    }

    private void DismissMoreToComePopup()
    {
        PlayerPrefsCommon.SetMoreToCome(true);
        hintMessageCanvasScript.dismissHintButton.onClick.RemoveListener(DismissMoreToComePopup);
    }

    public void OnApplicationQuit()
    {
        notificationMgr.SaveUserData();
    }

    public void ClassicButtonClicked()
    {
        // Set the level number to 0 in level manager
        LevelManager.SetLevelNumber(0);

        levelChangerScript.FadeToScene("ClassicGameScene");
    }

    public void AdventureButtonClicked()
    {
        levelChangerScript.FadeToScene("LevelSelectionScene");
    }

    public void InventoryButtonClicked()
    {
        // Clear the notification for first items collected if it's showing
        notificationMgr.NotificationChange(NotificationManager.firstTimeItemsNotificationId,
                                           NotificationManager.CLEAR_NOTIFICATION);

        inventoryCanvas.gameObject.SetActive(true);
    }

    private void InventoryCanvasDismissed()
    {
        inventoryCanvas.gameObject.SetActive(false);
    }

#if TESTING
    // This function is defined in PlayerPrefsCommon
    public void ResetPrefsButtonClicked()
    {
        PlayerPrefsCommon.DeleteAllPreferences();
    }

    public void DeleteDataButtonClicked()
    {
        TestingCommon.DeleteAdventureData();
    }

    public void EnableAllLevelsButtonClicked()
    {
        TestingCommon.EnableAllLevels();
    }

    public void GetAllCollectibesButtonClicked()
    {
        TestingCommon.GetAllCollectibles();
    }
#endif
}
