using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    public Vector3 start;
    private Vector3 end;
    public GameObject marker;

    public float speed = 10f;
    public float acc = 10f;
    public bool moving = false;
    private float startTime;
    private float journeyLength;

    public float maxDistanceFromMark = .5f;
    private float tolerableDistFromMark = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (marker != null) GetOnMark();
        else moving = false;
    }

    public void ClearColor()
    {
        transform.Find("Body").GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
    }

    public void SetColor(Material mat)
    {
        transform.Find("Body").GetComponent<MeshRenderer>().material = mat;
    }

    void GetOnMark()
    {
        end = marker.transform.position;

        // If we're too far away and haven't start moving, then start moving
        if (TooFarFromMarker() && !moving) {
            Debug.Log("starting");
            tolerableDistFromMark = UnityEngine.Random.Range(.1f, maxDistanceFromMark);
            start = transform.position;
            journeyLength = Vector3.Distance(start, marker.transform.position);
            startTime = Time.time;
            moving = true;
        }

        // If we're at the marker then stop moving and face forward
        if (!TooFarFromMarker() && moving) {
            Debug.Log("stopping");
            moving = false;
            Facing();
        }

        // If we are moving, then lerp our way over to the marker
        if (moving) {
            Debug.Log("Moving");
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(start, end, fractionOfJourney);
            transform.LookAt(end);
        }
    }

    bool TooFarFromMarker()
    {
        return (transform.position - end).magnitude > tolerableDistFromMark;
    }

    void Facing()
    {
        transform.rotation = marker.transform.rotation;
    }
}
