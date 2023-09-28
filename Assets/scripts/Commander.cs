using UnityEngine;

public class Commander : MonoBehaviour {
    public float moveSpeed = 10f;
    public float accSpeed = 10f;
    public float rotSpeed = Constants.SOLDIER_BASE_ROT_SPEED;

    public GameObject avatar;
    public GameObject unitPrefab;

    protected UnitController unit;
    public float unitOffset = 6f;

    public Material mat;

    public float maxCommandDist = 10f;


    protected virtual void LateUpdate()
    {
        // Move with unit
        if (unit && unit.forwardMarch) {
            transform.Translate(unit.travelVec);
        }
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

    protected virtual void ColumnMarch(Vector3 direction)
    {
        if (!Commandable()) return;
        direction.y = 0;
        unit.ColumnDir(direction.normalized);
    }

    public virtual void FallIn()
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

    public virtual void Aim(Vector3 lookPosition)
    {
        if (lookPosition == null) return;

        // Calculate the direction
        var direction = lookPosition - avatar.transform.position;

        // You might want to delete this line.
        // Ignore the height difference.
        direction.y = 0;

        var lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        avatar.transform.rotation = Quaternion.Slerp(avatar.transform.rotation, lookRot, Time.deltaTime * rotSpeed);
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
        }
    }

    virtual public void CommandHalt()
    {
        if (Commandable()) {
            unit.Halt();
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