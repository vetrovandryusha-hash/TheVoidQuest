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
    public bool isAttacking = false;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private bool isLampTriggerActive;
    [SerializeField] GameObject Lamp;
    private float timeDelay = 2f;
    void Start()
    { 
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemySpeed;
        //enemyAnimator = GetComponent<Animator>();
        agent.updateRotation = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        isAttacking = enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
        playerPosition = GameObject.Find("Player").transform;
        Vector3 playerDistance = playerPosition.position;
        if (Vector3.Distance(transform.position, playerDistance) <= 9f && Lamp.activeInHierarchy == true)
        {
            isLampTriggerActive = true;
        }
        if(Vector3.Distance(transform.position, playerDistance) >= 9f && Lamp.activeInHierarchy == false)
        {
            isLampTriggerActive = false;
        }
        if (!isLampTriggerActive)
        {
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
        else
        {
            
            if (playerPosition != null && Vector3.Distance(transform.position, playerDistance) <= 10f && isDelay != true)
            {
                agent.destination = patrolPoints[indexPoint].position;
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
        
        enemySpeed = 7f;
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isWalking", false);
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isRun", true);
        PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("canHit", false);

        //
        RotateToPlayer();

        if(Vector3.Distance(transform.position, playerDistance) <= 2f)
        {
            agent.destination = transform.position;
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isWalking", false);
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isRun", false);
            PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("canHit", true);
            //
            AnimatorStateInfo stateInfo = PatrolEnemyAnimator.Instance.enemyAnimator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("Base Layer.Hit") && stateInfo.normalizedTime >= 1.0f && Vector3.Distance(transform.position, playerDistance) <= 2f)
            {
                PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("isRun", true);
                PatrolEnemyAnimator.Instance.enemyAnimator.SetBool("canHit", false);
            }
           
        }
    }
    private void RotateToPlayer()
    {
        Vector3 direction = playerPosition.position - transform.position;
        direction.y = 0; // чтобы не заваливался вверх/вниз

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 7f // скорость поворота
            );
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
    /*private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("TriggerLight") && other.gameObject.layer == LayerMask.NameToLayer("LightTrigger") && Lamp.activeInHierarchy == true)
        {
            isLampTriggerActive = true;
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("TriggerLight") && other.gameObject.layer == LayerMask.NameToLayer("LightTrigger") && Lamp.activeInHierarchy == false)
        {
            isLampTriggerActive = false;
        }
        
    }
    */
    
    public void setLampStatus(bool status)
    {
        isLampTriggerActive = status;
    }
}
