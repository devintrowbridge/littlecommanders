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
        if (formation && formation.forwardMarch) {
            transform.Translate(formation.forward * Constants.SOLDIER_BASE_MOVE_SPEED * Time.deltaTime);
        }

        // Look direction
        Aim();

        if (Input.GetButtonUp("Fall In")) FallIn();
        if (Input.GetButtonDown("Command Menu") && Commandable()) cmdMenu.SetActive(!cmdMenu.activeSelf);
        if (Input.GetButtonUp("Column March")) ColumnMarch();
        if (Input.GetButtonDown("Fire")) Fire();
    }

    private void Fire()
    {
        if (formation == null) return;

        formation.Fire();
    }

    private void ColumnMarch()
    {
        if (!Commandable()) return;
        var (success, position) = GetMousePosition();
        if (!success) return;

        var direction = position - avatar.transform.position;
        direction.y = 0;
        formation.ColumnDir(direction.normalized);
    }
    
    private void FallIn()
    {
        if (formation != null) { Destroy(formation.gameObject); }

        var rot = Quaternion.LookRotation(-avatar.transform.forward, Vector3.up);
        formation = Instantiate(
            formationPrefab,
            avatar.transform.position + avatar.transform.forward * formationOffset,
            rot
        ).GetComponent<FormationController>();
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
        if (formation != null & formation.InCommandRange(transform.position)) { return true; }
        Debug.Log("Not Commandable");
        return false;
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

    public void CommandRight()
    {
        if (Commandable()) {
            formation.Face(FormationController.Direction.Right);
        }
    }
    public void CommandLeft()
    {
        if (Commandable()) {
            formation.Face(FormationController.Direction.Left);
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