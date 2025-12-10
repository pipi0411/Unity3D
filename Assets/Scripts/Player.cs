using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Player script has started.");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Player script is updating.");
    }
    private void FixedUpdate()
    {
        Debug.Log("Player script is fixed updating.");
    }
}
