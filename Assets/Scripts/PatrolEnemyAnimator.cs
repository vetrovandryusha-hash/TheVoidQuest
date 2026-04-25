using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemyAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    public static PatrolEnemyAnimator Instance;
    public Animator enemyAnimator;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        enemyAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void walkingAnimation()
    {
        enemyAnimator.SetBool("isWalking", true);
    }
    public void stopWalking()
    {
        enemyAnimator.SetBool("isWalking", false);
    }
    public void runningAnimation()
    {
        enemyAnimator.SetBool("isRun", true);
    }
    public void stopRunning()
    {
        enemyAnimator.SetBool("isRun", false);
    }
}
