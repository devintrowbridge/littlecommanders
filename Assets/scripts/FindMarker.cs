using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindMarker : MonoBehaviour
{
    private PlayerController playerController;
    public Vector3 start;
    private Vector3 end;
    public GameObject marker;

    public float speed = 1.0f;
    public bool moving = false;
    private float startTime;
    private float journeyLength;

    public float distanceFromMark = .5f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(marker);
        if (marker != null) {
            end = marker.transform.position;
            end.y = start.y;
            
            // If we're too far away and haven't start moving, then start moving
            if (TooFarFromMarker() && !moving) {
                start = transform.position;
                journeyLength = Vector3.Distance(start, marker.transform.position);
                startTime = Time.time;
                moving = true;
            }

            // If we're at the marker then stop moving
            if (!TooFarFromMarker()) {
                moving = false;
            }

            // If we are moving, then lerp our way over to the marker
            if (moving) {
                float distCovered = (Time.time - startTime) * speed;
                float fractionOfJourney = distCovered / journeyLength;
                transform.position = Vector3.Lerp(start, end, fractionOfJourney);
                transform.LookAt(end);
                transform.Rotate(0, transform.eulerAngles.y, 0);
            }
        }
    }

    bool TooFarFromMarker() {
        return (transform.position - end).magnitude > distanceFromMark;
    }
}
