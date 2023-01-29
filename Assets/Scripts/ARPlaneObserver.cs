using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Listens for ARPlaneManager.planesChanged and keeps a list of the ARPlanes found
/// Updates their materials based on the alignment
/// </summary>
public class ARPlaneObserver : MonoBehaviour
{
    private List<ARPlane> arPlanes;
    private ARPlaneManager arPlaneManager;
    public Toggle debugToggle;
    public List<ARPlane> ARPlanes { get { return arPlanes; } }

    // Start is called before the first frame update
    void OnEnable()
    {
        arPlanes = new List<ARPlane>();
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        arPlaneManager.planesChanged += ArPlaneManager_planesChanged;
    }

    private void OnDisable()
    {
        arPlaneManager.planesChanged -= ArPlaneManager_planesChanged;
    }

    private void ArPlaneManager_planesChanged(ARPlanesChangedEventArgs args)
    {
        // this isn't reliable
        //if (args.added != null && args.added.Count > 0)
        //    arPlanes.AddRange(args.added);
        //if (args.removed != null && args.removed.Count > 0)
        //    foreach (var removed in args.removed)
        //        arPlanes.Remove(removed);
        
        arPlanes.Clear();
        var planes = FindObjectsOfType<ARPlane>();
        arPlanes.AddRange(planes);
    }

    public void ClearPlanes()
    {
        arPlanes.Clear();
    }

    public void ShowPlanes(bool show)
    {
        foreach (var plane in arPlanes)
        {
            plane.GetComponent<ARPlaneMeshVisualizer>().enabled = show;
        }
    }

    void Update()
    {
        // update the AlignmentMaterial of all planes every frame.
        foreach (var plane in arPlanes)
        {
            plane.GetComponent<SetMaterialByAlignment>().UpdateAlignmentMaterial(plane.alignment);
        }
        ShowPlanes(debugToggle.isOn);
    }
}
