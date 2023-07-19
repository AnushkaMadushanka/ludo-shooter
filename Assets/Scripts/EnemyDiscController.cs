using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyDiscController : MonoBehaviour
{
    private Transform Target;
    public float UpdateSpeed = 0.1f;
    public float minDistanceToDetonate = 1.5f;
    public float detonateRadius = 1;
    public GameObject vfx;

    private bool detonated = false;

    private NavMeshAgent agent;
    private void Awake()
    {
        Target = FindObjectOfType<PlayerController>()?.transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(FollowTarget());
    }

    private void FixedUpdate()
    {
        if (!Target) return;
        if (!detonated && Vector3.Distance(transform.position, Target.position) < minDistanceToDetonate)
        {
            detonated = true;
            agent.isStopped = true;
            var hits = Physics.OverlapSphere(transform.position, detonateRadius);

            foreach (var hit in hits)
            {
                if (hit.transform != transform)
                    hit.GetComponent<HealthScript>()?.SetDamage(10);
            }
            Destroy(gameObject);
        }
    }

    private IEnumerator FollowTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(UpdateSpeed);
        while (enabled && Target)
        {
            agent.SetDestination(Target.transform.position);
            yield return wait;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance.isApplicationQuitting) return;
        var vfxObj = Instantiate(vfx, transform.position, Quaternion.identity);
        vfxObj.GetComponent<AudioSource>().Play();
        Destroy(vfxObj, 2f);
        GameManager.instance.RemoveEnemy();
    }
}
