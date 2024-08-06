using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTCTracker_Condition1 : MonoBehaviour
{
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity; // Returns current velocity of that ridgid body that contains a velocity in x,y,z direction
    }
}
