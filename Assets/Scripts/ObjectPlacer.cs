using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] private GameObject placeableObjectPrefab;
    [SerializeField] private GameObject previewObjectPrefab;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask placementSurfaceLayerMask;

    [SerializeField] private float objectDistanceFromPlayer;
    [SerializeField] private float raycastStartVerticalOffset;
    [SerializeField] private float raycastDistance;

    private GameObject _previewObject = null;
    private Vector3 _currentPlacementPosition = Vector3.zero;
    private bool _inPlacementMode = false;


    // Update is called once per frame
    private void Update()
    {
        UpdateInput();

        if(_inPlacementMode)
        {
            UpdateCurrentPlacementPosition();
        }
    }

    private void UpdateCurrentPlacementPosition()
    {
        // Define a forward vector based on the player's camera direction but flat on the x-z plane.
        Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z);
        cameraForward.Normalize();

        // Start position for the raycast, adjusted by distance from the player and vertical offset.
        Vector3 startPos = playerCamera.transform.position + (cameraForward * objectDistanceFromPlayer);
        startPos.y += raycastStartVerticalOffset;

        RaycastHit hitInfo;

        // Perform the raycast downward from the start position to detect the terrain.
        if (Physics.Raycast(startPos, Vector3.down, out hitInfo, raycastDistance, placementSurfaceLayerMask))
        {
            _currentPlacementPosition = hitInfo.point;

            // Get the object's height to place it correctly above the terrain.
            float objectHeight = previewObjectPrefab.GetComponent<Renderer>().bounds.size.y;

            // Adjust the placement position so the object sits on the terrain surface.
            _currentPlacementPosition.y += objectHeight / 2; 
        }

        // Set the rotation to match the camera's horizontal direction.
        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        _previewObject.transform.position = _currentPlacementPosition;
        _previewObject.transform.rotation = rotation;
    }


    private void UpdateInput()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            EnterPlacementMode();
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ExitPlacementMode();
        }
    }

    private void EnterPlacementMode()
    {
        Debug.Log("EnterPlacementMode");

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f); 
        _previewObject = Instantiate(previewObjectPrefab, _currentPlacementPosition, rotation, transform);
        _inPlacementMode = true;
    }

    private void ExitPlacementMode()
    {
        Debug.Log("ExitPlacementMode");
        _inPlacementMode = false;
    }

}
