using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NotificationDataStruct
{
    public Dictionary<uint, bool> notifications;
}

public class NotificationManager
{
    private static NotificationManager _instance;

    private InventoryData inventoryData;

    public Dictionary<uint, bool> notifications;
    private Dictionary<uint, HashSet<GameObject>> subscriptions;

    // User collected a new egg (show only once)
    public const uint newEggNotificationId = 0x1;
    // User can hatch an egg (show only once)
    public const uint canHatchNotificationId = 0x2;
    // User collected items for the first time (show only once)
    public const uint firstTimeItemsNotificationId = 0x3;

    private NotificationDataStruct notificationsData;

    private const string dataFilename = "h3qck1rwourh.hb";

    private static byte[] key =
    {
        0xd3, 0xa8, 0xc5, 0x02, 0xd2, 0x03, 0xe1, 0x6c, 0x52, 0xb3, 0xf3, 0x84, 0x2b, 0xa8, 0x7f, 0x3a
    };

    private static byte[] iv =
    {
        0x92, 0x5c, 0xff, 0xf3, 0xda, 0x90, 0xde, 0x19, 0x86, 0xde, 0xe7, 0x5a, 0xe1, 0x88, 0x98, 0x66
    };

    protected NotificationManager()
    {
        inventoryData = InventoryData.Instance();
        notifications = new Dictionary<uint, bool>()
        {
            [newEggNotificationId] = false,
            [canHatchNotificationId] = false,
            [firstTimeItemsNotificationId] = false,
        };
        subscriptions = new Dictionary<uint, HashSet<GameObject>>()
        {
            [newEggNotificationId] = new HashSet<GameObject>(),
            [canHatchNotificationId] = new HashSet<GameObject>(),
            [firstTimeItemsNotificationId] = new HashSet<GameObject>(),
        };
    }

    protected NotificationManager(NotificationDataStruct data)
    {
        inventoryData = InventoryData.Instance();

        notifications = data.notifications;
        subscriptions = new Dictionary<uint, HashSet<GameObject>>()
        {
            [newEggNotificationId] = new HashSet<GameObject>(),
            [canHatchNotificationId] = new HashSet<GameObject>(),
            [firstTimeItemsNotificationId] = new HashSet<GameObject>(),
        };
    }

    public static NotificationManager Instance()
    {
        if (null == _instance)
        {
            _instance = LoadUserData();
        }

        return _instance;
    }

    public void SaveUserData()
    {
        notificationsData.notifications = notifications;

        string fullPath = GetFullPath();
        IFormatter formatter = new BinaryFormatter();

        using (MemoryStream memStream = new MemoryStream())
        {
            formatter.Serialize(memStream, notificationsData);
            memStream.Seek(0, SeekOrigin.Begin);
            byte[] serializedData = memStream.ToArray();
            EncryptedDataManager.WriteData(fullPath, key, iv, serializedData);
        }
    }

    // Marked private because this should only be called via the Instance() method
    private static NotificationManager LoadUserData()
    {
        string fullPath = GetFullPath();

        if (File.Exists(fullPath))
        {
            byte[] serializedData = EncryptedDataManager.ReadData(fullPath, key, iv);
            using (MemoryStream memStream = new MemoryStream(serializedData))
            {
                IFormatter formatter = new BinaryFormatter();
                NotificationDataStruct data = (NotificationDataStruct)formatter.Deserialize(memStream);
                return new NotificationManager(data);
            }
        }

        NotificationManager instance = new NotificationManager();
        instance.notificationsData = new NotificationDataStruct
        {
            notifications = instance.notifications
        };

        return instance;
    }

    public static void DeleteUserData()
    {
        string fullPath = GetFullPath();

        if (File.Exists(fullPath))
        {
            _instance = null;
            File.Delete(fullPath);
        }
    }

    private static string GetFullPath()
    {
        string directory = NativeMobileStorage.GetApplicationDirectory();

        string fullPath = Path.Combine(directory, dataFilename);

        return fullPath;
    }


    public void Subscribe(uint key, GameObject obj)
    {
        if (subscriptions.ContainsKey(key))
        {
            subscriptions[key].Add(obj);
        }
    }

    public void Unsubscribe(uint key, GameObject obj)
    {
        if (null != obj)
        {
            if (subscriptions.ContainsKey(key))
            {
                obj.gameObject.SetActive(false);
                subscriptions[key].Remove(obj);
            }
        }
    }

    // Functions to run notification checks
    public void RunNotificationCheck()
    {
        CheckForNewEggs();
        CheckForEggHatch();
        CheckForNewItems();

        PurgeNullObjects();

        Notify();
    }

    public void ClearNotification(uint key)
    {
        if (notifications.ContainsKey(key))
        {
            if (notifications[key] && key == firstTimeItemsNotificationId)
            {
                // Prevent this notification from being shown again in the future
                PlayerPrefsCommon.SetFirstItemsCollected(true);
            }

            notifications[key] = false;

            foreach (GameObject obj in subscriptions[key])
            {
                obj.SetActive(false);
            }
        }

        SaveUserData();
    }

    private void Notify()
    {
        foreach (KeyValuePair<uint, bool> notification in notifications)
        {
            if (notification.Value)
            {
                foreach (GameObject obj in subscriptions[notification.Key])
                {
                    if (false == obj.activeInHierarchy)
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }
    }

    private void PurgeNullObjects()
    {
        foreach (KeyValuePair<uint, HashSet<GameObject>> item in subscriptions)
        {
            HashSet<GameObject> set = item.Value;
            List<GameObject> nulls = new List<GameObject>();
            foreach (GameObject go in set)
            {
                if (null == go)
                {
                    nulls.Add(go);
                }
            }
            foreach (GameObject go in nulls)
            {
                set.Remove(go);
            }
        }
    }

    private void CheckForNewEggs()
    {
        if (inventoryData.NotifyForNewEgg())
        {
            notifications[newEggNotificationId] = true;
        }
    }

    private void CheckForEggHatch()
    {
        /*
         * 1. Search for locked egg items
         * 2. Get item count requirements
         * 3. Search inventory data to see if the user has enough for each requirement
         */
        foreach (KeyValuePair<uint, bool> bird in inventoryData.birdDict)
        {
            if (InventoryData.UNHATCHED == bird.Value)
            {
                bool reqsMet = true;
                Dictionary<string, int> reqsDict = InventoryData.GetRequirementForId(bird.Key);
                foreach (KeyValuePair<string, int> req in reqsDict)
                {
                    if (inventoryData.collectibleDict[req.Key] < reqsDict[req.Key])
                    {
                        reqsMet = false;
                    }
                }

                if (reqsMet)
                {
                    notifications[canHatchNotificationId] = true;
                }
            }
        }
    }

    private void CheckForNewItems()
    {
        if (PlayerPrefsCommon.GetFirstItemsCollected())
        {
            // Don't run this check if this notification has already been shown and dismissed
            return;
        }
        foreach (KeyValuePair<string, int> item in inventoryData.collectibleDict)
        {
            if (item.Value > 0)
            {
                notifications[firstTimeItemsNotificationId] = true;
                break;
            }
        }
    }
}
