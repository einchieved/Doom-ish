using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunControl : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject flame;
    public Transform spawnPosition;
    public float shotDelay;
    public bool useHitscan = true;
    public LayerMask enemyLayer;

    private float firePause;
    private MeshRenderer flameMeshRender;

    // Called before the first frame
    private void Start()
    {
        flameMeshRender = flame.GetComponent<MeshRenderer>();
        flameMeshRender.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        firePause -= Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && firePause <= 0f)
        {
            if (useHitscan)
            {
                // Hitscan
                RaycastHit hit;
                Debug.DrawRay(spawnPosition.position, spawnPosition.forward * 600f, Color.green, 1f);
                if (Physics.Raycast(spawnPosition.position, spawnPosition.forward, out hit, 300f, enemyLayer))
                {
                    BotControl bCtrl = hit.collider.GetComponent<BotControl>();
                    if (bCtrl != null)
                    {
                        bCtrl.DealDamage();
                    }
                }
            }
            else
            {
                // Projectile
                Vector3 v = spawnPosition.position + spawnPosition.forward;
                Instantiate(bulletPrefab, v, spawnPosition.rotation);
            }
            EnableFlame();
            firePause = shotDelay - Time.deltaTime;
        }
        else if (firePause > 0f)
        {
            firePause -= Time.deltaTime;
        }
    }


    private void EnableFlame()
    {
        flameMeshRender.enabled = true;
        Invoke(nameof(DisableFlame), 0.1f);
    }

    private void DisableFlame()
    {
        flameMeshRender.enabled = false;
    }
}
