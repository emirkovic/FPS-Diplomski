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
         if (!playerTargetPoint) return;
         turretHead.LookAt(playerTargetPoint);
     }

     IEnumerator FireProjectiles()
     {
         while (player)
         {
             yield return new WaitForSeconds(fireRate);
             if (!player) yield break;
                Projectile newProjectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, turretHead.rotation).GetComponent<Projectile>();
                newProjectile.transform.LookAt(playerTargetPoint);
                newProjectile.Init(damage);
         }
     }
}
