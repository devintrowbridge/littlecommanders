using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float accSpeed = 10f;
    public float zoomSpeed = 1f;
    public GameObject cam;
    public GameObject avatar;
    public GameObject formationPrefab;

    private FormationController formation;
    public float formationOffset = 6f;

    public bool fallIn = false;

    public Material mat;


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

        cam.transform.Translate(Vector3.forward * zoom * zoomSpeed * Time.deltaTime);
        var lookDir = moveDirection + transform.position;

        // move in direction of inputs
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        avatar.transform.LookAt(lookDir);
        avatar.transform.Rotate(0,transform.eulerAngles.y,0);

        if (Input.GetButtonUp("Fall In")) {
            fallIn = !fallIn;

            if (fallIn) {
                var rot = Quaternion.LookRotation(- avatar.transform.forward, Vector3.up);

                formation = Instantiate(
                    formationPrefab, 
                    avatar.transform.position + avatar.transform.forward * formationOffset,
                    rot
                ).GetComponent<FormationController>();
                formation.SetColor(mat);
            } else {
                Destroy(formation.gameObject);
                formation = null;
            }
        }
    }
}

/* Facing movements
 * 
 * Movement
 *  • forward march
 *  • halt
 *  • charge
 *  • retreat
 * 
 * Firing
 *  • Fire
 *  • Fire at will
 *  • Cease Fire
 *  • Fix Bayonets
 * 
 * Direction Change
 *  • Column Left/Right
 *  
 * Dispersion (close, normal, open) these two move through them
 *  • Open Ranks
 *  • Close Ranks
 *  
 * Formations
 *  • Line
 *  • Column
 *  • Square
 */