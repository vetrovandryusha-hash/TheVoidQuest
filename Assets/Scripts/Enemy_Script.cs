using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Script : MonoBehaviour
{
    // Start is called before the first frame update
    private NavMeshAgent agent;
    private Transform playerPosition;
    [SerializeField] private float enemySpeed = 5f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private int indexPoint;
    void Start()
    {

        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemySpeed;
        playerPosition = GameObject.Find("Player").transform;
        //agent.destination = playerPosition.position;
        //agent.destination = patrolPoints[indexPoint].position;

    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = GameObject.Find("Player").transform;
        Vector3 playerDistance = playerPosition.position;
        if (playerPosition != null && Vector3.Distance(transform.position, playerDistance) < 5f)
        {
            stalkerBehaviour();
        }
        else
        {
            patrolBehaviour();
        }
    }
    private void checkPatrolTarget()
    {
        Vector3 targetPosition = patrolPoints[indexPoint].position;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine("walkingDelay");
            if (indexPoint < patrolPoints.Length - 1)
            {
                indexPoint++;
            }
            else
            {
                indexPoint = 0;
            }
        }
    }
    private void patrolBehaviour()
    {
        if (patrolPoints.Length > 0)
        {
            agent.destination = patrolPoints[indexPoint].position;
            checkPatrolTarget();
            PatrolEnemyAnimator.Instance.WalkingAnimation();
        }
    }
    private void stalkerBehaviour()
    {
        agent.destination = playerPosition.position;
        PatrolEnemyAnimator.Instance.WalkingAnimation();
    }
    IEnumerator walkingDelay()
    {
        enemySpeed = 0f;
        yield return new WaitForSecondsRealtime(2f);
        enemySpeed = 5f;
    }
}
