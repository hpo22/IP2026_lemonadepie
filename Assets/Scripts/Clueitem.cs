using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ClueItemGrab : MonoBehaviour
{
    public string itemId = "clue_001";
    public string itemName = "Keycard";

    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
    }

    private void OnDestroy()
    {
        if (grab != null) grab.selectEntered.RemoveListener(OnGrabbed);
    }

    private async void OnGrabbed(SelectEnterEventArgs args)
    {
        if (ClueTrackerRealtimeDB.Instance != null)
        {
            await ClueTrackerRealtimeDB.Instance.RegisterClueAsync(itemId, itemName);
        }
    }
}

