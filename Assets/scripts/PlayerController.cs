using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float zoomSpeed = 1f;
    public GameObject cam;
    public GameObject avatar;

    public bool fallIn = false;

    // Start is called before the first frame update
    void Start()
    {
        avatar = transform.Find("Avatar").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // movement
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        transform.Translate(horz * moveSpeed * Time.deltaTime, 0, vert * moveSpeed * Time.deltaTime);
        cam.transform.Translate(Vector3.forward * zoom * zoomSpeed * Time.deltaTime);
        var moveDirection = new Vector3(horz, 0, vert).normalized;
        var lookDir = moveDirection + transform.position;
        avatar.transform.LookAt(lookDir);

        if (Input.GetButtonDown("Fall In")) {
            fallIn = !fallIn;
        }
    }
}
