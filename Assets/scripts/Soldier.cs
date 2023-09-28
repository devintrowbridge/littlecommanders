using System.Collections;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    public Vector3 start;
    private Vector3 end;
    public GameObject marker;

    public float speed = Constants.SOLDIER_BASE_MOVE_SPEED;
    public bool moving = false;

    public float maxDistanceFromMark = .5f;
    private float tolerableDistFromMark = 0;

    private Gun gun;

    public delegate void Callback(Soldier s);

    // Start is called before the first frame update
    void Start()
    {
        tolerableDistFromMark = Random.Range(.1f, maxDistanceFromMark);
        gun = transform.Find("Gun").GetComponent<Gun>();
    }

    private IEnumerator Fire(float delay)
    {
        yield return new WaitForSeconds(Random.Range(0f, delay));
        gun.Fire();
    }

    public void Fire()
    {
        var delay = Random.Range(0f, 1f);
        StartCoroutine(Fire(delay));
    }

    public void Reload()
    {
        Debug.Log("Reloading");
        float reloadMultiplier = Random.Range(.5f, 2f);
        StartCoroutine(gun.Reload(reloadMultiplier));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (marker != null) GetOnMark();
        else {
            moving = false;
        }
    }

    public void ClearColor()
    {
        transform.Find("Body").GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
    }

    public void SetColor(Color color)
    {
        transform.Find("Body").GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }

    void GetOnMark()
    {
        end = marker.transform.position;

        // If we're too far away and haven't start moving, then start moving
        if (TooFarFromMarker() && !moving) {
            speed *= 1.5f; // catchup speed
            moving = true;
        }

        // If we're at the marker then stop moving and face forward
        if (!TooFarFromMarker()) {
            moving = false;
            speed = Constants.SOLDIER_BASE_MOVE_SPEED;
            Facing();
        }

        // If we are moving, then translate our way over to the marker
        if (moving) {
            transform.LookAt(end);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    bool TooFarFromMarker()
    {
        return (transform.position - end).magnitude > tolerableDistFromMark;
    }

    void Facing()
    {
        transform.rotation = marker.transform.rotation;
    }
}
