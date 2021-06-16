//This script lets you change time scale during training. It is not a required script for this demo to function

using UnityEngine;

public class AdjustTrainingTimescale : MonoBehaviour
{
    public float startingTimescale=10f;
    void Start(){
        Time.timeScale = startingTimescale;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 1f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 2f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 3f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 4f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 5f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 6f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 7f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 8f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale = 9f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0) && Input.GetKey(KeyCode.LeftAlt))
        {
            Time.timeScale *= 2f;
        }
    }
}