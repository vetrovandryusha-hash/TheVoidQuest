using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Script : MonoBehaviour
{
    // Start is called before the first frame update
    private NavMeshAgent agent;
    private Transform playerPosition;
    [SerializeField] private float enemySpeed = 3f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private int indexPoint;
    [SerializeField] private bool isDelay = false;
    [SerializeField] private bool isRotating = false;
    [SerializeField] private float enemyDelay = 2f;
    [SerializeField] private bool isHit = false;
    private float timeDelay = 2f;
    void Start()
    { 
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemySpeed;
    }

    // Update is called once per frame
    void Update()
    {
       
        playerPosition = GameObject.Find("Player").transform;
        Vector3 playerDistance = playerPosition.position;
        if (playerPosition != null && Vector3.Distance(transform.position, playerDistance) <= 10f && isDelay != true)
        {
            agent.destination = playerPosition.position;
            stalkerBehaviour();
        }
        else if (isDelay)
        {
            StartCoroutine("walkingDelay");
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 180, 0), -180f * Time.deltaTime);
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
            isDelay = true;
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
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isRun", false);
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("canHit", false);
            PatrolEnemyAnimator.Instance.walkingAnimation();
        }
    }
    private void stalkerBehaviour()
    {
        Vector3 playerDistance = playerPosition.position;
        AnimatorStateInfo stateInfo = PatrolEnemyAnimator.Instance.enemyAnimator.GetCurrentAnimatorStateInfo(0);
        enemySpeed = 7f;
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isWalking", false);
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isRun", true);
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("canHit", false);
        if(Vector3.Distance(transform.position, playerDistance) <= 2f)
        {
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isWalking", false);
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isRun", false);
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("canHit", true);
            if(stateInfo.IsName("Base Layer.Hit") && stateInfo.normalizedTime >= 1.0f && Vector3.Distance(transform.position, playerDistance) <= 2f)
            {
                PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isRun", true);
                PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("canHit", false);
            }
           
        }
    }
    IEnumerator walkingDelay()
    {
        enemySpeed = 0f;
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isWalking", false);
        yield return new WaitForSecondsRealtime(enemyDelay);
        enemySpeed = 3f;
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isWalking", true);
        isDelay = false;
    }
}
