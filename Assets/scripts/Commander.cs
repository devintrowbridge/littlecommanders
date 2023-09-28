using UnityEngine;

public abstract class Commander : MonoBehaviour {
    public float moveSpeed = 10f;
    public float accSpeed = 10f;

    public GameObject avatar;
    public GameObject unitPrefab;

    protected UnitController unit;
    public float unitOffset = 6f;

    public Material mat;

    public GameObject cmdMenu;
    public float maxCommandDist = 10f;


    protected abstract (bool success, Vector3 position) GetLookPosition();

    protected virtual void LateUpdate()
    {
        // Move with unit
        if (unit && unit.forwardMarch) {
            transform.Translate(unit.travelVec);
        }

        // Look direction
        Aim();
    }

    protected virtual void Fire()
    {
        if (unit == null) return;
        StartCoroutine(unit.Fire());
    }

    protected virtual void Reload()
    {
        if (unit == null) return;
        unit.Reload();
    }

    protected virtual void ColumnMarch()
    {
        if (!Commandable()) return;
        var (success, position) = GetLookPosition();
        if (!success) return;

        var direction = position - avatar.transform.position;
        direction.y = 0;
        unit.ColumnDir(direction.normalized);
    }

    protected virtual void FallIn()
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

    protected virtual void Aim()
    {
        var (success, position) = GetLookPosition();
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

    protected virtual bool Commandable()
    {
        if (unit != null & unit.InCommandRange(transform.position)) { return true; }
        Debug.Log("Not Commandable");
        return false;
    }

    virtual public void CommandForwardMarch()
    {
        if (Commandable()) {
            unit.ForwardMarch();
            cmdMenu.SetActive(false);
        }
    }

    virtual public void CommandHalt()
    {
        if (Commandable()) {
            unit.Halt();
            cmdMenu.SetActive(false);
        }
    }

    virtual public void CommandRight()
    {
        if (Commandable()) {
            unit.Face(UnitController.Direction.Right);
        }
    }

    virtual public void CommandLeft()
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