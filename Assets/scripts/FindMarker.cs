using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindMarker : MonoBehaviour
{
    public PlayerController playerController;
    public Vector3 startMarker;
    public Vector3 endMarker;

    public float speed = 1.0f;
    public bool moving = false;
    private float startTime;
    private float journeyLength;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController != null && playerController.fallIn) {
            if (moving) {
                float distCovered = (Time.time - startTime) * speed;
                float fractionOfJourney = distCovered / journeyLength;
                transform.position = Vector3.Lerp(startMarker, endMarker, fractionOfJourney);
                transform.LookAt(endMarker);

                if ((transform.position - endMarker).magnitude < .2f) {
                    moving = false;
                }
            } 
            
            if(!moving && endMarker == Vector3.zero) {
                startMarker = transform.position;
                startTime = Time.time;
                var markers = GameObject.FindGameObjectsWithTag("Marker");
                (endMarker, journeyLength) = GetClosestMarker(markers);
                moving = true;
            }
        }

        if (!playerController.fallIn) {
            moving = false;
            endMarker = Vector3.zero;
        }
    }

    (Vector3, float) GetClosestMarker(GameObject[] markers) 
    {
        GameObject tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject t in markers) {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist) {
                tMin = t;
                minDist = dist;
            }
        }

        return (tMin.transform.position, minDist);
    }
}
