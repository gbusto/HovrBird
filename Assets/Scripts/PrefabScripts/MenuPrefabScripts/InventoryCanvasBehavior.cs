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

    public Sprite coinSprite;
    public Sprite bananaSprite;
    public Sprite blueberrySprite;
    public Sprite strawberrySprite;

    // Collectible inventory stuff
    public Text coinCountText;
    public Text bananaCountText;
    public Text blueberriesCountText;
    public Text strawberryCountText;

    public Sprite kokoSprite;
    public Sprite samSprite;
    public Sprite nigelSprite;
    public Sprite stevenSprite;

    public GameObject notificationCanvasPrefab;
    private GameObject itemsNotification;
    private GameObject birdsNotification;

    public const float panelWidth = 300;

    private InventoryData inventoryData;

    private GameObject activeBirdPanel;

    private Color originalButtonColor;

    private NotificationManager notificationMgr;

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

    private void ItemsTabButtonClicked()
    {
        ColorBlock colors = itemsTabButton.colors;
        colors.selectedColor = itemsTabButton.colors.pressedColor;
        itemsTabButton.colors = colors;
        birdTabButton.image.color = originalButtonColor;

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

            case InventoryData.SAM_ID:
                return samSprite;

            case InventoryData.NIGEL_ID:
                return nigelSprite;

            case InventoryData.STEVEN_ID:
                return stevenSprite;
        }

        return null;
    }

    private string GetNameForId(uint id)
    {
        switch (id)
        {
            case InventoryData.KOKO_ID:
                return "Koko";

            case InventoryData.SAM_ID:
                return "Sam";

            case InventoryData.NIGEL_ID:
                return "Nigel";

            case InventoryData.STEVEN_ID:
                return "Steven";
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

        /* XXX REMOVE ME
        switch (id)
        {
            case InventoryData.SAM_ID:
                return new List<(Sprite, int)>()
                {
                    (coinSprite, InventoryData.samRequirements[InventoryData.coinKey]),
                    (bananaSprite, InventoryData.samRequirements[InventoryData.bananaKey]),
                    (blueberrySprite, InventoryData.samRequirements[InventoryData.blueberryKey]),
                    (strawberrySprite, InventoryData.samRequirements[InventoryData.strawberryKey])
                };

            case InventoryData.NIGEL_ID:
                return new List<(Sprite, int)>()
                {
                    (coinSprite, InventoryData.nigelRequirements[InventoryData.coinKey]),
                    (bananaSprite, InventoryData.nigelRequirements[InventoryData.bananaKey]),
                    (blueberrySprite, InventoryData.nigelRequirements[InventoryData.blueberryKey]),
                    (strawberrySprite, InventoryData.nigelRequirements[InventoryData.strawberryKey])
                };

            case InventoryData.STEVEN_ID:
                return new List<(Sprite, int)>()
                {
                    (coinSprite, InventoryData.stevenRequirements[InventoryData.coinKey]),
                    (bananaSprite, InventoryData.stevenRequirements[InventoryData.bananaKey]),
                    (blueberrySprite, InventoryData.stevenRequirements[InventoryData.blueberryKey]),
                    (strawberrySprite, InventoryData.stevenRequirements[InventoryData.strawberryKey])
                };
        }

        return new List<(Sprite, int)>();
        */
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
