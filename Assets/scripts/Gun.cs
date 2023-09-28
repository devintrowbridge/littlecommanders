using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour 
{
    private AudioSource gunshot;
    private ParticleSystem gunsmoke;
    private ParticleSystem gunspark;

    bool readyToFire;
    float reloadTime = 5f;
    float range = 200;

    // Start is called before the first frame update
    void Start()
    {
        gunshot = transform.Find("Gunshot").GetComponent<AudioSource>();
        gunsmoke = transform.Find("Gunsmoke").GetComponent<ParticleSystem>();
        gunspark = transform.Find("Gunspark").GetComponent<ParticleSystem>();

        readyToFire = false;

        if (Debug.isDebugBuild) {
            reloadTime = 0.1f;
        }
    }

    public IEnumerator Reload(float multiplier)
    {
        if (readyToFire) yield break;
        yield return new WaitForSeconds(multiplier * reloadTime);
        readyToFire = true;
        Debug.Log("Reloaded");
    }

    public void Fire()
    {
        if (!readyToFire) return;

        gunshot.Play();
        gunsmoke.Play();
        gunspark.Play();

        var dir = transform.forward;
        dir.x *= Random.Range(0, 10);
        dir.z *= Random.Range(0, 10);

        var raycast = Physics.Raycast(
            transform.position,
            dir,
            out var hit,
            range,
            Constants.LAYER_SOLDIER
        );

        var dist = range;
        if (raycast) {
            dist = hit.distance;
            hit.transform.gameObject.GetComponent<Soldier>().Hit();
        } 
        Debug.DrawRay(transform.position, dir * dist, Color.yellow, 30, false);

        readyToFire = false;
    }
}
