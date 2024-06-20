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
using Mapbox.Unity.Location;

public class SendRequest : MonoBehaviour
{
    public GameObject DialogObject;
    public TMP_InputField InputField;
    public GameObject LoadingObject;
    public Text LoadingText;
    public GameObject PublishObject;
    public GameObject OtherUIObject;
    public bool isLike = false;

    private bool loading = false;
    private string resultId = "";

    private void Start()
    {
        if (!isLike) return;

        List<SavedModel> models = new List<SavedModel>();
        if (PlayerPrefs.HasKey("saved_models"))
            models = new List<SavedModel>(PlayerPrefs.GetString("saved_models").Split('|').Select((e) => JsonUtility.FromJson<SavedModel>(e)));
        if (models.Count == 0) return;

        var currentLocation = ARLocationProvider.Instance.Provider.CurrentLocation;
        var nearestModel = LocationHelper.FindNearestModel(currentLocation.latitude, currentLocation.longitude, models);
        Debug.Log($"{nearestModel.Latitude} {nearestModel.Longitude}  {nearestModel.SavedPath}");
        byte[] file = File.ReadAllBytes(nearestModel.SavedPath);
        var arObject = Importer.LoadFromBytes(file);
        var scale = arObject.transform.localScale;
        arObject.transform.localScale = new Vector3(scale.x * 0.5f, scale.y * 0.5f, scale.z * 0.5f);
        var newLocation = new ARLocation.Location()
        {
            Latitude = nearestModel.Latitude,
            Longitude = nearestModel.Longitude
        };
        PlaceAtLocation.CreatePlacedInstance(arObject, newLocation, new PlaceAtLocation.PlaceAtOptions(), false);

    }

    private void Update()
    {
        if (isLike) return;

        LoadingObject.SetActive(loading);
        if (loading) OtherUIObject.SetActive(false);
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
                if (PlayerPrefs.HasKey("saved_models"))
                {
                    models = new List<SavedModel>(PlayerPrefs.GetString("saved_models").Split('|').Select((e) => JsonUtility.FromJson<SavedModel>(e)));
                }
                var savedModel = new SavedModel()
                {
                    SavedPath = savedPath,
                    Latitude = currentLocation.latitude,
                    Longitude = currentLocation.longitude,
                };
                models.Add(savedModel);
                var list = String.Join("|", models.Select((e) => JsonUtility.ToJson(e)));
                PlayerPrefs.SetString("saved_models", list);
                PlayerPrefs.Save();

                LoadingText.text = "Отображение модели...";
                var importedModel = Importer.LoadFromBytes(modelWww.downloadHandler.data);
                var scale = importedModel.transform.localScale;
                importedModel.transform.localScale = new Vector3(scale.x * 0.5f, scale.y * 0.5f, scale.z * 0.5f);
                var newLocation = new ARLocation.Location()
                {
                    Latitude = savedModel.Latitude,
                    Longitude = savedModel.Longitude
                };
                var instance = PlaceAtLocation.CreatePlacedInstance(importedModel, newLocation, new PlaceAtLocation.PlaceAtOptions(), false);

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