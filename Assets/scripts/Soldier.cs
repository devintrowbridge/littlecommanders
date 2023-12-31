using System.Collections;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Soldier : MonoBehaviour
{
    public Vector3 start;
    private Vector3 end;
    public GameObject marker;

    public bool moving = false;
    Vector3 velocity = Vector3.zero;

    public float maxDistanceFromMark = .5f;
    private float tolerableDistFromMark = 0;

    private Gun gun;
    private bool isDead = false;

    public delegate void Callback(Soldier s);

    public static void MoveTo(Transform me, Vector3 pos, ref Vector3 velocity, float maxSpeed = Constants.SOLDIER_MAX_MOVE_SPEED)
    {
        me.LookAt(pos);
        me.position = Vector3.SmoothDamp(me.position, pos, ref velocity, 0.1f, maxSpeed);
    }

    public static void MatchRot(Transform target, Transform transform, float rotSpeed = Constants.SOLDIER_BASE_ROT_SPEED)
    {
        var lookDir = new Vector3(0, target.eulerAngles.y, 0);
        var lookRot = Quaternion.Euler(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotSpeed);
    }

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
        if (moving) { return; }
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
        if (!isDead && marker != null) GetOnMark();
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
            moving = true;
        }

        // If we're at the marker then stop moving and face forward
        if (!TooFarFromMarker()) {
            moving = false;
            MatchRot(marker.transform, transform);
        }

        // If we are moving, then translate our way over to the marker
        if (moving) {
            MoveTo(transform, end, ref velocity);
        }
    }

    bool TooFarFromMarker()
    {
        return (transform.position - end).magnitude > tolerableDistFromMark;
    }

    IEnumerator Decompose()
    {
        yield return new WaitForSeconds(10);
        gameObject.SetActive(false);
    }

    private void Die()
    {
        transform.eulerAngles = new Vector3(90f,0,0);
        SetColor(Color.red);
        isDead = true;
        Destroy(marker);
        StartCoroutine(Decompose());
    }

    public void Hit()
    {
        Die();
    }
}
