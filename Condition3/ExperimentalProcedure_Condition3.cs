using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ExperimentalProcedure_Condition3 : MonoBehaviour
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
    public HTCTracker_Condition3 rightFootTracker;
    public HTCTracker_Condition3 leftFootTracker;


    [Header("Position Settings")]
    public Vector3 startPositionStepmarks = new Vector3(20, 0, 10);


    // Variables for Object pooling
    private StepmarkObjectPool rightStepmarkPool;
    private StepmarkObjectPool leftStepmarkPool;

    private List<GameObject> allStepmarks = new List<GameObject>();
    private int currentStepmarkIndex = 0;


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
        if (rightFootTracker == null || leftFootTracker == null)
        {
            Debug.LogError("Trackers are incorrectly assigned.");
            return;
        }

        UpdateStepmarkVisibility(); // Updating the visibility of the stepmarks
    }


    // Updating the visibility of the stepmarks based on the tracker positions
    private void UpdateStepmarkVisibility()
    {
        if (currentStepmarkIndex >= allStepmarks.Count) return;

        GameObject currentStepmark = allStepmarks[currentStepmarkIndex];
        HTCTracker_Condition3 relevantTracker = currentStepmarkIndex % 2 == 0 ? rightFootTracker : leftFootTracker;

        if (currentStepmark.transform.position.z <= relevantTracker.transform.position.z)
        {

            currentStepmark.SetActive(false); // Hide the current stepmark
            currentStepmarkIndex++; // Switch to the next index


            if (currentStepmarkIndex < allStepmarks.Count) // If there is a next stepmark, show it
            {
                allStepmarks[currentStepmarkIndex].SetActive(true);
            }
        }
        else
        {

            currentStepmark.SetActive(true); // Ensure that only the next (current) stepmark is visible

            for (int i = 0; i < allStepmarks.Count; i++) // Hide all other stepmarks
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