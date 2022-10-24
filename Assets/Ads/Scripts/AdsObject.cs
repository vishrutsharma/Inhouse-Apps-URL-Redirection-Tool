using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.RemoteConfig;
using System.Collections;
using System.Collections.Generic;


namespace Ad
{
    [Serializable]
    public class AdData
    {
        public string gameName;
        public string gameTextureURL;
        public string urlLink;
    }

    [Serializable]
    public class AdObject
    {
        public List<AdData> ads;
    }

    public class AdsObject : MonoBehaviour
    {
        [SerializeField] private string uAdsKey = "AdsData";
        [SerializeField] private GameObject AdsPrefab;
        [SerializeField] private Transform adsContainer;
        [SerializeField] private Button toggleAdsButton;

        private struct userAttributes { }
        private struct appAttributes { }
        private bool isAdsOn = false;
        private List<Ads> adsList;
        private Coroutine animCoroutine = null;
        private bool allowInteraction;

        private void Start()
        {
            allowInteraction = true;
            toggleAdsButton.onClick.AddListener(ToggleAds);
            ConfigManager.FetchCompleted += InitRemoteConfig;
            ConfigManager.SetEnvironmentID("production");
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
        }

        private void InitRemoteConfig(ConfigResponse configResponse)
        {
            Debug.Log("Init Remote Config");
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    Debug.Log("No settings loaded this session; using default values.");
                    break;
                case ConfigOrigin.Cached:
                    Debug.Log("No settings loaded this session; using cached values from a previous session.");
                    break;
                case ConfigOrigin.Remote:
                    Debug.Log("New settings loaded this session; update values accordingly.");


                    if (!ConfigManager.appConfig.HasKey(uAdsKey))
                    {
                        Debug.LogError("No Unleash Ads Key found");
                        return;
                    }

                    var jsonString = ConfigManager.appConfig.GetJson(uAdsKey);
                    UAdObject adOb = JsonUtility.FromJson<UAdObject>(jsonString);
                    StartCoroutine(PopulateAds(adOb.ads));
                    break;
            }
        }


        private IEnumerator PopulateAds(List<AdData> AdsList)
        {
            if (AdsList == null || AdsList.Count == 0)
            {
                Debug.Log("Ads Data not found");
            }
            else
            {
                adsList = new List<Ads>();
                for (int i = 0; i < AdsList.Count; i++)
                {
                    GameObject adObject = Instantiate(AdsPrefab, adsContainer);
                    Ads ads = adObject.GetComponent<Ads>();
                    ads.Init(uAdsList[i]);
                    adsList.Add(ads);
                }

                yield return new WaitForEndOfFrame();
                GetComponentInChildren<HorizontalLayoutGroup>().enabled = false;

                for (int i = 0; i < adsList.Count; i++)
                {
                    adsList[i].SetDestinationPos();
                    adsList[i].transform.localPosition = new Vector2(-950, 0);
                }
            }
        }

        private IEnumerator SlideAds()
        {
            for (int i = 0; i < adsList.Count; i++)
            {
                adsList[i].Slide(isAdsOn);
                yield return new WaitForSeconds(0.02f);
            }
            allowInteraction = true;
            isAdsOn = !isAdsOn;
        }

        private void ToggleAds()
        {
            if (allowInteraction)
            {
                toggleAdsButton.transform.localRotation = Quaternion.Euler(0, 0, isAdsOn ? 0 : 180);
                allowInteraction = false;
                if (animCoroutine != null)
                    StopCoroutine(animCoroutine);

                animCoroutine = StartCoroutine(SlideAds());
            }
        }
    }
}
