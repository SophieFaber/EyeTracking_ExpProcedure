// similarities: both inherit from MonoBehaviour; have a (rb) component; offer a GetVelocity()-Method
// differences: "HTCTracker_2907" w/ additional functionality to control visibility; "HTCTracker_2907" uses Awake instead of Start; "HTCTracker_2907" w/ dependency to different script (ExperimentalProcedure_(current version))

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class HTCTracker_Condition3 : MonoBehaviour
{
    private Rigidbody rb; // Rigidbody component for motion capturing
    private Renderer[] renderers; // Array for all renderer components of trackers
    [SerializeField] private ExperimentalProcedure_Condition3 otherScript; // Reference to other script containing the velocityTh variable
    [SerializeField] private bool useVisibilityControl = false; // Switch for controling visibility

    [Header("Lateral Shift")]
    public float lateralShift = 0f; // Positive for right foot and negative for left foot
    public bool isRightFoot = true; // True for the right foot and false for the left foot

    private Vector3 originalPosition;
    private Transform visualTransform;  

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
        
        if (useVisibilityControl) 
        {
            renderers = GetComponentsInChildren<Renderer>(); 
            if (otherScript == null) 
            {
                Debug.LogError("OtherScript reference is not set in the inspector!");
            }
        }

        // Create a new GameObject for the visual representation
        GameObject visualObject = new GameObject(nameof + "_Visual");
        visualTransform = visualObject.transform;
        visualTransform.SetParent(transform);
        visualTransform.localPosition = Vector3.zero;
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = Vector3.one;


        // Shifting all renderer components to the visual object
        foreach (var renderer in renderers)
        {
            renderer.transform.SetParent(visualTransform);
        }

        originalPosition = transform.position;
    }

    private void Update()
    {
        if (useVisibilityControl) 
        {
            CheckVelocityAndUpdateVisibility();
        }

        UpdateVisualPosition();
    }

    private void UpdateVisualPosition()
    {
        Vector3 shift = isRightFoot ? Vector3.right * lateralShift : Vector3.left * lateralShift;
        visualTransform.position = visualTransform.position + shift;
    }

    private void CheckVelocityAndUpdateVisibility() 
    {
        bool isVisible = rb.velocity.sqrMagnitude <= otherScript.velocityTh * otherScript.velocityTh; 
        SetVisibility(isVisible); 
    }

    private void SetVisibility(bool isVisible) 
    {
        foreach (var renderer in renderers) 
        {
            renderer.enabled = isVisible;
        }
    }

    public Vector3 GetVelocity() => rb.velocity; // Public method to call the current velocity

    // New method to get the different positions
    public Vector3 GetShiftedPosition()
    {
        return visualTransform.position;
    }
}

