using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LegoBlockBehavior : MonoBehaviour
{
    [SerializeField] private float blockHeight;
    public float BlockHeight => blockHeight;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _rb.isKinematic = true;
    }

    private void OnDisable()
    {
        _rb.isKinematic = false;
    }
}
