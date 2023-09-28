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

    private FormationController formation;

    public bool forwardMarch { private set; get;  }
    public Vector3 travelVec { 
        get {
            return formation.state.travelVec;
        }

        private set {
            travelVec = value;
        }
    }

    public Vector3 forward {
        private set { }  
        get
        {
            return formation.transform.forward;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create a subdivision 
        formation = Instantiate(
            subdivisionPrefab,
            transform.position,
            transform.rotation
        ).GetComponent<FormationController>();
        formation.Initialize(this);
        transform.position = Vector3.zero;
        transform.eulerAngles = Vector3.zero;

        // See what soldiers are around
        Collider[] hits = Physics.OverlapSphere(formation.transform.position, yellDist);
        foreach (var hit in hits) {
            if (hit.gameObject.CompareTag("Soldier")) {
                var soldier = hit.gameObject.GetComponent<Soldier>();
                soldiers.Add(soldier);
            }
        }

        // Put them in the formation
        formation.GenerateFormation(soldiers);
        if (mat != null) SetColor(mat);
    }

    public void ColumnDir(Vector3 newDir)
    {
        formation.state.ColumnDir(newDir);
    }

    public void Face(Direction dir)
    {
        formation.state.Face(dir);
    }

    // Checks to see if any soldier in the formation is within yelling distance,
    // If they are then the formation is commandable
    public bool InCommandRange(Vector3 position)
    {
        foreach (var s in soldiers) {
            if (Vector3.Distance(s.gameObject.transform.position, position) < yellDist) {
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

        if (formation != null) {
            Destroy(formation.gameObject);
        }
    }

    public void ForwardMarch()
    {
        formation.state.ResetFormation();
        forwardMarch = true;
    }

    public void Halt()
    {
        forwardMarch = false;
    }

    public IEnumerator Fire()
    {
        Halt();
        formation.state.MoveToFire();
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
