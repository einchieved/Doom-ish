using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    public float delay;
    public float explosionRadius;
    public float explosionForce;
    public float upModifier;
    public LayerMask enemyLayer;
    public GameObject particlePrefab;

    private GameObject particleInstance;

    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(Explode), delay);
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        foreach (Collider c in colliders)
        {
            Debug.Log("Collider");
            c.gameObject.GetComponent<BotControl>().DealDamage();
            //c.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, upModifier, ForceMode.Impulse);
        }
        GetComponent<AudioSource>().Play();
        particleInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity);
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        Invoke(nameof(Kill), 5);
    }

    void Kill()
    {
        Destroy(particleInstance);
        Destroy(gameObject);
    }
}
