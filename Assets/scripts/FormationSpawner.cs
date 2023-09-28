using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationSpawner : MonoBehaviour
{
    public float yellDist = 10;
    public GameObject markerPrefab;
    public float spacing = 2f;

    private List<FindMarker> soldiers = new List<FindMarker>();

    // Start is called before the first frame update
    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, yellDist);
        foreach (var hit in hits) {
            if (hit.gameObject.CompareTag("Soldier")) {
                var findmark = hit.gameObject.GetComponent<FindMarker>();
                soldiers.Add(findmark);
            }
        }

        GenerateFormation();
    }

    void GenerateFormation() 
    { 
        int positions = soldiers.Count;
        IEnumerator soldier = soldiers.GetEnumerator();
        soldier.MoveNext();

        float formationWidth = positions * spacing - spacing;
        Vector3 currPos = transform.position - transform.right * formationWidth / 2;
        Vector3 incrPos = transform.right * spacing;

        for (int position = 0; position < positions; ++position) {
            if (soldier.Current == null) break;
            ((FindMarker)soldier.Current).marker = Instantiate(
                markerPrefab, 
                currPos, 
                transform.rotation, 
                transform);
            currPos += incrPos;
            soldier.MoveNext();
        }
    }
}
