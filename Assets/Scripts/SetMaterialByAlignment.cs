using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Updates the material of the associated MeshRenderer based on the alignment of the associated ARPlane
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(ARPlaneMeshVisualizer))]
public class SetMaterialByAlignment : MonoBehaviour
{
    [SerializeField] Material horizontalMat;
    [SerializeField] Material verticalMat;
    [SerializeField] Material unalignedMat;

    ARPlane plane;
    MeshRenderer meshRenderer;
    UnityEngine.XR.ARSubsystems.PlaneAlignment lastAlignment;

    // Start is called before the first frame update
    void Start()
    {
        plane = this.GetComponent<ARPlane>();
        meshRenderer = this.GetComponent<MeshRenderer>();
        UpdateAlignmentMaterial(plane.alignment);
    }

    public void UpdateAlignmentMaterial(UnityEngine.XR.ARSubsystems.PlaneAlignment alignment)
    {
        if (lastAlignment.Equals(alignment)) return;
        Debug.Log($"Updating AlignmentMaterial for plane {plane.trackableId} to {alignment}");
        lastAlignment = alignment;

        switch (alignment)
        {
            case UnityEngine.XR.ARSubsystems.PlaneAlignment.None:
            case UnityEngine.XR.ARSubsystems.PlaneAlignment.NotAxisAligned:
                meshRenderer.material = unalignedMat;
                break;
            case UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalUp:
            case UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalDown:
                meshRenderer.material = horizontalMat;
                break;
            case UnityEngine.XR.ARSubsystems.PlaneAlignment.Vertical:
                meshRenderer.material = verticalMat;
                break;
        }
    }
}
