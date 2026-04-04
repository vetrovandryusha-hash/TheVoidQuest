using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamerManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField ]private GameObject screamerPanel;
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
            screamerPanel.SetActive(true);
        }
    }
}
