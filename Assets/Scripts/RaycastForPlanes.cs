using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RaycastForPlanes : MonoBehaviour
{
    [Tooltip("The event to call when a plane is hit")]
    public UnityEvent<Vector3> OnPlaneHit;

    ARRaycastManager m_RaycastManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DoRaycast();
        }
    }

    private void DoRaycast()
    {
        var touchPosition = Input.mousePosition;

        // avoid raycasting when clicking UI
        if (EventSystem.current.currentSelectedGameObject != null) return;

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;

            OnPlaneHit?.Invoke(hitPose.position);
        }
#if UNITY_EDITOR
        // Debug hits on editor
        OnPlaneHit?.Invoke(Camera.main.ScreenToWorldPoint(touchPosition));
#endif
    }
}
