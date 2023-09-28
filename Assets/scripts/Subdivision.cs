using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subdivision : MonoBehaviour {
    public List<Marker> markers = new List<Marker>();
    private FormationController _ctrl;
         
    int ranks;
    int files;
    Vector3 formationDim;

    Vector3 pivotPoint;

    List<List<GameObject>> ranksColl = new List<List<GameObject>>();

    // Update is called once per frame
    void Update()
    {
        if (_ctrl.forwardMarch) {
            transform.Translate(_ctrl.forwardSpeed * Time.deltaTime * transform.forward, Space.World);
        }
    }

    public void Initialize(FormationController ctrl)
    {
        _ctrl = ctrl;

        // Add 3 ranks, may or may not use them all
        ranksColl.Add(new List<GameObject>());
        ranksColl.Add(new List<GameObject>());
        ranksColl.Add(new List<GameObject>());
    }

    public void ColumnDir(Vector3 newDir)
    {
        Vector3 formationTrueDim = _ctrl.spacing * (formationDim - new Vector3(1, 0, 1));
        float angleDir = Vector3.SignedAngle(transform.forward, newDir, Vector3.up);

        // Find the center of the formation and apply an offset to get the front of the formation
        var center = -new Vector3((files - 1) * _ctrl.spacing / 2, 0, (ranks - 1) * _ctrl.spacing / 2);
        var pivotDir = angleDir > 0 ? Vector3.right : -Vector3.right;
        
        // Push offset out to the edge of the formation in the forward direction
        var offset = Vector3.Scale(Vector3.forward, formationTrueDim / 2);

        // Push offset to the inside edge of the turn 
        offset += Vector3.Scale(pivotDir, formationTrueDim / 2);

        // Combine offset with the center to get the pivot point
        var pivotPoint = center + offset;

        // rotate formation around pivot
        transform.RotateAround(transform.TransformPoint(pivotPoint), Vector3.up, angleDir);
    }

    void ResetFormation()
    {
        OrganizeByRank();

    }

    // Creates a marker in the formation for each Soldier to stand on
    public void GenerateFormation(List<Soldier> soldiers)
    {
        int positions = soldiers.Count;
        IEnumerator soldier = soldiers.GetEnumerator();
        soldier.MoveNext();


        foreach (var pos in NextPos(positions)) {
            if (pos.Item1 > ranks) { ranks = pos.Item1; }
            if (pos.Item2 > files) { files = pos.Item2; }

            var marker = Instantiate(
                _ctrl.markerPrefab,
                RankFileToPos(pos.Item1, pos.Item2),
                transform.rotation,
                transform
            ).GetComponent<Marker>();
            marker.rank = pos.Item1;
            marker.file = pos.Item2;

            ((Soldier)soldier.Current).marker = marker.gameObject;

            markers.Add(marker);
            soldier.MoveNext();
        }

        ranks++;
        files++;
        formationDim = new Vector3(files, 0, ranks);

        transform.position = transform.position + transform.right * (files - 1) * _ctrl.spacing / 2;
    }

    private Vector3 RankFileToPos(int rank, int file)
    {
        var zero = transform.position;
        return zero - rank * transform.forward * _ctrl.spacing - file * transform.right * _ctrl.spacing;
    }

    // Meat and potatoes of the formation generation function. This will walk through the formation
    // in position order defined in the table below.
    IEnumerable<(int, int)> NextPos(int totalPos)
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
            yield return (rank, file);

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

    public void Face(FormationController.Direction dir)
    {
        // left/right face
        float ang = 90f;
        if (dir == FormationController.Direction.Left) { ang *= -1; }

        // Rotate all of the markers as well as the parent so forward is the same for everything
        foreach (var marker in markers) {
            marker.transform.parent = null;
        }
        transform.Rotate(Vector3.up, ang);
        foreach (var marker in markers) {
            marker.transform.parent = transform;
            marker.transform.rotation = transform.rotation;
        }

        // swap the formation dims so they still make sense
        (ranks, files) = (files, ranks);
    }


    public void OrganizeByRank()
    {
        for (var rank = 0; rank < ranks; ++rank) {
            ranksColl[rank].Clear();

            var width = new Vector3((files - 1) * _ctrl.spacing, 0, 0);
            var centerFront = - width / 2;
            var centerRank = centerFront + new Vector3(0, 0, -rank * _ctrl.spacing);

            Debug.Log("width " + width + " center " + centerRank);

            Debug.Log("center " + transform.TransformPoint(centerRank));

            Collider[] hitColliders = Physics.OverlapBox(
                transform.TransformPoint(centerRank),
                new Vector3(width.x / 2, .5f, .5f),
                transform.rotation,
                Constants.LAYER_MARKER
            );

            foreach (var hit in hitColliders) {
                var s = hit.gameObject;
                if (s != null) {
                    ranksColl[rank].Add(s);
                }
            }
        }

    }

    // https://www.youtube.com/watch?v=EURWwDbKvWY
    public void MoveToFire()
    {
        // Three steps to volley fire
        // 1. Move into position
        // 2. Fire
        // 3. move back to ranks
        
        OrganizeByRank();

        // 1. Move into position
        // first ranks kneels
        if (ranksColl.Count > 0) {
            foreach (var marker in ranksColl[0]) {
                marker.transform.position += Vector3.down;
            }
        }

        // second rank move forward half step still
        if (ranksColl.Count > 1) {
            foreach (var marker in ranksColl[1]) {
                var move = transform.forward * _ctrl.spacing * .5f;
                marker.transform.position += move;
            }
        }

        // third rank takes half step right and full step forward
        if (ranksColl.Count > 2) {
            foreach (var marker in ranksColl[2]) {
                var move = transform.forward * _ctrl.spacing;
                move += transform.right * _ctrl.spacing * .5f;
                marker.transform.position += move;
            }
        }
    }

    public void Reload()
    {
        //DoRank<Soldier>(Soldier.Reload);

        // Move front row to the back
    }
}
