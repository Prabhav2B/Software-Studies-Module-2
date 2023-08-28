using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LegoBlockBehavior : MonoBehaviour
{
    [SerializeField] private float blockHeight;
    [SerializeField] private float divisionsX;
    [SerializeField] private float divisionsZ;
    public float BlockHeight => blockHeight;

    private Rigidbody _rb;
    private BoxCollider _collider;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        _rb.isKinematic = false;
    }

    private void OnDisable()
    {
        _rb.isKinematic = true;
    }

    public List<Vector2> SectionPoints()
    {
        var bounds = _collider.bounds;
        var sectionLengthX = bounds.size.x / divisionsX;
        var sectionLengthZ = bounds.size.y / divisionsZ;

        var sections = new List<Vector2>();
        
        for (int i = 0; i < divisionsX; i++)
        {
            for (int j = 0; j < divisionsZ; j++)
            {
                sections.Add(new Vector2(transform.position.x -bounds.size.x/2f  + i*sectionLengthX,
                    transform.position.z - bounds.size.z/2f  + j*sectionLengthZ));
            }
        }

        return sections;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        
        var bounds = _collider.bounds;
        var sectionLengthX = bounds.size.x / divisionsX;
        var sectionLengthZ = bounds.size.z / divisionsZ;

        var sections = new List<Vector2>();
        
        for (int i = 0; i < divisionsX; i++)
        {
            for (int j = 0; j < divisionsZ; j++)
            {
                Gizmos.DrawWireSphere(new Vector3(transform.position.x -bounds.size.x/2f  + i*sectionLengthX,
                    transform.position.y + blockHeight/2f,
                    transform.position.z - bounds.size.z/2f  + j*sectionLengthZ), .05f); 
            }
        }
    }
}
