using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkRoomManager : MonoBehaviour
{
    [SerializeField] private float timer = 5f;
    IEnumerator AttackTimer()
    {
        yield return new WaitForSeconds(timer);

    }
}
