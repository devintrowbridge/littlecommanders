using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public class FormationController : MonoBehaviour
{
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

        float formationWidth = positions * spacing - spacing;
        Vector3 currPos = transform.position - transform.right * formationWidth / 2;
        Vector3 incrPos = transform.right * spacing;

        for (int position = 0; position < positions; ++position) {
            if (soldier.Current == null) break;
            var marker = Instantiate(
                markerPrefab, 
                currPos, 
                transform.rotation, 
                transform);
            ((Soldier)soldier.Current).marker = marker;
            markers.Add(marker);
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
