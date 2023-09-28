using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Soldier : MonoBehaviour
{
    public Vector3 start;
    private Vector3 end;
    public GameObject marker;

    public float speed = Constants.SOLDIER_BASE_MOVE_SPEED;
    public bool moving = false;

    public float maxDistanceFromMark = .5f;
    private float tolerableDistFromMark = 0;

    private AudioSource gunshot;
    private ParticleSystem gunsmoke;
    private ParticleSystem gunspark;


    // Start is called before the first frame update
    void Start()
    {
        tolerableDistFromMark = UnityEngine.Random.Range(.1f, maxDistanceFromMark);
        gunshot = transform.Find("Gun/Gunshot").GetComponent<AudioSource>();
        gunsmoke = transform.Find("Gun/Gunsmoke").GetComponent<ParticleSystem>();
        gunspark = transform.Find("Gun/Gunspark").GetComponent<ParticleSystem>();
    }

    private IEnumerator Fire(float delay)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, delay));
        gunshot.Play();
        gunsmoke.Play();
        gunspark.Play();

        var raycast = Physics.Raycast(
            transform.position,
            transform.forward,
            out var hit,
            Mathf.Infinity,
            Constants.LAYER_SOLDIER
        );


        if (raycast) {
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
        }
    }

    public void Fire()
    {
        var delay = UnityEngine.Random.Range(0f, 1f);
        StartCoroutine(Fire(delay));
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
            speed *= 1.5f; // catchup speed
            moving = true;
        }

        // If we're at the marker then stop moving and face forward
        if (!TooFarFromMarker()) {
            moving = false;
            speed = Constants.SOLDIER_BASE_MOVE_SPEED;
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
