using UnityEngine;

public class PlayerController : Commander 
{ 
    public float zoomSpeed = 1f;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    protected override void LateUpdate()
    {
        base.LateUpdate();

        // movement
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        cam.transform.Translate(Vector3.forward * zoom * zoomSpeed * Time.deltaTime);

        // move in direction of inputs
        var moveDirection = new Vector3(horz, 0, vert).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Input.GetButtonUp("Fall In")) FallIn();
        if (Input.GetButtonDown("Command Menu") && Commandable()) cmdMenu.SetActive(!cmdMenu.activeSelf);
        if (Input.GetButtonUp("Column March")) ColumnMarch();
        if (Input.GetButtonDown("Fire") && !cmdMenu.activeSelf) Fire();
        if (Input.GetButtonDown("Reload")) Reload();
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

    protected override (bool success, Vector3 position) GetLookPosition()
    {
        return GetMousePosition();
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