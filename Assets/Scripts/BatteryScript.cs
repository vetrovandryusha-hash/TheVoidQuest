using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BatteryScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Outline outline;
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerScript playerScript;
    private Transform playerPosition;
    void Start()
    {
        playerScript = player.GetComponent<PlayerScript>();
        outline = GetComponent<Outline>();
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = GameObject.Find("Player").transform;
        Vector3 playerDistance = playerPosition.position;
        if (outline == null || playerScript == null) return;

        if (playerScript.targetedObject == this.gameObject)
        {
            outline.enabled = true;
            if (Input.GetMouseButtonDown(0) && Vector3.Distance(transform.position, playerDistance) <= 3f)
            {
                Destroy(this.gameObject);
                
                if(playerScript.currentBattery >= 50)
                {
                    playerScript.countBattery++;
                }
                if (playerScript.currentBattery < 50f)
                {
                    playerScript.currentBattery = 100f;
                }
            }
        }
        else
        {
            outline.enabled = false;
        }
    }
}
