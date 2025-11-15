using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class KinectInputSystem : MonoBehaviour
{
    public BodySourceManager bodySourceManager;
    
    [Header("Gesture Thresholds")]
    public float handRaiseThreshold = 0.3f;
    public float leanThreshold = 0.15f;
    public float swipeDistance = 0.5f;
    public float gestureHoldTime = 0.5f;
    
    // Virtual buttons state
    private Dictionary<string, bool> buttonStates = new Dictionary<string, bool>();
    private Dictionary<string, float> buttonHoldTimers = new Dictionary<string, float>();
    private Dictionary<ulong, Dictionary<string, bool>> userButtonStates = new Dictionary<ulong, Dictionary<string, bool>>();
    
    // Gesture tracking
    private Dictionary<ulong, Vector3> previousHandPositions = new Dictionary<ulong, Vector3>();
    
    public static KinectInputSystem Instance { get; private set; }
    
    // Define virtual buttons
    public readonly string[] VirtualButtons = {
        "Kinect_Jump",
        "Kinect_RightHandRaised",
        "Kinect_LeftHandRaised",
        "Kinect_LeanLeft", 
        "Kinect_LeanRight",
        "Kinect_SwipeRight",
        "Kinect_SwipeLeft",
        "Kinect_RightHandForward",
        "Kinect_LeftHandForward"
    };
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeButtonSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeButtonSystem()
    {
        foreach (string button in VirtualButtons)
        {
            buttonStates[button] = false;
            buttonHoldTimers[button] = 0f;
        }
    }
    
    void Update()
    {
        ResetButtonStates();
        ProcessKinectInput();
        UpdateButtonTimers();
    }
    
    void ResetButtonStates()
    {
        // Reset all buttons at start of frame
        foreach (string button in VirtualButtons)
        {
            buttonStates[button] = false;
        }
    }
    
    void ProcessKinectInput()
    {
        if (bodySourceManager == null) return;

        Windows.Kinect.Body[] bodies = bodySourceManager.GetData();
        if (bodies == null) return;
        
        foreach (var body in bodies)
        {
            if (body != null && body.IsTracked)
            {
                ProcessBodyGestures(body);
            }
        }
    }
    
    void ProcessBodyGestures(Windows.Kinect.Body body)
    {
        ulong trackingId = body.TrackingId;
        
        // Get key joints
        var spineMid = body.Joints[Kinect.JointType.SpineMid];
        var rightHand = body.Joints[Kinect.JointType.HandRight];
        var leftHand = body.Joints[Kinect.JointType.HandLeft];
        var head = body.Joints[Kinect.JointType.Head];
        var spineBase = body.Joints[Kinect.JointType.SpineBase];
        var spineShoulder = body.Joints[Kinect.JointType.SpineShoulder];
        
        // Check for raised hands (button press)
        if (rightHand.Position.Y > head.Position.Y + handRaiseThreshold)
        {
            SetButtonPressed("Kinect_RightHandRaised");
        }
        
        if (leftHand.Position.Y > head.Position.Y + handRaiseThreshold)
        {
            SetButtonPressed("Kinect_LeftHandRaised");
        }
        
        // Check for jumping
        if (spineBase.Position.Y > body.Joints[Kinect.JointType.FootLeft].Position.Y + 0.2f)
        {
            SetButtonPressed("Kinect_Jump");
        }
        
        // Check for leaning
        if (spineMid.Position.X < spineShoulder.Position.X - leanThreshold)
        {
            SetButtonPressed("Kinect_LeanLeft");
        }
        else if (spineMid.Position.X > spineShoulder.Position.X + leanThreshold)
        {
            SetButtonPressed("Kinect_LeanRight");
        }
        
        // Check for forward hand movements
        if (rightHand.Position.Z < spineShoulder.Position.Z - 0.3f)
        {
            SetButtonPressed("Kinect_RightHandForward");
        }
        
        if (leftHand.Position.Z < spineShoulder.Position.Z - 0.3f)
        {
            SetButtonPressed("Kinect_LeftHandForward");
        }
        
        // Swipe detection
        DetectSwipes(body, trackingId);
    }
    
    void DetectSwipes(Windows.Kinect.Body body, ulong trackingId)
    {
        var currentRightHandPos = GetVector3FromJoint(body.Joints[Kinect.JointType.HandRight]);
        
        if (!previousHandPositions.ContainsKey(trackingId))
        {
            previousHandPositions[trackingId] = currentRightHandPos;
            return;
        }
        
        Vector3 previousPos = previousHandPositions[trackingId];
        Vector3 delta = currentRightHandPos - previousPos;
        
        // Check for horizontal swipe
        if (Mathf.Abs(delta.x) > swipeDistance && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0)
            {
                SetButtonPressed("Kinect_SwipeRight");
            }
            else
            {
                SetButtonPressed("Kinect_SwipeLeft");
            }
        }
        
        previousHandPositions[trackingId] = currentRightHandPos;
    }
    
    void SetButtonPressed(string buttonName)
    {
        buttonStates[buttonName] = true;
        buttonHoldTimers[buttonName] = gestureHoldTime;
    }
    
    void UpdateButtonTimers()
    {
        foreach (string button in VirtualButtons)
        {
            if (buttonHoldTimers[button] > 0)
            {
                buttonHoldTimers[button] -= Time.deltaTime;
            }
        }
    }
    
    // Public API for other scripts to use - mimics Input.GetButton methods
    
    public static bool GetButton(string buttonName)
    {
        if (Instance == null) return false;
        return Instance.buttonStates.ContainsKey(buttonName) && Instance.buttonStates[buttonName];
    }
    
    public static bool GetButtonDown(string buttonName)
    {
        if (Instance == null) return false;
        
        // For GetButtonDown, we check if the button was pressed this frame
        // and the hold timer is fresh
        if (Instance.buttonStates.ContainsKey(buttonName) && 
            Instance.buttonStates[buttonName] && 
            Instance.buttonHoldTimers[buttonName] >= Instance.gestureHoldTime - Time.deltaTime)
        {
            return true;
        }
        return false;
    }
    
    public static bool GetButtonUp(string buttonName)
    {
        // This is trickier with gestures, but you can implement based on your needs
        // For now, return false or implement based on gesture release detection
        return false;
    }
    
    public static float GetButtonHoldTime(string buttonName)
    {
        if (Instance == null) return 0f;
        return Instance.buttonHoldTimers.ContainsKey(buttonName) ? Instance.buttonHoldTimers[buttonName] : 0f;
    }
    
    private Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}