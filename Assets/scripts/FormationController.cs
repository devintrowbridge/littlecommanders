using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationController : MonoBehaviour
{
    public float yellDist = 10;
    public GameObject markerPrefab;
    public float spacing = 2f;

    private Material mat;
    private List<Soldier> soldiers = new List<Soldier>();

    // Start is called before the first frame update
    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, yellDist);
        foreach (var hit in hits) {
            if (hit.gameObject.CompareTag("Soldier")) {
                var soldier = hit.gameObject.GetComponent<Soldier>();
                soldiers.Add(soldier);
            }
        }
        GenerateFormation();
    }

    public void GenerateFormation() 
    {
        int positions = soldiers.Count;
        IEnumerator soldier = soldiers.GetEnumerator();
        soldier.MoveNext();

        float formationWidth = positions * spacing - spacing;
        Vector3 currPos = transform.position - transform.right * formationWidth / 2;
        Vector3 incrPos = transform.right * spacing;

        for (int position = 0; position < positions; ++position) {
            if (soldier.Current == null) break;
            ((Soldier)soldier.Current).marker = Instantiate(
                markerPrefab, 
                currPos, 
                transform.rotation, 
                transform);
            currPos += incrPos;
            soldier.MoveNext();
        }

        if (mat != null) SetColor(mat);
    }

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
    }
}
