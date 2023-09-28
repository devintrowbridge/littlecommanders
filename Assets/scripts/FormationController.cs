using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using static FormationController;
using static UnityEditor.PlayerSettings;

public abstract class AFormation 
{
    protected FormationController _fc;

    public virtual void OnEnter(FormationController fc) {
        _fc = fc;
    }

    public virtual void UpdateState() 
    {
        if (_fc._ctrl.forwardMarch) {
            _fc.transform.Translate(_fc._ctrl.forwardSpeed * Time.deltaTime * _fc.transform.forward, Space.World);
        }
    }

    public virtual void OnExit() { }


    public virtual void Face(UnitController.Direction dir)
    {
        // left/right face
        float ang = 90f;
        if (dir == UnitController.Direction.Left) { ang *= -1; }

        // Rotate all of the markers as well as the parent so forward is the same for everything
        foreach (var marker in _fc.markers) {
            marker.transform.parent = null;
        }
        _fc._center.transform.parent = null;

        _fc.transform.Rotate(Vector3.up, ang);

        foreach (var marker in _fc.markers) {
            marker.transform.parent = _fc.transform;
            marker.transform.rotation = _fc.transform.rotation;
        }
        _fc._center.transform.parent = _fc.transform;
        _fc._center.transform.rotation = _fc.transform.rotation;

        // swap the formation dims so they still make sense
        (_fc.ranks, _fc.files) = (_fc.files, _fc.ranks);
    }

    public virtual void ResetFormation() { }

    public virtual void ColumnDir(Vector3 newDir)
    {
        float angleDir = Vector3.SignedAngle(_fc.transform.forward, newDir, Vector3.up);
        if (Mathf.Abs(angleDir) > 90f) return;

        var formationDim = new Vector3(_fc.files, 0, _fc.ranks);
        Vector3 formationTrueDim = _fc._ctrl.spacing * (formationDim - new Vector3(1, 0, 1));

        // Find the center of the formation and apply an offset to get the front of the formation
        var center = _fc.transform.InverseTransformPoint(_fc._center.transform.position);
        var pivotDir = angleDir > 0 ? Vector3.right : -Vector3.right;

        // Push offset out to the edge of the formation in the forward direction
        var offset = Vector3.Scale(Vector3.forward, formationTrueDim / 2);

        // Push offset to the inside edge of the turn 
        offset += Vector3.Scale(pivotDir, formationTrueDim / 2);
        var pivotPoint = center + offset;

        // rotate formation around pivot
        _fc.transform.RotateAround(_fc.transform.TransformPoint(pivotPoint), Vector3.up, angleDir);
    }

    public virtual List<List<Marker>> OrganizeByRank()
    {
        List<List<Marker>> col = new List<List<Marker>>();

        for (var rank = 0; rank < _fc.ranks; ++rank) {
            col.Add(new List<Marker>());

            var width = new Vector3((_fc.files - 1) * _fc._ctrl.spacing, 0, 0);
            var centerFront = -width / 2;
            var centerRank = centerFront + new Vector3(0, 0, -rank * _fc._ctrl.spacing);

            Collider[] hitColliders = Physics.OverlapBox(
                _fc.transform.TransformPoint(centerRank),
                new Vector3(width.x / 2, .5f, .5f),
                _fc.transform.rotation,
                Constants.LAYER_MARKER
            );

            foreach (var hit in hitColliders) {
                var s = hit.gameObject.GetComponent<Marker>();
                if (s != null) {
                    col[rank].Add(s);
                }
            }
        }

        return col;
    }

    public virtual void MoveToFire() { }
}

public class Line : AFormation 
{
    public override void Face(UnitController.Direction dir)
    {
        base.Face(dir);
        _fc.ChangeState(new Column());
    }

    public override void MoveToFire() 
    {
        var ranksCol = OrganizeByRank();

        // first ranks kneels
        if (ranksCol.Count > 0) {
            foreach (var marker in ranksCol[0]) {
                marker.lastPos = marker.transform.position;
                marker.transform.position += Vector3.down * 1.2f;
            }
        }

        // second rank move forward half step, and just a smidge to the right
        if (ranksCol.Count > 1) {
            foreach (var marker in ranksCol[1]) {
                marker.lastPos = marker.transform.position;

                var move = _fc.transform.forward * _fc._ctrl.spacing * .5f;
                move += _fc.transform.right * _fc._ctrl.spacing * .1f;
                marker.transform.position += move;
            }
        }

        // third rank takes half step right and full step forward
        if (ranksCol.Count > 2) {
            foreach (var marker in ranksCol[2]) {
                marker.lastPos = marker.transform.position;

                var move = _fc.transform.forward * _fc._ctrl.spacing;
                move += _fc.transform.right * _fc._ctrl.spacing * .5f;
                marker.transform.position += move;
            }
        }

        _fc.ChangeState(new Volley());
    }
}

public class Column : AFormation 
{
    public override void Face(UnitController.Direction dir)
    {
        base.Face(dir);
        _fc.ChangeState(new Line());
    }
}

public class Volley : AFormation 
{
    public override void UpdateState() { }
    public override void Face(UnitController.Direction dir) { }

    public override void ResetFormation()
    {
        foreach (var marker in _fc.markers) {
            marker.transform.position = marker.lastPos;
        }
        _fc.ChangeState(new Line());
    }
}

public class Square : AFormation { /* todo */ }

public class FormationController : MonoBehaviour 
{
    public AFormation state { get; private set; }
    public List<Marker> markers = new List<Marker>();
    public UnitController _ctrl { get; private set; }
    public GameObject _center;

    public int ranks;
    public int files;

    void Update()
    {
        if (state != null) state.UpdateState();
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

        _center = new GameObject();
        _center.transform.parent = transform;
        _center.name = "center";
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

            ((Soldier)soldier.Current).marker = marker.gameObject;

            markers.Add(marker);
            soldier.MoveNext();
        }

        ranks++;
        files++;

        transform.position = transform.position + transform.right * (files - 1) * _ctrl.spacing / 2;

        // get initial position of center
        var formationDim = new Vector3(files, 0, ranks);
        var localCenter = -(formationDim - new Vector3(1, 0, 1)) * _ctrl.spacing / 2;
        _center.transform.position = transform.TransformPoint(localCenter);
        _center.transform.rotation = transform.rotation;

        ChangeState(new Line());
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
}
