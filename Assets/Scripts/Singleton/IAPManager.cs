using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class IAPManager : IStoreListener
{
    public const string COIN_PACK1_ID = "xyz.ashgames.hovrbird.product.coin_pack1";
    public const string COIN_PACK2_ID = "xyz.ashgames.hovrbird.product.coin_pack2";
    public const string COIN_PACK3_ID = "xyz.ashgames.hovrbird.product.coin_pack3";
    public const string KOKO2_PRODUCT_ID = "xyz.ashgames.hovrbird.product.koko2_egg";
    public const string SAM2_PRODUCT_ID = "xyz.ashgames.hovrbird.product.sam2_egg";
    public const string NIGEL2_PRODUCT_ID = "xyz.ashgames.hovrbird.product.nigel2_egg";
    public const string STEVEN2_PRODUCT_ID = "xyz.ashgames.hovrbird.product.steven2_egg";

    public const int COIN_PACK1_VALUE = 600;
    public const int COIN_PACK2_VALUE = 2000;
    public const int COIN_PACK3_VALUE = 3400;

    private IStoreController controller;
    private IExtensionProvider extensions;

    private bool initSucceeded;

    public IAPManager()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(COIN_PACK1_ID, ProductType.Consumable);
        builder.AddProduct(COIN_PACK2_ID, ProductType.Consumable);
        builder.AddProduct(COIN_PACK3_ID, ProductType.Consumable);
        builder.AddProduct(KOKO2_PRODUCT_ID, ProductType.NonConsumable);
        builder.AddProduct(SAM2_PRODUCT_ID, ProductType.NonConsumable);
        builder.AddProduct(NIGEL2_PRODUCT_ID, ProductType.NonConsumable);
        builder.AddProduct(STEVEN2_PRODUCT_ID, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsInitialized()
    {
        return initSucceeded;
    }

    /*
     * Initialization succeeded
     */
    void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        initSucceeded = true;

        this.controller = controller;
        this.extensions = extensions;

        // XXX Need to add functionality to restore purchases on Apple devices if/when
        // they ever accept this game into their store
    }

    /*
     * Initialization failed
     */
    void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
    {
        // Notify the user that initialization failed if they're in the store
        MonoBehaviour.print("Error initializing Unity IAP: " + error.ToString());
    }

    /*
     * Purchase failed
     */
    void IStoreListener.OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        // Notify the user why the purchase failed
        MonoBehaviour.print("Error initializing Unity IAP: " + p.ToString());
    }

    /*
     * Purchase succeeded
     */
    PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs e)
    {
        bool validPurchase = ValidateReceipt(e.purchasedProduct);

        if (validPurchase)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();

            switch (e.purchasedProduct.definition.id)
            {
                case COIN_PACK1_ID:
                    dict.Add(InventoryData.coinKey, COIN_PACK1_VALUE);
                    InventoryData.Instance().AddCurrency(dict);
                    break;

                case COIN_PACK2_ID:
                    dict.Add(InventoryData.coinKey, COIN_PACK2_VALUE);
                    InventoryData.Instance().AddCurrency(dict);
                    break;

                case COIN_PACK3_ID:
                    dict.Add(InventoryData.coinKey, COIN_PACK3_VALUE);
                    InventoryData.Instance().AddCurrency(dict);
                    break;

                case KOKO2_PRODUCT_ID:
                    UnlockAngryKokoEgg();
                    break;

                case SAM2_PRODUCT_ID:
                    UnlockAngrySamEgg();
                    break;

                case NIGEL2_PRODUCT_ID:
                    UnlockAngryNigelEgg();
                    break;

                case STEVEN2_PRODUCT_ID:
                    UnlockAngryStevenEgg();
                    break;
            }
        }

        // Notify the user that the purchase was successful
        return PurchaseProcessingResult.Complete;
    }

    /*
     * Initializing a purchase
     */
    public void MakePurchase(string productId)
    {
        controller.InitiatePurchase(productId);
    }

    private bool ValidateReceipt(Product product)
    {
        bool validPurchase = true;

        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
        try
        {
            var result = validator.Validate(product.receipt);
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate);
                Debug.Log(productReceipt.transactionID);
            }
        }
        catch (IAPSecurityException)
        {
            Debug.Log("Invalid receipt, not unlocking content");
            validPurchase = false;
        }

        return validPurchase;
    }

    private void UnlockAngryKokoEgg()
    {
        Product product = controller.products.WithID(KOKO2_PRODUCT_ID);
        if (product.hasReceipt)
        {
            // Fulfill purchase now
            InventoryData.Instance().AddEggToInventory(InventoryData.ANGRY_KOKO_ID);
        }
    }

    private void UnlockAngrySamEgg()
    {
        Product product = controller.products.WithID(SAM2_PRODUCT_ID);
        if (product.hasReceipt)
        {
            // Fulfill purchase now
            InventoryData.Instance().AddEggToInventory(InventoryData.ANGRY_SAM_ID);
        }
    }

    private void UnlockAngryNigelEgg()
    {
        Product product = controller.products.WithID(NIGEL2_PRODUCT_ID);
        if (product.hasReceipt)
        {
            // Fulfill purchase now
            InventoryData.Instance().AddEggToInventory(InventoryData.ANGRY_NIGEL_ID);
        }
    }

    private void UnlockAngryStevenEgg()
    {
        Product product = controller.products.WithID(STEVEN2_PRODUCT_ID);
        if (product.hasReceipt)
        {
            // Fulfill purchase now
            InventoryData.Instance().AddEggToInventory(InventoryData.ANGRY_STEVEN_ID);
        }
    }
}