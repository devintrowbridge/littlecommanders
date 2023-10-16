using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public GameObject soldierPrefab;
    public TeamType teamType;
    public int num;
    public float radius;

    public bool polling;

    private List<GameObject> soldiers = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < num; ++i) {
            var sp = Instantiate(soldierPrefab);
            sp.GetComponent<CTeam>().Team = teamType;
            SetSoldierPos(sp);
            soldiers.Add(sp);
        }
    }

    void SetSoldierPos(GameObject soldier)
    {
        var rot = Quaternion.AngleAxis(Random.Range(0, 359), Vector3.up);
        var pos = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
        pos += gameObject.transform.position;

        soldier.gameObject.transform.position = pos;
        soldier.gameObject.transform.rotation = rot;
    }

    IEnumerator Poll()
    {
        polling = true;

        foreach (var s in soldiers) {
            if (!s.activeSelf) {
                SetSoldierPos(s);
                s.SetActive(true);
            }
        }

        yield return new WaitForSeconds(10);
        polling = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!polling) StartCoroutine(Poll());
    }
}
