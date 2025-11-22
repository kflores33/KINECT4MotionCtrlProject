using UnityEngine;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    public bool SensorReady = false;
    bool canStart = false;
    public float waitTime = 3.0f;

    public Color green;

    public Image Stop;
    public Image Go;
    public Image MoveLeft;
    public Image MoveRight;
    public Image Right;
    public Image Left;

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

        InputTestDisplay();
    }

    public void SetSensorBool()
    {
        SensorReady = true;
    }

    void MoveToNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    void InputTestDisplay()
    {
        if (KinectInputSystem.GetButtonDown("Kinect_RightHandHorizontal"))
        {
            Debug.Log("Right Hand Horizontal Detected");
            Right.color = green;

            Left.color = Color.white;
            MoveLeft.color = Color.white;
            MoveRight.color = Color.white;
            Stop.color = Color.white;
            Go.color = Color.white;
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_LeftHandHorizontal"))
        {
            Debug.Log("Left Hand Horizontal Detected");

            Left.color = green;

            Right.color = Color.white;
            MoveLeft.color = Color.white;
            MoveRight.color = Color.white;
            Stop.color = Color.white;
            Go.color = Color.white;
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_RightHandForward"))
        {
            Stop.color = green;

            Left.color = Color.white;
            MoveLeft.color = Color.white;
            MoveRight.color = Color.white;
            Right.color = Color.white;
            Go.color = Color.white;
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_LeftLegSideRaise"))
        {
            Debug.Log("Left Leg Side Raise Detected");

            MoveLeft.color = green;

            Left.color = Color.white;
            Right.color = Color.white;
            MoveRight.color = Color.white;
            Stop.color = Color.white;
            Go.color = Color.white;
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_RightLegSideRaise"))
        {
            Debug.Log("Right Leg Side Raise Detected");
            MoveRight.color = green;

            Left.color = Color.white;
            MoveLeft.color = Color.white;
            Right.color = Color.white;
            Stop.color = Color.white;
            Go.color = Color.white;
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_RightHandDown"))
        {
            Debug.Log("Right Hand Down Detected");

            Go.color = green;

            Left.color = Color.white;
            MoveLeft.color = Color.white;
            MoveRight.color = Color.white;
            Stop.color = Color.white;
            Right.color = Color.white;
        }
    }
}
