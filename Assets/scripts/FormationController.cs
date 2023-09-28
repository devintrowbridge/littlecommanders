using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class FormPos {
    public int rank;
    public int file;

    public FormPos() { }
    public FormPos(int rank, int file)
    {
        this.rank = rank;
        this.file = file;
    }

    public override string ToString()
    {
        return "FormPos: rank " + rank + ", file " + file;
    }
}

public abstract class AFormation 
{
    protected FormationController _fc;
    protected float speedMultiplier;
    public Vector3 travelVec { get; private set; }

    public virtual void OnEnter(FormationController fc) {
        speedMultiplier = 1.0f;
        _fc = fc;
    }

    public virtual void UpdateState() 
    {
        if (_fc._ctrl.forwardMarch) {
            travelVec = Constants.SOLDIER_BASE_MOVE_SPEED * Time.deltaTime * speedMultiplier * _fc.transform.forward;
            _fc.transform.Translate(travelVec, Space.World);
        }
    }

    public virtual void OnExit() { }

    private Vector3 getNewOrigin(UnitController.Direction dir)
    {
        // The origin of the formation is rank 1, file 1.
        // Positive z/forward is the direction the formation is facing

        var newOrigin = Vector3.zero;

        switch (dir) {
            case UnitController.Direction.Left:
                // Facing left, the new origin will the the front & left-most corner
                newOrigin.x = -_fc.spacing * (_fc.files - 1);
                break;
            case UnitController.Direction.Right:
                // Facing right, the new origin will be the current back & right-most corner 
                newOrigin.z = -_fc.spacing * (_fc.ranks - 1);
                break;
        }

        return _fc.transform.TransformPoint(newOrigin);
    }

    public virtual void Face(UnitController.Direction dir)
    {
        // left/right face
        float ang = 0f;
        switch (dir) {
            case UnitController.Direction.Left:  ang = -90f; break;
            case UnitController.Direction.Right: ang =  90f; break;
        }

        // Rotate all of the markers as well as the parent so forward is the same for everything
        foreach (var marker in _fc.markers) {
            marker.transform.parent = null;
        }

        var newOrigin = getNewOrigin(dir);
        _fc.transform.Rotate(Vector3.up, ang);
        _fc.transform.position = newOrigin;

        foreach (var marker in _fc.markers) {
            marker.transform.parent   = _fc.transform;
            marker.transform.rotation = _fc.transform.rotation;
            marker.RankFileFromPos();
        }

        // swap the formation dims so they still make sense
        (_fc.ranks, _fc.files) = (_fc.files, _fc.ranks);
    }

    public virtual void ResetFormation() 
    {
        foreach (var marker in _fc.markers) {
            marker.transform.localPosition = -new Vector3(marker.file, 0, marker.rank) * _fc.spacing;
        }
    }

    public virtual void ColumnDir(Vector3 newDir)
    {
        float angleDir = Vector3.SignedAngle(_fc.transform.forward, newDir, Vector3.up);
        if (Mathf.Abs(angleDir) > 90f) return;

        var formationDim = new Vector3(_fc.files, 0, _fc.ranks);
        Vector3 formationTrueDim = _fc.spacing * (formationDim - new Vector3(1, 0, 1));

        // Find the center of the formation and apply an offset to get the front of the formation
        var center = -formationTrueDim / 2;
        var pivotDir = angleDir > 0 ? Vector3.right : -Vector3.right;

        // Push offset out to the edge of the formation in the forward direction
        var offset = Vector3.Scale(Vector3.forward, formationTrueDim / 2);

        // Push offset to the inside edge of the turn 
        offset += Vector3.Scale(pivotDir, formationTrueDim / 2);
        var pivotPoint = center + offset;

        // rotate formation around pivot
        _fc.transform.RotateAround(_fc.transform.TransformPoint(pivotPoint), Vector3.up, angleDir);
    }

    public virtual void MoveToFire() { }
}

public class Line : AFormation 
{
    public override void OnEnter(FormationController fc)
    {
        base.OnEnter(fc);
        speedMultiplier = .6f;
    }

    public override void Face(UnitController.Direction dir)
    {
        base.Face(dir);
        _fc.ChangeState(new Column());
    }

    public override void MoveToFire() 
    {
        foreach(var marker in _fc.markers) {
            switch (marker.rank) {
                case 0: // first ranks kneels
                    marker.transform.position += Vector3.down * 1.2f;
                    break;
                case 1: // second rank move forward half step, and just a smidge to the right
                    marker.transform.position += _fc.transform.forward * _fc.spacing * .5f;
                    marker.transform.position += _fc.transform.right * _fc.spacing * .1f;
                    break;
                case 2: // third rank takes half step right and full step forward
                    marker.transform.position += _fc.transform.forward * _fc.spacing;
                    marker.transform.position += _fc.transform.right * _fc.spacing * .5f;
                    break;

                default:
                    break;
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
    public override void Face(UnitController.Direction dir) {
        ResetFormation();
    }

    public override void ResetFormation()
    {
        base.ResetFormation();
        _fc.ChangeState(new Line());
    }
}

public class Square : AFormation { /* todo */ }

public class FormationController : MonoBehaviour 
{
    public AFormation state { get; private set; }
    public List<Marker> markers = new List<Marker>();
    public UnitController _ctrl { get; private set; }

    public float spacing = 2f;
    public int ranks;
    public int files;

    public float width
    {
        get => (files - 1) * spacing;
        private set => width = 0;
    }

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
        transform.position += transform.right * width / 2;
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
