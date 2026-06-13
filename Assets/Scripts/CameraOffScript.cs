using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOffScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject cameraSelf;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.activeInHierarchy)
        {
            cameraSelf.SetActive(false);
        }
    }
}
