using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private bool _inputBuffer;
    private bool _isSnapping;
    
    
    private Camera _mainCam;

    private const int GroundLayer = 6;
    private const int LegoLayer = 7;

    private void Start()
    {
        if (legoBlockPrefab == null) throw new MissingReferenceException();

        _mainCam = Camera.main;

        _currentBlock = Instantiate(legoBlockPrefab, Vector3.zero, Quaternion.identity);
        _currentLegoBlockBehavior = _currentBlock.GetComponent<LegoBlockBehavior>();
        _currentLegoBlockBehavior.enabled = false;

        _extents = new List<Vector3>();
        PlaceholderBlockPreProcessing(_currentBlock);

        _inputBuffer = false;
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

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        _inputBuffer = true;
    }

    private void FixedUpdate()
    {
        
        //This only works for orthographic cameras (thus v useful in 2D projects)
        // var screenToWorldPoint= _mainCam.ScreenToWorldPoint(Input.mousePosition);
        // _currentBlock.transform.position = new Vector3(screenToWorldPoint.x, screenToWorldPoint.y, 10f);

        RaycastHit hitInfo;
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo, maxIntersectionDistance, intersectionCheckMask))
        {
            var placementPosition = new Vector3(hitInfo.point.x, 
                    hitInfo.point.y + (_currentLegoBlockBehavior.BlockHeight/2f) , 
                    hitInfo.point.z);

            Collider closestCollider = null;

            do
            {
                
                var intersectingBlockColliders = CheckBlockOverlaps(placementPosition);
                if (intersectingBlockColliders.Length != 0)
                {
                    closestCollider = FindClosestBlockCollider(intersectingBlockColliders);
                    placementPosition = new Vector3(placementPosition.x, 
                        closestCollider.bounds.center.y + _currentLegoBlockBehavior.BlockHeight, 
                        placementPosition.z);
                    _isSnapping = true;
                }
                else
                {
                    break;
                }
            } while (true);

            if (hitInfo.collider.gameObject.layer == LegoLayer)
            {
                _isSnapping = true;
                closestCollider = hitInfo.collider;
            }

            _currentBlock.transform.position = _isSnapping ? SnapPosition(closestCollider.transform.position, placementPosition) : placementPosition;
            _isSnapping = false;
        }
        else return;

        if (!_inputBuffer) return;
        
        Instantiate(legoBlockPrefab, _currentBlock.transform.position, Quaternion.identity);
        _inputBuffer = false;

    }

    private Vector3 SnapPosition(Vector3 referencePosition, Vector3 placementPosition)
    {
        var sectionPositions = _currentLegoBlockBehavior.SectionPoints(referencePosition, placementPosition.y);
        
        var closestSection = sectionPositions[0];
        var distanceB = Vector3.Distance (placementPosition, sectionPositions[0]);

        foreach (var sectionPosition in sectionPositions)
        {
            var distanceA = Vector3.Distance (placementPosition, sectionPosition);

            if (!(distanceA < distanceB)) continue;
            closestSection = sectionPosition;
            distanceB = distanceA;
        }
        
        return closestSection;
    }
    
    private Collider FindClosestBlockCollider(Collider[] intersectingColliders)
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

    private Collider[] CheckBlockOverlaps(Vector3 overlapBoxCentre)
    {

        var hitColliders = Physics.OverlapBox(overlapBoxCentre,
            _maxExtents * 0.90f,
            Quaternion.identity, 1<<LegoLayer);
        
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
