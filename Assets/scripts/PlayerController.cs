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
    public GameObject formationPrefab;

    private GameObject formation;
    public float formationOffset = 6f;

    public bool fallIn = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // movement
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        var moveDirection = new Vector3(horz, 0, vert).normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        cam.transform.Translate(Vector3.forward * zoom * zoomSpeed * Time.deltaTime);
        var lookDir = moveDirection + transform.position;
        avatar.transform.LookAt(lookDir);
        avatar.transform.Rotate(0,transform.eulerAngles.y,0);

        if (Input.GetButtonUp("Fall In")) {
            fallIn = !fallIn;

            if (fallIn) {
                formation = Instantiate(
                    formationPrefab, 
                    avatar.transform.position + avatar.transform.forward * formationOffset, 
                    Quaternion.Inverse(avatar.transform.rotation));
            } else {
                Destroy(formation);
                formation = null;
            }
        }
    }
}
