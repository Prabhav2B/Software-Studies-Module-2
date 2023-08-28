using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LegoBlockBehavior : MonoBehaviour
{
    [SerializeField] private float blockHeight;
    [SerializeField] private int divisionsX;
    [SerializeField] private int divisionsZ;
    public float BlockHeight => blockHeight;

    private Rigidbody _rb;
    private BoxCollider _collider;
    private Bounds _bounds;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
        _bounds = _collider.bounds;
    }

    private void OnEnable()
    {
        //_rb.isKinematic = false;
    }

    private void OnDisable()
    {
        _rb.isKinematic = true;
    }

    public List<Vector3> SectionPoints(Vector3 referencePosition, float yPos)
    {
        var sectionLengthX = _bounds.size.x / divisionsX;
        var sectionLengthZ = _bounds.size.z / divisionsZ;

        var sections = new List<Vector3>();
        
        for (int i = -(divisionsX-1) ; i < 2*divisionsX-1 ; i++)
        {
            for (int j = -(divisionsZ-1); j < 2*divisionsZ-1; j++)
            {
                sections.Add(new Vector3(referencePosition.x + i*sectionLengthX,
                    yPos,
                    referencePosition.z + j*sectionLengthZ));
            }
        }

        return sections;
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     
    //     var bounds = _collider.bounds;
    //     var sectionLengthX = bounds.size.x / divisionsX;
    //     var sectionLengthZ = bounds.size.z / divisionsZ;
    //
    //     for (int i = -(divisionsX-1) ; i < 2*divisionsX-1 ; i++)
    //     {
    //         for (int j = -(divisionsZ-1); j < 2*divisionsZ-1; j++)
    //         {
    //             Gizmos.DrawWireSphere(new Vector3(transform.position.x -bounds.size.x/2f  + i*sectionLengthX,
    //                 transform.position.y,
    //                 transform.position.z - bounds.size.z/2f  + j*sectionLengthZ), .05f); 
    //         }
    //     }
    // }
}
