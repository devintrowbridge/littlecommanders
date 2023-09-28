using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class FormationController : MonoBehaviour
{
    public enum Direction { Left, Right };

    public int maxPerRank = 30;

    public float yellDist = 10;
    public GameObject markerPrefab;
    public GameObject subdivisionPrefab;

    private Material mat;
    private List<Soldier> soldiers = new List<Soldier>();

    private List<Subdivision> sd = new List<Subdivision>();

    public bool forwardMarch { private set; get;  }
    public float forwardSpeed { private set; get; } = Constants.SOLDIER_BASE_MOVE_SPEED;
    public float spacing = 2f;

    public Vector3 forward {
        private set { }  
        get
        {
            return sd[0].transform.forward;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create a subdivision 
        var subd = Instantiate(
            subdivisionPrefab,
            transform.position,
            transform.rotation
        ).GetComponent<Subdivision>();
        subd.Initialize(this);
        transform.position = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
        sd.Add(subd);

        // See what soldiers are around
        Collider[] hits = Physics.OverlapSphere(sd[0].transform.position, yellDist);
        foreach (var hit in hits) {
            if (hit.gameObject.CompareTag("Soldier")) {
                var soldier = hit.gameObject.GetComponent<Soldier>();
                soldiers.Add(soldier);
                forwardSpeed = soldier.speed;
            }
        }

        // Put them in the formation
        subd.GenerateFormation(soldiers);
        if (mat != null) SetColor(mat);
    }

    public void ColumnDir(Vector3 newDir)
    {
        sd[0].ColumnDir(newDir);
    }

    public void Face(Direction dir)
    {
        foreach (var subd in sd) {
            subd.Face(dir);
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
            soldier.SetColor(mat);
        }
    }

    private void OnDestroy()
    {
        foreach (var soldier in soldiers) {
            if (soldier != null) soldier.ClearColor();
        }

        foreach(var subdivision in sd) {
            if (subdivision != null) {
                Destroy(subdivision.gameObject);
            }
        }
    }

    public void ForwardMarch()
    {
        forwardMarch = true;
    }

    public void Halt()
    {
        forwardMarch = false;
    }

    public IEnumerator Fire()
    {
        foreach (var subdivision in sd) {
            subdivision.MoveToFire();
        }

        yield return new WaitForSeconds(1);

        foreach (var s in soldiers) {
            s.Fire();
        }
    }
    public void Reload()
    {
        foreach (var subdivision in sd) {
            subdivision.Reload();
        }
    }
}
