// similarities: both inherit from MonoBehaviour; have a (rb) component; offer a GetVelocity()-Method
// differences: "HTCTracker_2907" w/ additional functionality to control visibility; "HTCTracker_2907" uses Awake instead of Start; "HTCTracker_2907" w/ dependency to different script (ExperimentalProcedure_(current version))

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTCTracker_Condition2 : MonoBehaviour
{
    private Rigidbody rb; // Rigidbody component for motion capturing
    private Renderer[] renderers; // Array for all renderer components of trackers
    [SerializeField] private ExperimentalProcedure_Condition2 otherScript; // Reference to other script containing the velocityTh variable
    [SerializeField] private bool useVisibilityControl = false; // Switch for controling visibility

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Get and save the rb component
        
        if (useVisibilityControl) // Initializing additional components as visibility control is active
        {
            renderers = GetComponentsInChildren<Renderer>(); // Get and save all renderer components
            if (otherScript == null) // Check whether the reference to the other script was set correctly
            {
                Debug.LogError("OtherScript reference is not set in the inspector!");
            }
        }
    }

    private void Update()
    {
        if (useVisibilityControl) // Using visibility control only if active
        {
            CheckVelocityAndUpdateVisibility();
        }
    }

    private void CheckVelocityAndUpdateVisibility() // Check velocity and update the visibility accordingly
    {
        bool isVisible = rb.velocity.sqrMagnitude <= otherScript.velocityTh * otherScript.velocityTh; // Compare sqr of the velocity w/ sqr of the th -> more efficient as calculating actual magnitude
        SetVisibility(isVisible); 
    }

    private void SetVisibility(bool isVisible) // Set visibility of trackers
    {
        foreach (var renderer in renderers) // Runs through all renderer and sets their visibility
        {
            renderer.enabled = isVisible;
        }
    }

    public Vector3 GetVelocity() => rb.velocity; // Public method to call the current velocity
}

