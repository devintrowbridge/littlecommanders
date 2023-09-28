using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public enum Direction { Left, Right };

    public int maxPerRank = 30;

    public float yellDist = 10;
    public GameObject markerPrefab;
    public GameObject subdivisionPrefab;

    private Material mat;
    private List<Soldier> soldiers = new List<Soldier>();

    private List<FormationController> formations = new List<FormationController>();

    public bool forwardMarch { private set; get;  }
    public float forwardSpeed { private set; get; } = Constants.SOLDIER_BASE_MOVE_SPEED;
    public float spacing = 2f;

    public Vector3 forward {
        private set { }  
        get
        {
            return formations[0].transform.forward;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create a subdivision 
        var form = Instantiate(
            subdivisionPrefab,
            transform.position,
            transform.rotation
        ).GetComponent<FormationController>();
        form.Initialize(this);
        transform.position = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
        formations.Add(form);

        // See what soldiers are around
        Collider[] hits = Physics.OverlapSphere(formations[0].transform.position, yellDist);
        foreach (var hit in hits) {
            if (hit.gameObject.CompareTag("Soldier")) {
                var soldier = hit.gameObject.GetComponent<Soldier>();
                soldiers.Add(soldier);
                forwardSpeed = soldier.speed;
            }
        }

        // Put them in the formation
        form.GenerateFormation(soldiers);
        if (mat != null) SetColor(mat);
    }

    public void ColumnDir(Vector3 newDir)
    {
        formations[0].state.ColumnDir(newDir);
    }

    public void Face(Direction dir)
    {
        foreach (var f in formations) {
            f.state.Face(dir);
        }
    }

    // Checks to see if any soldier in the formation is within yelling distance,
    // If they are then the formation is commandable
    public bool InCommandRange(Vector3 position)
    {
        foreach (var sol in soldiers) {
            if (Vector3.Distance(sol.gameObject.transform.position, position) < yellDist) {
                return true;
            }
        }

        return false;
    }

    // We want the color of the soldiers in the formation to match the commander's colors
    public void SetColor(Material mat_)
    {
        mat = mat_;
        foreach (var soldier in soldiers) {
            soldier.SetColor(mat.color);
        }
    }

    private void OnDestroy()
    {
        foreach (var soldier in soldiers) {
            if (soldier != null) soldier.ClearColor();
        }

        foreach(var f in formations) {
            if (f != null) {
                Destroy(f.gameObject);
            }
        }
    }

    public void ForwardMarch()
    {
        foreach (var f in formations) {
            f.state.ResetFormation();
        }
        forwardMarch = true;
    }

    public void Halt()
    {
        forwardMarch = false;
    }

    public IEnumerator Fire()
    {
        // Can't fire if we're moving
        if (forwardMarch) yield break;

        // Move into volley formation
        foreach (var f in formations) {
            f.state.MoveToFire();
        }

        yield return new WaitForSeconds(1);

        // Give command to fire
        foreach (var s in soldiers) {
            s.Fire();
        }
    }
    public void Reload()
    {
        // Can't reload if we're moving
        if (forwardMarch) return;

        foreach (var s in soldiers) {
            s.Reload();
        }
    }
}
