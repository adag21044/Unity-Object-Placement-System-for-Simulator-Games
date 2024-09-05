using GinjaGaming.FinalCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Placement Parameters")]
    [SerializeField] private GameObject placeableObjectPrefab;
    [SerializeField] private GameObject previewObjectPrefab;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask placementSurfaceLayerMask;

    [Header("Preview Material")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;

    [Header("Raycast Parameters")]
    [SerializeField] private float objectDistanceFromPlayer;
    [SerializeField] private float raycastStartVerticalOffset;
    [SerializeField] private float raycastDistance;

    private GameObject _previewObject = null;
    private Vector3 _currentPlacementPosition = Vector3.zero;
    private bool _inPlacementMode = false;
    private bool _validPreviewState = false;

    private void Update()
    {
        UpdateInput();

        if (_inPlacementMode)
        {
            UpdateCurrentPlacementPosition();

            if (CanPlaceObject())
                SetValidPreviewState();
            else
                SetInvalidPreviewState();
        }
    }

    private void UpdateCurrentPlacementPosition()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera is not assigned!");
            return;
        }

        Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z);
        cameraForward.Normalize();

        Vector3 startPos = playerCamera.transform.position + (cameraForward * objectDistanceFromPlayer);
        startPos.y += raycastStartVerticalOffset;

        RaycastHit hitInfo;
        if (Physics.Raycast(startPos, Vector3.down, out hitInfo, raycastDistance, placementSurfaceLayerMask))
        {
            _currentPlacementPosition = hitInfo.point;

            // Yer yüzeyinin biraz üstünde kalması için ekliyoruz (Örneğin objenin yüksekliğinin yarısı kadar)
            float objectHeightOffset = _previewObject != null ? _previewObject.GetComponent<Collider>().bounds.extents.y : 0.5f;
            _currentPlacementPosition.y += objectHeightOffset;
        }

        if (_previewObject != null)
        {
            Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
            _previewObject.transform.position = _currentPlacementPosition;
            _previewObject.transform.rotation = rotation;
        }
        else
        {
            Debug.LogWarning("Preview object is null during placement update.");
        }
    }


    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EnterPlacementMode();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ExitPlacementMode();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
    }

    private void SetValidPreviewState()
    {
        if (previewMaterial == null)
        {
            Debug.Log("Preview material is not assigned.");
            return;
        }

        previewMaterial.color = validColor;
        _validPreviewState = true;
    }

    private void SetInvalidPreviewState()
    {
        if (previewMaterial == null)
        {
            Debug.Log("Preview material is not assigned.");
            return;
        }

        previewMaterial.color = invalidColor;
        _validPreviewState = false;
    }

    private bool CanPlaceObject()
    {
        if (previewMaterial == null)
        {
            Debug.Log("Preview material is not assigned.");
            return false;
        }

        if (_previewObject == null)
            return false;

        var checker = _previewObject.GetComponentInChildren<PreviewObjectValidChecker>();
        if (checker == null)
            return false;

        return checker.IsValid;
    }

    private void PlaceObject()
    {
        if (!_inPlacementMode || !_validPreviewState)
            return;

        Debug.Log("Placing object at: " + _currentPlacementPosition);

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        Instantiate(placeableObjectPrefab, _currentPlacementPosition, rotation, transform);

        ExitPlacementMode();
    }

    private void EnterPlacementMode()
    {
        if (_inPlacementMode)
            return;

        PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.Disable();

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        _previewObject = Instantiate(previewObjectPrefab, _currentPlacementPosition, rotation, transform);

        // Check if the instantiated preview object has the necessary component
        if (_previewObject.GetComponentInChildren<PreviewObjectValidChecker>() == null)
        {
            Debug.Log("Preview object does not contain a PreviewObjectValidChecker component!");
        }

        _inPlacementMode = true;
    }


    private void ExitPlacementMode()
    {
        PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.Enable();

        Destroy(_previewObject);
        _previewObject = null;
        _inPlacementMode = false;
    }
}