using UnityEngine.SceneManagement;
using UnityEngine;

public class EventPointer : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] float amplitude = 2.0f;
    [SerializeField] float frequency = 0.5f;


    void Update()
    {
        RotatePointer();
    }

    void RotatePointer()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude) + 15, transform.position.z);
    }

    private void OnMouseDown()
    {
        SceneManager.LoadScene("LikeScene", LoadSceneMode.Single);
    }
}