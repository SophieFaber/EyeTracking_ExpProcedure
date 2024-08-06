using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentalProcedure_Condition2 : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject circlePrefab;
    public Color stepmarkColor = Color.green;

    [Header("Step Settings")]
    public float strideLength = 1.0f; 
    public float stepWidth = 0.2f; 
    public float yOffset = 0.01f; 
    public int trialNumber = 100; 

    [Header("Tracker Settings")]
    public HTCTracker_Condition2 rightFootTracker; 
    public HTCTracker_Condition2 leftFootTracker; 
    public float velocityTh = 1.0f; 

    [Header("Position Settings")]
    public Vector3 startPositionStepmarks = new Vector3(20, 0, 10); 

    // Variables for Object pooling
    private StepmarkObjectPool rightStepmarkPool;
    private StepmarkObjectPool leftStepmarkPool;

    // List for all stepmarks (left and right) 
    private List<GameObject> allStepmarks = new List<GameObject>();
    // Index for the currently displayed stepmark
    private int currentStepmarkIndex = 0;

    // Variables for caching and optimization
    private float lastVelocityCheckTime;
    private const float velocityCheckInterval = 0.1f; // Checking velocity every 0.1seconds -> const to define a constant

    private void Start()
    {
        if (circlePrefab == null) // Checking whether prefab was chosen correctly
        {
            Debug.LogError("Prefab is not assigned correctly.");
            return;
        }

        // Initializing Objectpools for right and left stepmarks
        rightStepmarkPool = new StepmarkObjectPool(circlePrefab, trialNumber);
        leftStepmarkPool = new StepmarkObjectPool(circlePrefab, trialNumber);

        // Starting coroutine for initialization of stepmarks
        StartCoroutine(InitializeStepmarks());
    }

    // Updated Method to initialize the stepmarksn
    private IEnumerator InitializeStepmarks()
    {
        Vector3 rightStepmarkPosition = startPositionStepmarks + new Vector3(stepWidth / 2, yOffset, 0);
        Vector3 leftStepmarkPosition = startPositionStepmarks + new Vector3(-stepWidth / 2, yOffset, strideLength / 2);

        for (int i = 0; i < trialNumber * 2; i++) // Twice the number for both feet
        {
            bool isRightFoot = i % 2 == 0;
            Vector3 stepmarkPosition = isRightFoot ? rightStepmarkPosition : leftStepmarkPosition;

            GameObject stepmark = isRightFoot ? rightStepmarkPool.GetObject() : leftStepmarkPool.GetObject();
            stepmark.transform.position = stepmarkPosition + new Vector3(0, 0, (i / 2) * strideLength);
            stepmark.GetComponent<SpriteRenderer>().color = stepmarkColor;
            stepmark.SetActive(false);

            allStepmarks.Add(stepmark);

            // Distributing load over multiple frames to prevent drops in performance
            if (i % 10 == 0) yield return null; // Is done every 10 iterations
        }
    }

    // FixedUpdate() used for physics based updates
    private void FixedUpdate()
    {
        // Checking whether foot trackers are assigned correctly
        if (rightFootTracker == null || leftFootTracker == null)
        {
            Debug.LogError("Trackers are incorrectly assigned.");
            return;
        }

        float currentTime = Time.time; // capturing the current time
        if (currentTime - lastVelocityCheckTime >= velocityCheckInterval) // Checking velocity in fixed intervals
        {
            CheckVelocityAndUpdateStepmarks(); // Checking the velocity for both feet
            lastVelocityCheckTime = currentTime; // Updating the time of the last velocity check
        }

        UpdateStepmarkVisibility(); // Updating the visibility of the stepmarks
    }

    // Checking the velocity of both feed and updating the stepmarks accordingly
    private void CheckVelocityAndUpdateStepmarks()
    {
        Vector3 rightVelocity = rightFootTracker.GetVelocity();
        Vector3 leftVelocity = leftFootTracker.GetVelocity();

        if (rightVelocity.magnitude > velocityTh && leftVelocity.magnitude > velocityTh)
        {
            ClearAllStepmarks();
            currentStepmarkIndex = 0;
        }
    }

    // Updating the visibility of the stepmarks based on the tracker positions
    private void UpdateStepmarkVisibility()
    {
        if (currentStepmarkIndex >= allStepmarks.Count) return;

        GameObject currentStepmark = allStepmarks[currentStepmarkIndex];
        HTCTracker_Condition2 relevantTracker = currentStepmarkIndex % 2 == 0 ? rightFootTracker : leftFootTracker; // Must be referenced to the current HTCTracker_(current version) script here!

        // Checking whether the tracker reached the current stepmark
        if (currentStepmark.transform.position.z <= relevantTracker.transform.position.z)
        {
            // Hide the current stepmark
            currentStepmark.SetActive(false);

            // Switch to the next index
            currentStepmarkIndex++;

            // If there is a next stepmark, show it
            if (currentStepmarkIndex < allStepmarks.Count)
            {
                allStepmarks[currentStepmarkIndex].SetActive(true);
            }
        }
        else
        {
            // Ensure that only the next (current) stepmark is visible
            currentStepmark.SetActive(true);

            // Hide all other stepmarks
            for (int i = 0; i < allStepmarks.Count; i++)
            {
                if (i != currentStepmarkIndex)
                {
                    allStepmarks[i].SetActive(false);
                }
            }
        }
    }

    // Remove all stepmarks and give them back to the according pool
    private void ClearAllStepmarks()
    {
        foreach (GameObject stepmark in allStepmarks)
        {
            stepmark.SetActive(false);
            if (stepmark.transform.position.x > 0)
                rightStepmarkPool.ReturnObject(stepmark);
            else
                leftStepmarkPool.ReturnObject(stepmark);
        }
        allStepmarks.Clear();
    }
}

// Class for object pooling (unchanged)
public class StepmarkObjectPool
{
    private Queue<GameObject> objects = new Queue<GameObject>();
    private GameObject prefab;

    // Constructor: creating pool with certain initial size
    public StepmarkObjectPool(GameObject prefab, int initialSize)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            objects.Enqueue(obj);
        }
    }

    // Picking an object from pool or creating a new one, if pool is empty
    public GameObject GetObject()
    {
        if (objects.Count > 0)
            return objects.Dequeue();
        return Object.Instantiate(prefab);
    }

    // Returning object back to pool
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        objects.Enqueue(obj);
    }
}