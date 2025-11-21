using UnityEngine;

public class TitleSceneManager : MonoBehaviour
{
    public bool SensorReady = false;
    bool canStart = false;
    public float waitTime = 3.0f;

    private void Update()
    {
        if (!canStart && SensorReady)
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                canStart = true;
            }
        }
        else if (!SensorReady)
        {
            waitTime = 3.0f;
        }

        if (KinectInputSystem.GetButtonDown("Kinect_Bow") && canStart)
        {
            MoveToNextScene();
        }
    }

    public void SetSensorBool()
    {
        SensorReady = true;
    }

    void MoveToNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
