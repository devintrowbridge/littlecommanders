using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    public Vector3 start;
    private Vector3 end;
    public GameObject marker;

    public float speed = Constants.SOLDIER_BASE_MOVE_SPEED;
    public bool moving = false;

    public float maxDistanceFromMark = .5f;
    private float tolerableDistFromMark = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (marker != null) GetOnMark();
        else {
            moving = false;
        }
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
            tolerableDistFromMark = UnityEngine.Random.Range(.1f, maxDistanceFromMark);
            moving = true;
        }

        // If we're at the marker then stop moving and face forward
        if (!TooFarFromMarker() && moving) {
            moving = false;
            Facing();
        }

        // If we are moving, then translate our way over to the marker
        if (moving) {
            transform.LookAt(end);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
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
