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
    public GameObject unitPrefab;

    private UnitController unit;
    public float unitOffset = 6f;

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

        // Move with unit
        if (unit && unit.forwardMarch) {
            transform.Translate(unit.forward * Constants.SOLDIER_BASE_MOVE_SPEED * Time.deltaTime);
        }

        // Look direction
        Aim();

        if (Input.GetButtonUp("Fall In")) FallIn();
        if (Input.GetButtonDown("Command Menu") && Commandable()) cmdMenu.SetActive(!cmdMenu.activeSelf);
        if (Input.GetButtonUp("Column March")) ColumnMarch();
        if (Input.GetButtonDown("Fire") && !cmdMenu.activeSelf) Fire();
        if (Input.GetButtonDown("Reload")) Reload();
    }

    private void Fire()
    {
        if (unit == null) return;
        StartCoroutine(unit.Fire());
    }

    private void Reload()
    {
        if (unit == null) return;
        unit.Reload();
    }

    private void ColumnMarch()
    {
        if (!Commandable()) return;
        var (success, position) = GetMousePosition();
        if (!success) return;

        var direction = position - avatar.transform.position;
        direction.y = 0;
        unit.ColumnDir(direction.normalized);
    }
    
    private void FallIn()
    {
        if (unit != null) { Destroy(unit.gameObject); }

        var rot = Quaternion.LookRotation(-avatar.transform.forward, Vector3.up);
        unit = Instantiate(
            unitPrefab,
            avatar.transform.position + avatar.transform.forward * unitOffset,
            rot
        ).GetComponent<UnitController>();
        unit.name = "UnitController";
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
        if (unit != null & unit.InCommandRange(transform.position)) { return true; }
        Debug.Log("Not Commandable");
        return false;
    }

    public void CommandForwardMarch()
    {
        if (Commandable()) {
            unit.ForwardMarch();
            cmdMenu.SetActive(false);
        } 
    }
    public void CommandHalt()
    {
        if (Commandable()) {
            unit.Halt();
            cmdMenu.SetActive(false);
        }
    }

    public void CommandRight()
    {
        if (Commandable()) {
            unit.Face(UnitController.Direction.Right);
        }
    }
    public void CommandLeft()
    {
        if (Commandable()) {
            unit.Face(UnitController.Direction.Left);
        }
    }
}

/* Facing movements
 * 
 * Movement
 *  • DONE forward march
 *  • DONE halt
 *  • charge
 *  • retreat
 * 
 * Firing
 *  • DONE Fire
 *  • Fire at will
 *  • Cease Fire
 *  • Fix Bayonets
 * 
 * Direction Change
 *  • DONE Column Left/Right
 *  • DONE Left and Right Face
 *  
 * Dispersion (close, normal, open) these two move through them
 *  • Open Ranks
 *  • Close Ranks
 *  
 * units
 *  • DONE Line
 *  • DONE Column
 *  • Square
 */