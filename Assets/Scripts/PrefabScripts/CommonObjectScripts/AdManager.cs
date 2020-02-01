using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdManager
{
    public static AdManager instance;
    public static RewardedAd rewardAd;

    private static bool rewardAdBeingLoaded;
    private static bool rewardAdIsReady;
    private static bool useTestAds;

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
            return "ca-app-pub-3940256099942544/6300978111";
        }

#if UNITY_ANDROID
        // Return the Android banner ad unit ID
        return "ca-app-pub-4215818305477568/6510880295";
#elif UNITY_IOS
        // Return the iOS banner ad unit ID
        return "ca-app-pub-4215818305477568/4527355999";
#endif
        return "";
    }

    private string GetInterstitialAdUnitId()
    {
        if (useTestAds)
        {
            // Return the test interstitial ad unit ID
            return "ca-app-pub-3940256099942544/1033173712";
        }

#if UNITY_ANDROID
        // Return the Android banner ad unit ID
        return "ca-app-pub-4215818305477568/2039769350";
#elif UNITY_IOS
        // Return the iOS banner ad unit ID
        return "ca-app-pub-4215818305477568/4801375080";
#endif
        return "";

    }

    private string GetRewardAdUnitId()
    {
        if (useTestAds)
        {
            // Return the test reward ad unit ID
            return "ca-app-pub-3940256099942544/1712485313";
        }

#if UNITY_ANDROID
        // Return the Android banner ad unit ID
        return "ca-app-pub-4215818305477568/8030462638";
#elif UNITY_IOS
        // Return the iOS banner ad unit ID
        return "ca-app-pub-4215818305477568/7044395043";
#endif
        return "";
    }

    private void print(string message)
    {
        MonoBehaviour.print(message);
    }
}
