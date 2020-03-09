using System;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleMobileAds.Api;

public struct RewardStruct
{
    public bool rewardUser;
    public int rewardAmount;

    public void SetReward(bool shouldReward, int amount)
    {
        rewardUser = shouldReward;
        rewardAmount = amount;
    }

    public void ClearReward()
    {
        rewardUser = false;
        rewardAmount = 0;
    }
}

public class AdObjectBehavior : MonoBehaviour
{
    // NOTE: Set TESTING to false for releases to show real ads
    private bool TESTING = true;

    private RewardedAd rewardAd;
    private RewardStruct reward;

    private GameObject attachedGameObject;

    private const int REWARD_LIVES_AMOUNT = 3;

    private void Awake()
    {
        AdManager.Initialize(TESTING);

        if (AdManager.instance.IsSupportedPlatform())
        {
            rewardAd = AdManager.instance.GetRewardAd();

            // Called when the user should be rewarded for interacting with the ad.
            rewardAd.OnUserEarnedReward += HandleUserEarnedReward;
            // Called when the ad is closed.
            rewardAd.OnAdClosed += HandleRewardAdClosed;

            rewardAd.OnAdFailedToLoad += HandleRewardAdLoadFailed;
        }

        reward = new RewardStruct();
    }

    private void UnsubscribeRewardAdEvents()
    {
        if (null != rewardAd)
        {
            rewardAd.OnUserEarnedReward -= HandleUserEarnedReward;
            rewardAd.OnAdClosed -= HandleRewardAdClosed;
            rewardAd.OnAdFailedToLoad -= HandleRewardAdLoadFailed;
        }
    }

    public void HandleRewardAdLoadFailed(object sender, AdErrorEventArgs args)
    {
        // Notify the game object to which this is attached
        if (null != attachedGameObject)
        {
            attachedGameObject.SendMessage("RewardAdFailed", args);
        }
    }

    public void HandleRewardAdClosed(object sender, EventArgs args)
    {
        // Notify the game object to which this is attached
        if (null != attachedGameObject)
        {
            attachedGameObject.SendMessage("RewardAdClosed", reward);
        }

        // Clear out this variable now
        reward.ClearReward();
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        if (TESTING)
        {
            reward.SetReward(true, REWARD_LIVES_AMOUNT);
        }
        else
        {
            // I believe the reward amount for test ads is 10 and that can't
            // be changed; only use the amount coming from Admob functions when
            // using REAL/LIVE ads, which means when TESTING = false, not true
            reward.SetReward(true, (int)args.Amount);
        }
    }

    public void ShowRewardLivesAd()
    {
        if (AdManager.instance.IsSupportedPlatform())
        {
            if (rewardAd.IsLoaded())
            {
                rewardAd.Show();
            }
        }
        else
        {
            // Simulate a reward ad having been loaded and shown if this is not
            // a supported platform
            reward.SetReward(true, REWARD_LIVES_AMOUNT);
            HandleRewardAdClosed(this, new EventArgs());
        }
    }

    private void OnDestroy()
    {
        UnsubscribeRewardAdEvents();
    }

    // MUST call this to init this Ad Object
    public void InitAdObject(GameObject obj)
    {
        attachedGameObject = obj;
    }
}
