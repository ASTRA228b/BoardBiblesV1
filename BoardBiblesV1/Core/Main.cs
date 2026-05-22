using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Astras_BB.Core;

public class Main : MonoBehaviour
{
    private TMP_Text? MotdText;
    private TMP_Text? MotdBodyText;
    private TMP_Text? COCText;
    private TMP_Text? COCBodyText;
    // private TMP_Text? COCRankedBText;
    // private TMP_Text? COCRankedHText;

    private DateTime? ChangeTime;
    private string CV = "";
    private string local = "Environment Objects/LocalObjects_Prefab/TreeRoom/";
    // private string RLocal = "RankedMain/Ranked_Layout/Ranked_TreeRoom_prefab";
    private string ModHTitle = "Board Bible V1";

    private bool showingStartMessage = true;
    private DateTime? StartT;
    private string StartMsg = "WELCOME TO BOARD BIBLE MADE BY ASTRA\n LOADING GET READY...";

    private bool LV = false;
    [Serializable]
    public class ApiBibleRes
    {
        public string? reference;
        public string? text;
    }

    private readonly string[] verses =
    {
     "John 3:16 - For God so loved the world that he gave his one and only Son.",
     "Philippians 4:13 - I can do all things through Christ who strengthens me.",
     "Psalm 23:1 - The Lord is my shepherd; I shall not want.",
     "Romans 8:28 - And we know that in all things God works for good.",
     "Proverbs 3:5 - Trust in the Lord with all your heart.",
     "Jeremiah 29:11 - Plans to give you hope and a future.",
     "Matthew 5:14 - You are the light of the world.",
     "Isaiah 41:10 - Do not fear, for I am with you.",
     "Joshua 1:9 - Be strong and courageous.",
     "1 Peter 5:7 - Cast all your anxiety on him because he cares for you.",
     "Romans 12:2 - Be transformed by the renewing of your mind.",
     "Psalm 46:1 - God is our refuge and strength.",
     "Matthew 11:28 - Come to me, all who are weary.",
     "2 Timothy 1:7 - God gave us power, love, and self-control.",
     "Colossians 3:23 - Work at it with all your heart.",
     "Isaiah 40:31 - Those who hope in the Lord will renew their strength.",
     "Hebrews 11:1 - Faith is confidence in what we hope for.",
     "Psalm 118:24 - This is the day the Lord has made.",
     "Galatians 5:22 - The fruit of the Spirit is love, joy, peace.",
     "James 1:2 - Consider it joy when you face trials."
    };

    private void Update()
    {
        if (MotdText == null || MotdBodyText == null || COCText == null || COCBodyText == null)
        {
            FindThem();
            return;
        }

        if (showingStartMessage)
        {
            CV = StartMsg;

            if (DateTime.UtcNow - StartT >= TimeSpan.FromMinutes(2))
            {
                showingStartMessage = false;
                PickVerse();
            }
        }
        else
        {
            if (!LV && (ChangeTime == null || DateTime.UtcNow - ChangeTime >= TimeSpan.FromMinutes(5)))
            {
                PickVerse();
            }
        }
        ApplyTexts();
    }

    private void Start()
    {
        StartT = DateTime.UtcNow;
        CV = StartMsg;
    }


    private void FindThem()
    {
        GameObject MotdHTxt = GameObject.Find(local + "motdHeadingText");
        GameObject MotdBTxt = GameObject.Find(local + "motdBodyText");
        GameObject COCHTxt = GameObject.Find(local + "CodeOfConductHeadingText");
        GameObject COCBTxt = GameObject.Find(local + "COCBodyText_TitleData");
        // GameObject COCRankedHTxt = GameObject.Find(RLocal + "CodeOfConductHeadingText");
        // GameObject COCRankedBTxt = GameObject.Find(RLocal + "COCBodyText_TitleDataRanked");
        MotdBTxt.GetComponent<PlayFabTitleDataTextDisplay>().enabled = false;
        COCBTxt.GetComponent<PlayFabTitleDataTextDisplay>().enabled = false;
        // COCRankedBTxt.GetComponent<PlayFabTitleDataTextDisplay>().enabled = false;
        if (MotdHTxt != null)
            MotdText = MotdHTxt.GetComponent<TMP_Text>();
        
        if (MotdBTxt != null)
            MotdBodyText = MotdBTxt.GetComponent<TMP_Text>();
        
        if (COCHTxt != null)
            COCText = COCHTxt.GetComponent<TMP_Text>();
        
        if (COCBTxt != null)
            COCBodyText = COCBTxt.GetComponent<TMP_Text>();

        // if (COCRankedBTxt != null)
            // COCRankedBText = COCRankedBTxt.GetComponent<TMP_Text>();

        // if (COCRankedHTxt != null)
            // COCRankedHText = COCRankedHTxt.GetComponent<TMP_Text>();
    }

    private void ApplyTexts()
    {
        if (MotdText != null)
            MotdText.text = ModHTitle;

        if (COCText != null)
            COCText.text = ModHTitle;

        // if (COCRankedHText != null)
            // COCRankedHText.text = ModHTitle;

        if (MotdBodyText != null)
            MotdBodyText.text = CV.ToUpper();

        if (COCBodyText != null)
            COCBodyText.text = CV.ToUpper();

        // if (COCRankedBText != null)
           //  COCRankedBText.text = CV.ToUpper();
    }

    private void PickVerse()
    {
        if (!LV)
        {
            StartCoroutine(APIPuller());
        }
    }

    private IEnumerator APIPuller()
    {
        LV = true;

        string[] books = {
            "john", "psalms", "romans", "proverbs",
            "matthew", "isaiah", "james", "hebrews"
        };
        string book = books[UnityEngine.Random.Range(0, books.Length)];
        int chapter = UnityEngine.Random.Range(1, 20);
        int verse = UnityEngine.Random.Range(1, 30);
        string refText = $"{book} {chapter}:{verse}";
        string url = "https://bible-api.com/" + UnityWebRequest.EscapeURL(refText);
        using (UnityWebRequest GIF = UnityWebRequest.Get(url))
        {
            GIF.timeout = 8;
            yield return GIF.SendWebRequest();
            if (GIF.result == UnityWebRequest.Result.ConnectionError ||
                GIF.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogWarning("[Bible] Your Network is shit, using hardcoded stuff.");
                UseLocal();
                yield break;
            }

            if (GIF.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("[Bible] HTTP error: " + GIF.responseCode);
                UseLocal();
                yield break;
            }

            if (string.IsNullOrEmpty(GIF.downloadHandler.text))
            {
                Debug.LogWarning("[Bible] Empty response");
                UseLocal();
                yield break;
            }

            ApiBibleRes Data;

            try
            {
                Data = JsonUtility.FromJson<ApiBibleRes>(GIF.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Bible] JSON error: " + e.Message);
                UseLocal();
                yield break;
            }

            if (Data == null || string.IsNullOrEmpty(Data.text))
            {
                UseLocal();
                yield break;
            }

            CV = Data.reference + " - " + Data.text;
            ChangeTime = DateTime.UtcNow;
            LV = false;
        }
    }

    

    private void UseLocal()
    {
        int index = UnityEngine.Random.Range(0, verses.Length);
        CV = verses[index];
        CV += "\n\n(offline mode)";
        ChangeTime = DateTime.UtcNow;
        LV = false;
    }

}