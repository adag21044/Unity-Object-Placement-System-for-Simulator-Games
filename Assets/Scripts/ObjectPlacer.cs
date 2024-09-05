using GinjaGaming.FinalCharacterController;
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

    // Update is called once per frame
    private void Update()
    {
        UpdateInput();

        if (_inPlacementMode)
        {
            UpdateCurrentPlacementPosition();

            if (CanPlaceObject())
            {
                SetValidPreviewState();
            }
            else
            {
                SetInvalidPreviewState();
            }
        }
    }

    private void UpdateCurrentPlacementPosition()
    {
        Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z);
        cameraForward.Normalize();

        Vector3 startPos = playerCamera.transform.position + (cameraForward * objectDistanceFromPlayer);
        startPos.y += raycastStartVerticalOffset;

        RaycastHit hitInfo;
        if (Physics.Raycast(startPos, Vector3.down, out hitInfo, raycastDistance, placementSurfaceLayerMask))
        {
            _currentPlacementPosition = hitInfo.point;
            Debug.Log("Hit position: " + _currentPlacementPosition);
        }
        else
        {
            Debug.Log("No surface hit detected.");
        }

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);

        // Önizleme objesinin aktif olduğunu ve pozisyonunun güncellendiğini kontrol edin
        if (_previewObject != null)
        {
            _previewObject.SetActive(true); // Önizleme objesinin aktif olduğundan emin olun
            _previewObject.transform.position = _currentPlacementPosition; // Pozisyonu güncelle
            _previewObject.transform.rotation = rotation; // Rotasyonu güncelle

            Debug.Log("Preview Object Position: " + _previewObject.transform.position);
            Debug.Log("Preview Object Rotation: " + _previewObject.transform.rotation);
        }
        else
        {
            Debug.LogWarning("Preview object is not set or inactive."); // Hata mesajı ver
        }
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("1 tuşuna basıldı.");
            EnterPlacementMode();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ExitPlacementMode();
        }
        else if (Input.GetMouseButton(0))
        {
            PlaceObject();
        }
    }

    private void SetValidPreviewState()
    {
        previewMaterial.color = validColor;
        _validPreviewState = true;
    }

    private void SetInvalidPreviewState()
    {
        previewMaterial.color = invalidColor;
        _validPreviewState = false;
    }

    private bool CanPlaceObject()
    {
        if (_previewObject == null) return false;

        return _previewObject.GetComponentInChildren<PreviewObjectValidChecker>().IsValid;
    }

    private void PlaceObject()
    {
        if (!_inPlacementMode || !_validPreviewState) return;

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        Instantiate(placeableObjectPrefab, _currentPlacementPosition, rotation, transform);

        ExitPlacementMode();
    }

    private void EnterPlacementMode()
    {
        Debug.Log("EnterPlacementMode");

        if (_inPlacementMode) return;

        PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.Disable();

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        _previewObject = Instantiate(previewObjectPrefab, _currentPlacementPosition, rotation, transform);

        // Önizleme materyalini ata
        var renderers = _previewObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.sharedMaterial = previewMaterial; // sharedMaterial kullanımı bazen daha doğru olur
        }

        _inPlacementMode = true;
    }

    private void ExitPlacementMode()
    {
        Debug.Log("ExitPlacementMode");

        PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.Enable();

        Destroy(_previewObject);
        _previewObject = null;

        _inPlacementMode = false;
    }
}
