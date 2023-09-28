using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationSpawner : MonoBehaviour
{
    public float yellDist = 10;

    // Start is called before the first frame update
    void Start()
    {
        IEnumerator child = transform.GetEnumerator();
        child.MoveNext();

        Collider[] hits = Physics.OverlapSphere(transform.position, yellDist);    
        foreach (var hit in hits) 
        {
            if (hit.gameObject.CompareTag("Soldier")) {
                hit.gameObject.GetComponent<FindMarker>().marker = ((Transform)child.Current).gameObject;

                if (!child.MoveNext()) { return; }
            }
        }
    }

}
