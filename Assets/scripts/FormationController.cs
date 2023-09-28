using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public class FormationController : MonoBehaviour
{
    public int maxPerRank = 30;

    public float yellDist = 10;
    public GameObject markerPrefab;
    public float spacing = 2f;

    private Material mat;
    private List<Soldier> soldiers = new List<Soldier>();
    private List<GameObject> markers = new List<GameObject>();

    public bool forwardMarch { private set; get;  }
    private float forwardSpeed = Constants.SOLDIER_BASE_MOVE_SPEED;

    // Start is called before the first frame update
    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, yellDist);
        foreach (var hit in hits) {
            if (hit.gameObject.CompareTag("Soldier")) {
                var soldier = hit.gameObject.GetComponent<Soldier>();
                soldiers.Add(soldier);
                forwardSpeed = soldier.speed;
            }
        }
        GenerateFormation();
    }

    private void Update()
    {
        if (forwardMarch) {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        }
    }

    public void GenerateFormation() 
    {
        int positions = soldiers.Count;
        IEnumerator soldier = soldiers.GetEnumerator();
        soldier.MoveNext();

        float formationWidth = FormationWidth(positions);

        foreach (var pos in NextPos(positions)) {
            var marker = Instantiate(
                markerPrefab,
                pos,
                transform.rotation,
                transform);
            ((Soldier)soldier.Current).marker = marker;
            markers.Add(marker);
            soldier.MoveNext();
        }
        transform.position = transform.position + transform.right * formationWidth / 2;

        if (mat != null) SetColor(mat);
    }

    float FormationWidth(int totalPos)
    {
        if (totalPos == 1) return 0;
        if (totalPos == 2) return 1 * spacing;
        if (totalPos <= 9) return 2 * spacing;
        return Mathf.Floor(totalPos / 3) * spacing;
    }

    IEnumerable<Vector3> NextPos(int totalPos)
    {
        /*  Basic idea is to fill out the first 3 ranks with 3 then go by file
         *  
         *  7 | 8 | 9 | 12 | 15  | ... | 90 |
         *  4 | 5 | 6 | 11 | 14  | ... | 89 |
         *  1 | 2 | 3 | 10 | 13  | ... | 88 |
         */
        var zero = transform.position;
        int rank = 0;
        int file = 0;

        for (int i = 0; i < totalPos; ++i) {
            Debug.Log("rank " + rank + " file " + file);
            yield return zero - rank * transform.forward * spacing - file * transform.right * spacing;

            if (i < 8) {
                file++;
                if (file > 2) { rank++; file = 0; }
            } else {
                rank++;
                if (rank > 2) { file++; rank = 0; }
            }
        }
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

    public void ForwardMarch()
    {
        forwardMarch = true;
        foreach (var marker in markers) {
            marker.transform.Translate(Vector3.forward);
        }

    }

    public void Halt()
    {
        forwardMarch = false;
    }

}
