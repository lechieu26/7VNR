using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport : MonoBehaviour
{
    public Action OnEnterTransport;
    public string mapNext;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Transporting to new scene...");
            OnEnterTransport?.Invoke();
        }
    }
}
