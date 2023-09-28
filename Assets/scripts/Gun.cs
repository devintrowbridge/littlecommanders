using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour 
{
    private AudioSource gunshot;
    private ParticleSystem gunsmoke;
    private ParticleSystem gunspark;

    bool readyToFire;

    // Start is called before the first frame update
    void Start()
    {
        gunshot = transform.Find("Gunshot").GetComponent<AudioSource>();
        gunsmoke = transform.Find("Gunsmoke").GetComponent<ParticleSystem>();
        gunspark = transform.Find("Gunspark").GetComponent<ParticleSystem>();

        readyToFire = false;
    }

    public void Reload()
    {
        readyToFire = true;
    }

    public void Fire()
    {
        gunshot.Play();
        gunsmoke.Play();
        gunspark.Play();

        var raycast = Physics.Raycast(
            transform.position,
            transform.forward,
            out var hit,
            Mathf.Infinity,
            Constants.LAYER_SOLDIER
        );

        if (raycast) {
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow);
            hit.transform.gameObject.GetComponent<Soldier>().SetColor(Color.red);
        }

        readyToFire = false;
    }
}
