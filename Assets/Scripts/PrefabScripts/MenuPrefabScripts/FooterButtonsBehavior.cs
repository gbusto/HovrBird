using UnityEngine;
using UnityEngine.UI;

public class FooterButtonsBehavior : MonoBehaviour
{
    public Button exitButton;
    public Button inventoryButton;
    public Button storeButton;

    public GameObject notificationCanvasPrefab;
    private GameObject inventoryNotification;
    private GameObject exitNotification;

    private NotificationManager notificationMgr;

    public void Start()
    {
        notificationMgr = NotificationManager.Instance();

        inventoryNotification = Instantiate(notificationCanvasPrefab);
        inventoryNotification.transform.SetParent(inventoryButton.transform, false);
        inventoryNotification.gameObject.SetActive(false);

        exitNotification = Instantiate(notificationCanvasPrefab);
        exitNotification.transform.SetParent(exitButton.transform, false);
        exitNotification.gameObject.SetActive(false);

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
}
