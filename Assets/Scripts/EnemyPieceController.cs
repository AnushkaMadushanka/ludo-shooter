using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPieceController : MonoBehaviour
{
    private Transform Target;
    public Gradient aimingGradient;
    public Gradient trailGradient;
    [SerializeField]
    private GameObject hitVfxPrefab;
    [SerializeField]
    private float bulletHitMissDistance = 25f;

    public Transform bulletPoint;
    public AudioSource shootSFX;
    public AudioSource aimSFX;

    private LineRenderer lineRenderer;
    private NavMeshAgent agent;
    private float timer = 2f;
    private bool shooting = false;

    private void Awake()
    {
        Target = FindObjectOfType<PlayerController>()?.transform;
        agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Target) return; 
        bulletPoint.parent.transform.LookAt(Target);
        if (shooting) return;
        Vector3 aimDir = (Target.position - bulletPoint.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(bulletPoint.position, aimDir, out hit, Mathf.Infinity) && hit.transform == Target) {
            agent.isStopped = true;
            if(timer > 0)
            {
                if (!lineRenderer.enabled) aimSFX.Play();
                timer -= Time.fixedDeltaTime;
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, bulletPoint.position);
                lineRenderer.SetPosition(1, Target.position);
                lineRenderer.colorGradient = aimingGradient;
            }
            else
            {
                lineRenderer.enabled = false;
                StartCoroutine("ShootBullet");
                timer = 2f;
            }
        } else
        {
            agent.isStopped = false;
            timer = 2f;
            agent.SetDestination(Target.position);
            lineRenderer.enabled = false;
        }
    }

    IEnumerator ShootBullet()
    {
        shooting = true;
        yield return new WaitForSeconds(1f);
        lineRenderer.enabled = true;
        RaycastHit hit;
        Vector3 aimDir = (lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)).normalized;
        var collided = Physics.Raycast(lineRenderer.GetPosition(0), aimDir, out hit, Mathf.Infinity);
        var point = collided ? hit.point : lineRenderer.GetPosition(0) + aimDir * bulletHitMissDistance;
        lineRenderer.colorGradient = trailGradient;
        lineRenderer.SetPosition(1, point);
        hit.transform?.GetComponent<HealthScript>()?.SetDamage(10f);
        shootSFX.Play();
        if (collided && hitVfxPrefab != null)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            var hitVFX = Instantiate(hitVfxPrefab, hit.point, rot);
            var ps = hitVFX.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitVFX, psChild.main.duration);
            }
            else
                Destroy(hitVFX, ps.main.duration);
        }
        yield return new WaitForSeconds(0.1f);
        lineRenderer.enabled = false;
        agent.isStopped = false;
        agent.SetDestination(RandomNavmeshLocation(3f));
        yield return new WaitForSeconds(3f);
        shooting = false;
    }

    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    private void OnDestroy()
    {
        if (GameManager.instance.isApplicationQuitting) return;
        GameManager.instance.RemoveEnemy();
    }
}
