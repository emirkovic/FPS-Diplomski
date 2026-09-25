using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
     [SerializeField] private Transform turretHead;
     [SerializeField] private Transform playerTargetPoint;
     [SerializeField] private Transform projectileSpawnPoint;
     [SerializeField] float fireRate = 1f;
     [SerializeField] int damage = 2;

     PlayerHealth player;

     void Start()
     {
         player = FindFirstObjectByType<PlayerHealth>();
         StartCoroutine(FireProjectiles());
     }
     
     void Update()
     {
         turretHead.LookAt(playerTargetPoint);
     }

     IEnumerator FireProjectiles()
     {
         while (player)
         {
             yield return new WaitForSeconds(fireRate);
             Instantiate(projectilePrefab, projectileSpawnPoint.position, turretHead.rotation);
                Projectile newProjectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, turretHead.rotation).GetComponent<Projectile>();
                newProjectile.transform.LookAt(playerTargetPoint);
                newProjectile.Init(damage);
         }
     }
}
