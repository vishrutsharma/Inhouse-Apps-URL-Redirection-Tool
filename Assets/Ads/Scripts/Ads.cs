using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;


namespace Ad
{
    public class Ads : MonoBehaviour
    {
        [SerializeField] private RawImage gameIconTexture;
        [SerializeField] private TextMeshProUGUI gameNameText;
        [SerializeField] private GameObject downloadIcon;
        [SerializeField] private GameObject downloadErrorIcon;

        private AdData myAdData = null;
        private bool isDownloadingTexture = false;
        private Vector2 destinationPos;


        private void Update()
        {
            if (isDownloadingTexture)
                downloadIcon.transform.Rotate(Vector3.forward * 180 * Time.deltaTime);
        }

        private void Reset()
        {
            downloadErrorIcon.SetActive(false);
            downloadIcon.SetActive(false);
            GetComponent<Image>().enabled = false;
            gameIconTexture.gameObject.SetActive(false);
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        IEnumerator DownloadTexture()
        {
            yield return new WaitForEndOfFrame();
            downloadIcon.SetActive(true);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(myAdData.gameTextureURL);
            yield return www.SendWebRequest();

            downloadIcon.SetActive(false);
            isDownloadingTexture = false;

            if (www.result != UnityWebRequest.Result.Success)
            {
                downloadErrorIcon.SetActive(true);
                Debug.LogError("Could not download Texture for :" + myAdData.gameName);
            }
            else
            {
                gameIconTexture.gameObject.SetActive(true);
                gameIconTexture.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }
        }

        private void OnButtonClick()
        {
            if (string.IsNullOrEmpty(myAdData.urlLink))
            {
                Debug.LogError("Game URL missing");
                return;
            }

            Application.OpenURL(myAdData.urlLink);
        }

        public void Init(AdData adData)
        {
            myAdData = adData;
            Reset();
            isDownloadingTexture = true;
            StartCoroutine(DownloadTexture());
        }

        public void SetDestinationPos()
        {
            GetComponent<Image>().enabled = true;
            destinationPos = new Vector2(transform.localPosition.x, 0);
        }

        public void Slide(bool isAdsOn)
        {
            if (!isAdsOn)
            {
                transform.DOLocalMove(destinationPos, 0.5f).SetEase(Ease.OutCirc);
            }
            else
            {
                transform.DOLocalMove(new Vector2(-950, 0), 0.5f).SetEase(Ease.OutCirc);
            }
        }
    }
}
