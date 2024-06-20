using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using System.Collections.Generic;
using System.Linq;

public class SpawnerOnMap : MonoBehaviour
{
    [SerializeField]
    AbstractMap _map;
    Vector2d[] _locations;

    [SerializeField]
    float _spawnScale = 1f;

    [SerializeField]
    GameObject _markerPrefab;

    List<GameObject> _spawnedObjects;

    private void Start()
    {
        // mock placemarks
        List<SavedModel> models = new List<SavedModel>() {
                new SavedModel() {
                    Latitude = 56.630848,
                    Longitude = 47.890566,
                }
            };
        if (PlayerPrefs.HasKey("saved_models"))
        {
            models = new List<SavedModel>(PlayerPrefs.GetString("saved_models").Split('|').Select((e) => JsonUtility.FromJson<SavedModel>(e)));
        }
        _locations = new Vector2d[models.Count];
        _spawnedObjects = new List<GameObject>();
        for (int i = 0; i < models.Count; i++)
        {
            Debug.Log($"SavedModel: {models[i].Latitude} {models[i].Longitude} {models[i].SavedPath}");
            _locations[i] = new Vector2d(models[i].Latitude, models[i].Longitude);
            var instance = Instantiate(_markerPrefab);
            instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);
            instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            _spawnedObjects.Add(instance);
        }
    }

    private void Update()
    {
        int count = _spawnedObjects.Count;
        for (int i = 0; i < count; i++)
        {
            var spawnedObject = _spawnedObjects[i];
            var location = _locations[i];
            spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
            spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
        }
    }
}
