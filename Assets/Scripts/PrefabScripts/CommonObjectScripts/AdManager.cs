using System;
using GoogleMobileAds.Api;
using UnityEngine;

/* NOTE:
 *
 * Apple is not currently and will not accept my game for iOS. So I have removed
 * most/all code for preparing ads for iOS devices.
 */

public class AdManager
{
    public static AdManager instance;
    public static RewardedAd rewardAd;

    private static bool rewardAdBeingLoaded;
    private static bool rewardAdIsReady;
    private static bool useTestAds;

    private const string BANNER_AD_TEST_ID = "ca-app-pub-3940256099942544/6300978111";
    private const string ANDROID_BANNER_AD_LEVELS_ID = "ca-app-pub-4215818305477568/6510880295";
    private const string IOS_BANNER_AD_LEVELS_ID = "ca-app-pub-4215818305477568/4527355999";

    private const string INTERSTITIAL_AD_TEST_ID = "ca-app-pub-3940256099942544/1033173712";
    private const string IOS_INTERSTITIAL_AD_LEVELS_ID = "ca-app-pub-4215818305477568/4801375080";
    private const string ANDROID_INTERSTITIAL_AD_LEVELS_ID = "ca-app-pub-4215818305477568/2039769350";

    private const string REWARD_AD_TEST_ID = "ca-app-pub-3940256099942544/1712485313";
    private const string IOS_REWARD_AD_RESCUE_USER_ID = "ca-app-pub-4215818305477568/7044395043";
    private const string ANDROID_REWARD_AD_RESCUE_USER_ID = "ca-app-pub-4215818305477568/8030462638";
    private const string ANDROID_REWARD_AD_THREE_LIVES_ID = "ca-app-pub-4215818305477568/9879467987";

    protected AdManager()
    {

    }

    /*
     * Public ad functions
     */
    public static AdManager Initialize(bool testing = false)
    {
        if (null == instance)
        {
            // Initialize Google ads
            MobileAds.Initialize(initStatus => { });

            // Initialize our instance
            instance = new AdManager();
        }

        useTestAds = testing;
        return instance;
    }

    public BannerView GetBannerView()
    {
        // Caller needs to ensure this banner view is destroyed

        string adId = GetBannerAdUnitId();
        BannerView bannerAd = new BannerView(adId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = LoadAdRequest();
        bannerAd.LoadAd(request);
        return bannerAd;
    }

    public InterstitialAd GetInterstitialAd()
    {
        // Caller needs to ensure this interstitial ad is destroyed

        string adId = GetInterstitialAdUnitId();
        InterstitialAd interstitialAd = new InterstitialAd(adId);
        AdRequest request = LoadAdRequest();
        interstitialAd.LoadAd(request);
        return interstitialAd;
    }

    public RewardedAd GetRewardAd()
    {
        if (false == rewardAdBeingLoaded && false == rewardAdIsReady)
        {
            string adId = GetRewardAdUnitId();
            rewardAd = new RewardedAd(adId);

            // Load a new reward ad
            AdRequest request = LoadAdRequest();
            rewardAd.LoadAd(request);
            rewardAdBeingLoaded = true;

            rewardAd.OnAdClosed += RewardAdClosed;
            rewardAd.OnAdLoaded += RewardAdLoadSuccess;
            rewardAd.OnAdFailedToLoad += RewardAdLoadFail;
        }

        return rewardAd;
    }

    public void RewardAdClosed(object sender, EventArgs args)
    {
        rewardAdIsReady = false;
    }

    public void RewardAdLoadSuccess(object sender, EventArgs args)
    {
        rewardAdBeingLoaded = false;
        rewardAdIsReady = true;
    }

    public void RewardAdLoadFail(object sender, AdErrorEventArgs args)
    {
        rewardAdIsReady = false;
        rewardAdBeingLoaded = false;
    }

    public bool IsSupportedPlatform()
    {
        if (useTestAds)
        {
#if UNITY_EDITOR
            return false;
#else
            return true;
#endif
        }

#if UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }


    /*
     * Private ad functions
     */
    private AdRequest LoadAdRequest()
    {
        AdRequest request;
        if (useTestAds)
        {
#if UNITY_ANDROID
            request = new AdRequest.Builder().AddTestDevice("91D37DDD343C93F8D7DC71E404E0C2F4").Build();
#elif UNITY_IOS
            request = new AdRequest.Builder().AddTestDevice("32d0ef8d0bb6ecc4c5a33bcaf54b0db4").Build();
#else
            request = new AdRequest.Builder().Build();
#endif
        }
        else
        {
            request = new AdRequest.Builder().Build();
        }

        return request;
    }

    private string GetBannerAdUnitId()
    {
        if (useTestAds)
        {
            // Return the test banner ad unit ID
            return BANNER_AD_TEST_ID;
        }

#if UNITY_ANDROID
        // Return the Android banner ad unit ID
        return ANDROID_BANNER_AD_LEVELS_ID;
#endif
        return "";
    }

    private string GetInterstitialAdUnitId()
    {
        if (useTestAds)
        {
            // Return the test interstitial ad unit ID
            return INTERSTITIAL_AD_TEST_ID;
        }

#if UNITY_ANDROID
        // Return the Android banner ad unit ID
        return ANDROID_INTERSTITIAL_AD_LEVELS_ID;
#endif
        return "";
    }

    private string GetRewardAdUnitId()
    {
        if (useTestAds)
        {
            // Return the test reward ad unit ID
            return REWARD_AD_TEST_ID;
        }

#if UNITY_ANDROID
        // Return the Android reward ad unit ID; switching the reward ad for this function to the 3 lives reward
        return ANDROID_REWARD_AD_THREE_LIVES_ID;
#endif
        return "";
    }
}
