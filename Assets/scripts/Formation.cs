using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Formation : MonoBehaviour {
    public enum FormationType {
        Column,
        Line, 
        Volley,
        Square
    };

    public FormationType formationType { get; private set; }

    List<List<Marker>> ranksColl = new List<List<Marker>>();
    public List<Marker> markers = new List<Marker>();
    private UnitController _ctrl;

    int ranks;
    int files;
    GameObject _center;


    // Update is called once per frame
    void Update()
    {
        if (_ctrl.forwardMarch) {
            transform.Translate(_ctrl.forwardSpeed * Time.deltaTime * transform.forward, Space.World);
        }
    }

    public void Initialize(UnitController ctrl)
    {
        _ctrl = ctrl;

        _center = new GameObject();
        _center.transform.parent = transform;
        _center.transform.position = transform.position;
        _center.transform.rotation = transform.rotation;
        _center.name = "center";

        // Add 3 ranks, may or may not use them all
        ranksColl.Add(new List<Marker>());
        ranksColl.Add(new List<Marker>());
        ranksColl.Add(new List<Marker>());
    }

    public void ColumnDir(Vector3 newDir)
    {
        float angleDir = Vector3.SignedAngle(transform.forward, newDir, Vector3.up);
        if (Mathf.Abs(angleDir) > 90f) return;

        var formationDim = new Vector3(files, 0, ranks);
        Vector3 formationTrueDim = _ctrl.spacing * (formationDim - new Vector3(1, 0, 1));

        // Find the center of the formation and apply an offset to get the front of the formation
        var center = transform.InverseTransformPoint(_center.transform.position);
        var pivotDir = angleDir > 0 ? Vector3.right : -Vector3.right;
        
        // Push offset out to the edge of the formation in the forward direction
        var offset = Vector3.Scale(Vector3.forward, formationTrueDim / 2);

        // Push offset to the inside edge of the turn 
        offset += Vector3.Scale(pivotDir, formationTrueDim / 2);
        var pivotPoint = center + offset;

        // rotate formation around pivot
        transform.RotateAround(transform.TransformPoint(pivotPoint), Vector3.up, angleDir);
    }

    public void ResetFormation()
    {
        foreach(var marker in markers) {
            marker.transform.position = marker.lastPos;
        }
    }

    // Creates a marker in the formation for each Soldier to stand on
    public void GenerateFormation(List<Soldier> soldiers)
    {
        formationType = FormationType.Line;

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

            ((Soldier)soldier.Current).marker = marker.gameObject;

            markers.Add(marker);
            soldier.MoveNext();
        }

        ranks++;
        files++;

        transform.position = transform.position + transform.right * (files - 1) * _ctrl.spacing / 2;

        // set the center
        var formationDim = new Vector3(files, 0, ranks);
        var localCenter = (formationDim - new Vector3(1, 0, 1)) * _ctrl.spacing / 2;
        _center.transform.position = transform.TransformPoint(-localCenter);
        _center.transform.rotation = transform.rotation;
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

    public void Face(UnitController.Direction dir)
    {
        // left/right face
        float ang = 90f;
        if (dir == UnitController.Direction.Left) { ang *= -1; }

        // Rotate all of the markers as well as the parent so forward is the same for everything
        foreach (var marker in markers) {
            marker.transform.parent = null;
        }
        _center.transform.parent = null;

        transform.Rotate(Vector3.up, ang);

        foreach (var marker in markers) {
            marker.transform.parent = transform;
            marker.transform.rotation = transform.rotation;
        }
        _center.transform.parent = transform;
        _center.transform.rotation = transform.rotation;

        // swap the formation dims so they still make sense
        (ranks, files) = (files, ranks);
    }


    public void OrganizeByRank()
    {
        ranksColl.Clear();

        for (var rank = 0; rank < ranks; ++rank) {
            ranksColl.Add(new List<Marker>());

            var width = new Vector3((files - 1) * _ctrl.spacing, 0, 0);
            var centerFront = - width / 2;
            var centerRank = centerFront + new Vector3(0, 0, -rank * _ctrl.spacing);

            Collider[] hitColliders = Physics.OverlapBox(
                transform.TransformPoint(centerRank),
                new Vector3(width.x / 2, .5f, .5f),
                transform.rotation,
                Constants.LAYER_MARKER
            );

            foreach (var hit in hitColliders) {
                var s = hit.gameObject.GetComponent<Marker>();
                if (s != null) {
                    ranksColl[rank].Add(s);
                }
            }
        }

    }

    // https://www.youtube.com/watch?v=EURWwDbKvWY
    public void MoveToFire()
    {
        if (formationType == FormationType.Volley) return;
        formationType = FormationType.Volley;
        OrganizeByRank();

        // first ranks kneels
        if (ranksColl.Count > 0) {
            foreach (var marker in ranksColl[0]) {
                marker.lastPos = marker.transform.position;

                marker.transform.position += Vector3.down * 1.2f;
            }
        }

        // second rank move forward half step, and just a smidge to the right
        if (ranksColl.Count > 1) {
            foreach (var marker in ranksColl[1]) {
                marker.lastPos = marker.transform.position;

                var move = transform.forward * _ctrl.spacing * .5f;
                move += transform.right * _ctrl.spacing * .1f;
                marker.transform.position += move;
            }
        }

        // third rank takes half step right and full step forward
        if (ranksColl.Count > 2) {
            foreach (var marker in ranksColl[2]) {
                marker.lastPos = marker.transform.position;

                var move = transform.forward * _ctrl.spacing;
                move += transform.right * _ctrl.spacing * .5f;
                marker.transform.position += move;
            }
        }


    }
}
