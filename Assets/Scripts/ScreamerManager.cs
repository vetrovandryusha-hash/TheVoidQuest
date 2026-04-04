using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreamerManager : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject screamerPanel;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //screamerPanel.SetActive(true);
            screamerPanel = GameObject.Find("ScreamerPanel");
            screamerPanel.GetComponent<Image>().color = new Color(0, 0, 0, 255f);
            screamerPanel.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
