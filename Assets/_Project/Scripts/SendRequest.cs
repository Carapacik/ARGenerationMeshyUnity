using ARLocation;
using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SendRequest : MonoBehaviour
{
    public GameObject DialogObject;
    public TMP_InputField InputField;
    public GameObject LoadingObject;
    public Text LoadingText;
    public GameObject PublishObject;
    public GameObject OtherUIObject;
    public PlaceAtLocation.PlaceAtOptions PlacementOptions;
    private bool loading = false;
    public bool isLike = false;
    private string resultId = "";

    void Start()
    {
        if (isLike)
        {
            List<SavedModel> models = new List<SavedModel>();
            if (PlayerPrefs.HasKey("MODELS"))
            {
                models = new List<SavedModel>(PlayerPrefs.GetString("MODELS").Split(',').Select((e) => JsonUtility.FromJson<SavedModel>(e)));
            }
            if (models.Count == 0)
            {
                return;
            }

            var currentLocation = ARLocationProvider.Instance.Provider.CurrentLocation;
            var nearestModel = LocationHelper.FindNearestModel(currentLocation.latitude, currentLocation.longitude, models);
            byte[] file = File.ReadAllBytes(nearestModel.SavedPath);
            var arObject = Importer.LoadFromBytes(file);
            arObject.transform.localScale = new Vector3(-1, 1, 1);
            var newLocation = new Location()
            {
                Latitude = nearestModel.Latitude,
                Longitude = nearestModel.Longitude,
                AltitudeMode = AltitudeMode.DeviceRelative
            };
            PlaceAtLocation.CreatePlacedInstance(arObject, newLocation, PlacementOptions, false);
        }
    }

    void Update()
    {
        LoadingObject.SetActive(loading);
        if (loading)
        {
            OtherUIObject.SetActive(false);
        }

    }

    public void OpenGenerateDialog()
    {
        DialogObject.SetActive(true);
    }

    public void HidePublish()
    {
        OtherUIObject.SetActive(true);
        PublishObject.SetActive(false);
    }

    public void SendPrompt()
    {
        loading = true;
        StartCoroutine(GetResultId(InputField.textComponent.text));
    }

    private IEnumerator<object> GetResultId(string prompt)
    {
        using UnityWebRequest www = UnityWebRequest.Get(new Uri($"https://street-ar-carapacika2.amvera.io/api/Generation/text-2-result?prompt={prompt}"));
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        resultId = www.downloadHandler.text;
        LoadingText.text = "Генерация модели...";
        Debug.Log($"resultId = {resultId}");
        if (!string.IsNullOrEmpty(resultId) && loading)
        {
            StartCoroutine(GetResultById());
        }
    }

    private IEnumerator<object> GetResultById()
    {
        while (loading)
        {
            using UnityWebRequest www = UnityWebRequest.Get(new Uri($"https://street-ar-carapacika2.amvera.io/api/Generation/result-2-model?resultId={resultId}"));
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                loading = false;
            }
            var response = JsonUtility.FromJson<GeneratedContent>(www.downloadHandler.text);
            if (response.progress < 100 && string.IsNullOrEmpty(response.model_urls.glb))
            {
                Debug.Log($"progress {response.progress}");
                yield return new WaitForSeconds(2);
            }
            else
            {
                LoadingText.text = "Загрузка модели...";
                using UnityWebRequest modelWww = UnityWebRequest.Get(response.model_urls.glb);
                yield return modelWww.SendWebRequest();
                if (modelWww.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(modelWww.error);
                    loading = false;
                }
                var savedPath = string.Format("{0}/{1}.glb", Application.persistentDataPath, resultId);
                Debug.Log(savedPath);
                File.WriteAllBytes(savedPath, modelWww.downloadHandler.data);
                var models = new List<SavedModel>();
                var currentLocation = ARLocationProvider.Instance.Provider.CurrentLocation;
                if (PlayerPrefs.HasKey("MODELS"))
                {
                    models = new List<SavedModel>(PlayerPrefs.GetString("MODELS").Split(',').Select((e) => JsonUtility.FromJson<SavedModel>(e)));
                }
                var newSavedModel = new SavedModel()
                {
                    SavedPath = savedPath,
                    Latitude = currentLocation.latitude,
                    Longitude = currentLocation.longitude,
                };
                models.Add(newSavedModel);
                var list = String.Join(",", models.Select((e) => JsonUtility.ToJson(e)));
                PlayerPrefs.SetString("MODELS", list);
                PlayerPrefs.Save();

                LoadingText.text = "Отображение модели...";
                var model = Importer.LoadFromBytes(modelWww.downloadHandler.data);
                model.transform.localScale = new Vector3(-1, 1, 1);
                var newLocation = new Location()
                {
                    Latitude = currentLocation.latitude,
                    Longitude = currentLocation.longitude,
                    AltitudeMode =
                    AltitudeMode.DeviceRelative
                };
                PlaceAtLocation.CreatePlacedInstance(model, newLocation, PlacementOptions, false);

                InputField.SetTextWithoutNotify("");
                loading = false;
                DialogObject.SetActive(false);
                OtherUIObject.SetActive(false);
                PublishObject.SetActive(true);
                yield break;
            }
        }
    }
}


[Serializable]
public class GeneratedContent
{
    public int progress;
    public string video_url;
    public ModelUrls model_urls;
}

[Serializable]
public class ModelUrls
{
    public string glb;
}