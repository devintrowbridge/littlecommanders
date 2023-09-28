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
    private bool followFormation = false;

    public bool fallIn = false;

    public Material mat;

    public GameObject cmdMenu;
    public float maxCommandDist = 10f;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // movement
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        cam.transform.Translate(Vector3.forward * zoom * zoomSpeed * Time.deltaTime);

        // move in direction of inputs
        var moveDirection = new Vector3(horz, 0, vert).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Move with formation
        if (formation && followFormation && formation.forwardMarch) {
            transform.Translate(formation.transform.forward * Constants.SOLDIER_BASE_MOVE_SPEED * Time.deltaTime);
        }

        // Look direction
        Aim();

        if (Input.GetButtonUp("Fall In")) FallIn();
        if (Input.GetButtonDown("Command Menu") && Commandable()) cmdMenu.SetActive(!cmdMenu.activeSelf);
    }
    
    private void FallIn()
    {
        fallIn = !fallIn;

        if (fallIn) {
            var rot = Quaternion.LookRotation(-avatar.transform.forward, Vector3.up);

            formation = Instantiate(
                formationPrefab,
                avatar.transform.position + avatar.transform.forward * formationOffset,
                rot
            ).GetComponent<FormationController>();
            formation.SetColor(mat);
        }
        else {
            Destroy(formation.gameObject);
            formation = null;
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

    private bool Commandable()
    {
        return formation != null & formation.InCommandRange(transform.position);
    }

    public void CommandForwardMarch()
    {
        if (Commandable()) {
            formation.ForwardMarch();
            cmdMenu.SetActive(false);
        }
    }
    public void CommandHalt()
    {
        if (Commandable()) {
            formation.Halt();
            cmdMenu.SetActive(false);
        }
    }

    public void Follow()
    {
        if (Commandable()) {
            followFormation = !followFormation;
            cmdMenu.SetActive(false);
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