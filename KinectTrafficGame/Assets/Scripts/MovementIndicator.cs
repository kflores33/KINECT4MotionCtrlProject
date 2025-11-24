using UnityEngine;
using UnityEngine.UI;

public class MovementIndicator : MonoBehaviour
{
    public bool SensorReady = false;
    bool canStart = false;
    public float waitTime = 3.0f;

    public Color green;

    public Image displayImage;
    public Image defaultImage;
    public Image[] wayImages;

    private void Start()
    {
        displayImage = defaultImage;
        foreach (Image img in wayImages)
        {
            img.color = Color.green;
        }
    }

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

        InputTestDisplay();

    }

    public void SetSensorBool()
    {
        SensorReady = true;
    }

    void InputTestDisplay()
    {
        if (KinectInputSystem.GetButtonDown("Kinect_RightHandHorizontal"))
        {
            displayImage = wayImages[2];

        }
        else if (KinectInputSystem.GetButtonDown("Kinect_LeftHandHorizontal"))
        {
            displayImage = wayImages[3];
            Debug.Log("Left Hand Horizontal Detected");

        }
        else if (KinectInputSystem.GetButtonDown("Kinect_RightHandForward"))
        {
            displayImage = wayImages[0];
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_LeftLegSideRaise"))
        {
            displayImage = wayImages[4];
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_RightLegSideRaise"))
        {
            displayImage = wayImages[5];
        }
        else if (KinectInputSystem.GetButtonDown("Kinect_RightHandDown"))
        {
            displayImage = wayImages[1];
        }
        else
        {
            displayImage = defaultImage;
        }
    }
}
