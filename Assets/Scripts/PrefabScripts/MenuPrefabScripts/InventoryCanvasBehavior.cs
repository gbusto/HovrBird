using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class InventoryCanvasBehavior : MonoBehaviour
{
    public GameObject birdPanelPrefab;
    public Button inventoryDismissButton;

    public Button itemsTabButton;
    public Button birdTabButton;

    public RectTransform birdScrollViewContent;
    public Canvas nestedInventoryCanvas;
    public Canvas nestedBirdCanvas;

    public Sprite heartSprite;
    public Sprite coinSprite;
    public Sprite bananaSprite;
    public Sprite blueberrySprite;
    public Sprite strawberrySprite;

    // Collectible inventory stuff
    public Text lifeCountText;
    public Text coinCountText;
    public Text bananaCountText;
    public Text blueberriesCountText;
    public Text strawberryCountText;

    public Sprite kokoSprite;
    public Sprite angryKokoSprite;
    public Sprite samSprite;
    public Sprite angrySamSprite;
    public Sprite nigelSprite;
    public Sprite angryNigelSprite;
    public Sprite stevenSprite;
    public Sprite angryStevenSprite;

    public GameObject notificationCanvasPrefab;
    private GameObject itemsNotification;
    private GameObject birdsNotification;

    public const float panelWidth = 300;

    private InventoryData inventoryData;

    private GameObject activeBirdPanel;

    private Color originalButtonColor;

    private NotificationManager notificationMgr;

    private BirdPanelBehavior[] birdPanelScripts;

    // XXX The code in this function should really be called in Start()
    // but that doesn't get called until the script gets attached and the
    // object is activated for the first time
    private void Initialize()
    {
        notificationMgr = NotificationManager.Instance();

        itemsTabButton.onClick.AddListener(ItemsTabButtonClicked);
        birdTabButton.onClick.AddListener(BirdTabButtonClicked);

        itemsNotification = Instantiate(notificationCanvasPrefab);
        itemsNotification.transform.SetParent(itemsTabButton.transform, false);
        Vector3 iScale = itemsNotification.transform.localScale;
        itemsNotification.transform.localScale = new Vector3(iScale.x * 0.5f, iScale.y * 0.5f, iScale.z);
        itemsNotification.gameObject.SetActive(false);

        birdsNotification = Instantiate(notificationCanvasPrefab);
        birdsNotification.transform.SetParent(birdTabButton.transform, false);
        Vector3 bScale = birdsNotification.transform.localScale;
        birdsNotification.transform.localScale = new Vector3(bScale.x * 0.5f, bScale.y * 0.5f, bScale.z);
        birdsNotification.gameObject.SetActive(false);

        notificationMgr.Subscribe(NotificationManager.newEggNotificationId, birdsNotification);
        notificationMgr.Subscribe(NotificationManager.canHatchNotificationId, birdsNotification);

        birdPanelScripts = new BirdPanelBehavior[InventoryData.BIRDS.Length];
    }

    private void OnEnable()
    {
        // Update the count for the items
        if (null == inventoryData)
        {
            inventoryData = InventoryData.Instance();
            inventoryDismissButton.onClick.AddListener(InventoryDismissButtonClicked);

            Initialize();

            for (int i = 0; i < InventoryData.BIRDS.Length; ++i)
            {
                /*
                 * Bird panel position should be base panel prefab transform plus
                 * bird panel size.width
                 */
                uint birdId = InventoryData.BIRDS[i];
                Sprite sprite = GetSpriteForId(birdId);
                string birdName = GetNameForId(birdId);
                List<(Sprite, int)> reqsSpriteList = GetReqSpriteListForId(birdId);
                Dictionary<string, int> reqDict = InventoryData.GetRequirementForId(birdId);
                // Instantiate a new panel
                GameObject birdPanel = Instantiate(birdPanelPrefab);
                // Set it as a child of the scroll view content
                birdPanel.transform.SetParent(birdScrollViewContent, false);
                BirdPanelBehavior birdPanelScript = birdPanel.GetComponent<BirdPanelBehavior>();
                birdPanelScript.InitBirdPanel(birdId, sprite, birdName, reqsSpriteList, reqDict, gameObject);
                birdPanelScripts[i] = birdPanelScript;

                // Properly position the bird panel
                Vector3 pos = birdPanel.transform.localPosition;
                birdPanel.transform.localPosition = new Vector3(pos.x + (panelWidth * i), pos.y, pos.z);
            }
        }

        originalButtonColor = birdTabButton.image.color;

        ItemsTabButtonClicked();
    }

    public void OnDestroy()
    {
        notificationMgr.Unsubscribe(NotificationManager.newEggNotificationId, birdsNotification);
        notificationMgr.Unsubscribe(NotificationManager.canHatchNotificationId, birdsNotification);
    }

    public void InventoryDismissButtonClicked()
    {
        gameObject.SetActive(false);
    }

    public void SwitchActiveBird(GameObject birdPanel)
    {
        // XXX This can be improved using the birdPanelScripts array in this file now

        if (null == activeBirdPanel)
        {
            activeBirdPanel = birdPanel;
        }

        BirdPanelBehavior newScript = birdPanel.GetComponent<BirdPanelBehavior>();
        uint newBirdId = newScript.GetBirdId();

        BirdPanelBehavior currentScript = activeBirdPanel.GetComponent<BirdPanelBehavior>();
        uint currentBirdId = currentScript.GetBirdId();

        if (newBirdId != currentBirdId)
        {
            PlayerPrefsCommon.SetActiveBirdId(newBirdId);

            // When a Fly button is clicked to switch birds
            activeBirdPanel.SendMessage("DeactivateBird");

            activeBirdPanel = birdPanel;
        }
    }

    public void RunHatchChecks(uint birdId)
    {
        for (int i = 0; i < birdPanelScripts.Length; ++i)
        {
            if (birdPanelScripts[i].GetBirdId() != birdId)
            {
                birdPanelScripts[i].RunInventoryCheck();
            }
        }
    }

    private void ItemsTabButtonClicked()
    {
        ColorBlock colors = itemsTabButton.colors;
        colors.selectedColor = itemsTabButton.colors.pressedColor;
        itemsTabButton.colors = colors;
        birdTabButton.image.color = originalButtonColor;

        lifeCountText.text = "x" + inventoryData.livesCount;
        coinCountText.text = "x" + inventoryData.collectibleDict[InventoryData.coinKey].ToString();
        bananaCountText.text = "x" + inventoryData.collectibleDict[InventoryData.bananaKey].ToString();
        blueberriesCountText.text = "x" + inventoryData.collectibleDict[InventoryData.blueberryKey].ToString();
        strawberryCountText.text = "x" + inventoryData.collectibleDict[InventoryData.strawberryKey].ToString();

        nestedInventoryCanvas.gameObject.SetActive(true);
        nestedBirdCanvas.gameObject.SetActive(false);
    }

    private void BirdTabButtonClicked()
    {
        // Clear bird button notifications if they're still there
        notificationMgr.NotificationChange(NotificationManager.newEggNotificationId,
                                           NotificationManager.CLEAR_NOTIFICATION);

        ColorBlock colors = birdTabButton.colors;
        colors.selectedColor = birdTabButton.colors.pressedColor;
        birdTabButton.colors = colors;
        itemsTabButton.image.color = originalButtonColor;

        nestedInventoryCanvas.gameObject.SetActive(false);
        nestedBirdCanvas.gameObject.SetActive(true);
    }

    private Sprite GetSpriteForId(uint id)
    {
        switch (id)
        {
            case InventoryData.KOKO_ID:
                return kokoSprite;

            case InventoryData.ANGRY_KOKO_ID:
                return angryKokoSprite;

            case InventoryData.SAM_ID:
                return samSprite;

            case InventoryData.ANGRY_SAM_ID:
                return angrySamSprite;

            case InventoryData.NIGEL_ID:
                return nigelSprite;

            case InventoryData.ANGRY_NIGEL_ID:
                return angryNigelSprite;

            case InventoryData.STEVEN_ID:
                return stevenSprite;

            case InventoryData.ANGRY_STEVEN_ID:
                return angryStevenSprite;
        }

        return null;
    }

    private string GetNameForId(uint id)
    {
        switch (id)
        {
            case InventoryData.KOKO_ID:
                return "Koko";

            case InventoryData.ANGRY_KOKO_ID:
                return "Angry Koko";

            case InventoryData.SAM_ID:
                return "Sam";

            case InventoryData.ANGRY_SAM_ID:
                return "Angry Sam";

            case InventoryData.NIGEL_ID:
                return "Nigel";

            case InventoryData.ANGRY_NIGEL_ID:
                return "Angry Nigel";

            case InventoryData.STEVEN_ID:
                return "Steven";

            case InventoryData.ANGRY_STEVEN_ID:
                return "Angry Steven";
        }

        return "";
    }

    private List<(Sprite, int)> GetReqSpriteListForId(uint id)
    {
        List<(Sprite, int)> spriteReqList = new List<(Sprite, int)>();
        Dictionary<string, int> reqDict = InventoryData.GetRequirementForId(id);

        foreach (KeyValuePair<string, int> pair in reqDict)
        {
            Sprite s = GetCollectibleSpriteForKey(pair.Key);
            spriteReqList.Add((s, pair.Value));
        }
        return spriteReqList;
    }

    private Sprite GetCollectibleSpriteForKey(string key)
    {
        if (key == InventoryData.coinKey)
        {
            return coinSprite;
        }
        if (key == InventoryData.bananaKey)
        {
            return bananaSprite;
        }
        if (key == InventoryData.blueberryKey)
        {
            return blueberrySprite;
        }
        if (key == InventoryData.strawberryKey)
        {
            return strawberrySprite;
        }

        return coinSprite;
    }
}
