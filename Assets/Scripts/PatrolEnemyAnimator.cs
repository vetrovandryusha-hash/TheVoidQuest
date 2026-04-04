using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemyAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    public static PatrolEnemyAnimator Instance;
    private Animator enemyAnimator;
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
    public void WalkingAnimation()
    {
        enemyAnimator.SetBool("isWalking", true);
    }
    public void StopWalking()
    {
        enemyAnimator.SetBool("isWalking", false);
    }
}
