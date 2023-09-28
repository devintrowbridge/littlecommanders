using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationController : MonoBehaviour 
{
    public AFormation state { get; private set; }

    // Note these can be destroyed by the Soldier that occupies the marker
    public List<Marker> markers = new List<Marker>();
    
    public UnitController _ctrl { get; private set; }

    public float spacing = 2f;
    public int ranks;
    public int files;


    public GameObject origin;

    private void Start()
    {
        if (Debug.isDebugBuild) {
            origin = new GameObject();
            origin.name = "Origin";
            origin.transform.parent = transform;
        }
    }

    void Update()
    {
        if (state != null) state.UpdateState();
        if (Debug.isDebugBuild) {
            origin.transform.position = transform.position;
            origin.transform.rotation = transform.rotation;
        }
    }

    public void ChangeState(AFormation newState)
    {
        if (state != null) state.OnExit();
        state = newState;
        state.OnEnter(this);
    }

    public void Initialize(UnitController ctrl)
    {
        _ctrl = ctrl;
    }

    // Creates a marker in the formation for each Soldier to stand on
    public void GenerateFormation(List<Soldier> soldiers)
    {
        int positions = soldiers.Count;
        IEnumerator soldier = soldiers.GetEnumerator();
        soldier.MoveNext();

        foreach (var pos in NextPos(positions)) {
            if (pos.rank > ranks) { ranks = pos.rank; }
            if (pos.file > files) { files = pos.file; }

            var marker = Instantiate(
                _ctrl.markerPrefab,
                Vector3.zero,
                transform.rotation,
                transform
            ).GetComponent<Marker>();
            marker.spacing = spacing;
            marker.SetPosition(pos);

            ((Soldier)soldier.Current).marker = marker.gameObject;

            markers.Add(marker);
            soldier.MoveNext();
        }

        ranks++;
        files++;

        // Now that the formation is made, we need to center it on the position it was instantiated
        // and back it up a little to the let the commander breath
        transform.position += transform.right * (files - 1) * spacing / 2;
        transform.position += -transform.forward * 2;

        ChangeState(new Line());
    }

    // Meat and potatoes of the formation generation function. This will walk through the formation
    // in position order defined in the table below.
    static IEnumerable<FormPos> NextPos(int totalPos)
    {
        /*  Basic idea is to fill out the first 3 ranks with 3 then go by file
         *  
         *  7 | 8 | 9 | 12 | 15  | ... | 90 |
         *  4 | 5 | 6 | 11 | 14  | ... | 89 |
         *  1 | 2 | 3 | 10 | 13  | ... | 88 |
         */
        int rank = 0;
        int file = 0;

        for (int i = 0; i < totalPos; ++i) {
            yield return new FormPos(rank, file);

            if (i < 8) {
                file++;
                if (file > 2) { rank++; file = 0; }
            }
            else {
                rank++;
                if (rank > 2) { file++; rank = 0; }
            }
        }
    }
}
