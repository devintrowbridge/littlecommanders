using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using UnityEngine;
using static UnityEngine.UI.Image;


public abstract class AAiCommander 
{
    protected AiCommanderStateController _sc;

    public virtual void OnEnter(AiCommanderStateController sc)
    {
        _sc = sc;
    }

    public virtual void UpdateState() { }
    public virtual void OnExit() { }
}

internal class Searching : AAiCommander 
{
    public float maxSearchDist = 500;
    private GameObject closestSoldier;
    private Vector3 velocity = Vector3.zero;

    private GameObject FindClosestSoldier()
    {
        var hitRad = 1;
        Collider[] hits = { };

        // while there are no hits and we're under the max search distance
        while (hits.Length < 1 || hitRad < maxSearchDist) {
            hits = Physics.OverlapSphere(_sc.transform.position, hitRad, Constants.LAYER_SOLDIER);
            hitRad *= 2;
        }

        if (hits.Length < 1) return null;
        return hits[0].gameObject;
    }

    public override void UpdateState()
    {
        if (closestSoldier != null) {
            Soldier.MoveTo(_sc.transform, closestSoldier.transform.position, ref velocity);

            var dist = (_sc.transform.position - closestSoldier.transform.position).magnitude;
            if (dist < 5) {
                _sc.FallIn();
                _sc.ChangeState(new Idle());
            }
        }
        else closestSoldier = FindClosestSoldier();
    }
}

internal class Idle : AAiCommander 
{

}

internal class Patrolling : AAiCommander 
{

}

public class AiCommanderStateController : Commander {

    public AAiCommander state { get; private set; }

    private void Start()
    {
        ChangeState(new Searching());
    }

    void Update()
    {
        if (state != null) state.UpdateState();
    }

    public void ChangeState(AAiCommander newState)
    {
        if (state != null) state.OnExit();
        state = newState;
        state.OnEnter(this);
    }
}
