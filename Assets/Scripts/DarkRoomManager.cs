using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DarkRoomManager : MonoBehaviour
{
    [SerializeField] private float timer = 5f;
    private GameObject screamerPanel;
    IEnumerator AttackTimer()
    {
        yield return new WaitForSeconds(timer);
        //screamerPanel.SetActive(true);
        screamerPanel = GameObject.Find("ScreamerPanel");
        screamerPanel.GetComponent<Image>().color = new Color(0, 0, 0, 255f);
        screamerPanel.transform.GetChild(0).gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Timer started.");
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine("AttackTimer");
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Timer stopped.");
        if (other.gameObject.CompareTag("Player"))
        {
            StopCoroutine("AttackTimer");
        }
    }
}
