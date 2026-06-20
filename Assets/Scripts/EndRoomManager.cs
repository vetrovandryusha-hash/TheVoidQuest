using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndRoomManager : MonoBehaviour
{
    [SerializeField] private GameObject endSphere;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            endSphere.SetActive(true);
        }
    }
}
