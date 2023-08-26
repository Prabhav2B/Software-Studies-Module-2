using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

public class LegoPlacement : MonoBehaviour
{
    [SerializeField] private GameObject legoBlockPrefab;
    [SerializeField] private MeshCollider intersectionPlane;

    [SerializeField] private float maxIntersectionDistance = 30f;

    [SerializeField] private LayerMask intersectionCheckMask;
    
    private GameObject _currentBlock;
    private LegoBlockBehavior _currentLegoBlockBehavior;
    private List<Vector3> _extents;
    private Vector3 _maxExtents;
    
    private Camera _mainCam;

    private void Start()
    {
        if (legoBlockPrefab == null) throw new MissingReferenceException();

        _mainCam = Camera.main;

        _currentBlock = Instantiate(legoBlockPrefab, Vector3.zero, Quaternion.identity);
        _currentLegoBlockBehavior = _currentBlock.GetComponent<LegoBlockBehavior>();
        _currentLegoBlockBehavior.enabled = false;

        _extents = new List<Vector3>();
        PlaceholderBlockPreProcessing(_currentBlock);
    }

    private void PlaceholderBlockPreProcessing(GameObject itemToProcess)
    {
        foreach (var col in itemToProcess.GetComponentsInChildren<Collider>())
        {
            _extents.Add(col.bounds.extents);
            col.enabled = false;
        }
        
        var maxX = _extents.Max(v => v.x);
        var maxY = _extents.Max(v => v.y);
        var maxZ = _extents.Max(v => v.z);

        _maxExtents = new Vector3(maxX, maxY, maxZ);
        
        foreach (var rb in itemToProcess.GetComponentsInChildren<Rigidbody>())
        {
            rb.detectCollisions = false;
            rb.useGravity = false;
        }
    }

    private void FixedUpdate()
    {
        
        //This only works for orthographic cameras (thus v useful in 2D projects)
        // var screenToWorldPoint= _mainCam.ScreenToWorldPoint(Input.mousePosition);
        // _currentBlock.transform.position = new Vector3(screenToWorldPoint.x, screenToWorldPoint.y, 10f);

        RaycastHit hitInfo;
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo, maxIntersectionDistance, 1<<intersectionCheckMask))
        {

            var intersectingColliders = CheckBlockOverlaps();
            if (intersectingColliders.Length != 0)
            {
                var closestCollider = FindClosestCollider(intersectingColliders);
            }

            _currentBlock.transform.position = new Vector3(hitInfo.point.x, 
                hitInfo.point.y + (_currentLegoBlockBehavior.BlockHeight/2f) , 
                hitInfo.point.z);
        }
        else return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Instantiate(legoBlockPrefab, _currentBlock.transform.position, Quaternion.identity);
        }
    }

    private Collider FindClosestCollider(Collider[] intersectingColliders)
    {
        var currentObjectPos = _currentBlock.transform.position;
        
        var closestCollider = intersectingColliders[0];
        var closestPointB = closestCollider.ClosestPointOnBounds (currentObjectPos);
        var distanceB = Vector3.Distance (closestPointB, currentObjectPos);

        foreach (Collider collider in intersectingColliders)
        {
            Vector3 closestPointA = collider.ClosestPointOnBounds (currentObjectPos);
            float distanceA = Vector3.Distance (closestPointA, currentObjectPos);

            if (distanceA < distanceB)
            {
                closestCollider = collider;
                distanceB = distanceA;
            }
        }

        return closestCollider;
    }

    private Collider[] CheckBlockOverlaps()
    {

        var hitColliders = Physics.OverlapBox(_currentBlock.transform.position,
            _maxExtents,
            Quaternion.identity, intersectionCheckMask);

        return hitColliders;
    }
    
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //
    //     //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
    //     //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
    //     Gizmos.DrawWireCube(_currentBlock.transform.position, _maxExtents);
    // }
}
