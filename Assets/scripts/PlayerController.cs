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
    private Camera cam;
    public GameObject avatar;
    public GameObject formationPrefab;

    private FormationController formation;
    public float formationOffset = 6f;

    public bool fallIn = false;

    public Material mat;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
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

        // Look direction
        Aim();

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

    private void Aim()
    {
        var (success, position) = GetMousePosition();
        if (success) {
            // Calculate the direction
            var direction = position - avatar.transform.position;

            // You might want to delete this line.
            // Ignore the height difference.
            direction.y = 0;

            // Make the transform look in the direction.
            avatar.transform.forward = direction;
        }
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity)) {
            // The Raycast hit something, return with the position.
            return (success: true, position: hitInfo.point);
        }
        else {
            // The Raycast did not hit anything.
            return (success: false, position: Vector3.zero);
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
 *  • Left and Right Face
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