using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.IK.SubSystems;
using UnityEngine;

namespace NAK.Melons.IKAdjustments;

public class IKOffsetManager : MonoBehaviour
{
    public class GrabState
    {
        public bool handGrabbed;
        public TrackingPoint tracker;
        public Vector3 displayOffset;
        public Quaternion displayOffsetRotation;
        public GrabState otherGrab;
    }

    public enum AdjustMode
    {
        Position = 0,
        Rotation,
        Both,
    }

    public bool isAdjustMode = false;

    public float Setting_MaxGrabDistance = 0.2f;
    public AdjustMode Setting_AdjustMode = AdjustMode.Position;

    GrabState leftGrabState = new GrabState();
    GrabState rightGrabState = new GrabState();

    void Start()
    {
        leftGrabState.otherGrab = rightGrabState;
        rightGrabState.otherGrab = leftGrabState;
    }

    void Update()
    {
        if (!isAdjustMode) return;
        if (BodySystem.isCalibrating)
        {
            isAdjustMode = false;
            return;
        }

        UpdateGrabbing(true, ref leftGrabState);
        UpdateGrabbing(false, ref rightGrabState);
    }

    public void EnterAdjustMode()
    {
        isAdjustMode = true;
        IKSystem.Instance.SetTrackingPointVisibility(true);
        CVR_MenuManager.Instance.ToggleQuickMenu(false);
        foreach (var tracker in IKSystem.Instance.AllTrackingPoints)
        {
            tracker.ClearLineTarget();
        }
    }

    public void ExitAdjustMode()
    {
        isAdjustMode = false;
        IKSystem.Instance.SetTrackingPointVisibility(false);
    }

    public void ResetAllOffsets()
    {
        foreach (var tracker in IKSystem.Instance.AllTrackingPoints)
        {
            tracker.offsetTransform.SetParent(tracker.displayObject.transform, true);
            tracker.displayObject.transform.localPosition = Vector3.zero;
            tracker.displayObject.transform.localRotation = Quaternion.identity;
            tracker.offsetTransform.SetParent(tracker.referenceTransform, true);
        }
    }

    public void CycleAdjustMode()
    {
        int currentValue = (int)Setting_AdjustMode;
        int numValues = Enum.GetValues(typeof(AdjustMode)).Length;
        int nextValue = (currentValue + 1) % numValues;
        Setting_AdjustMode = (AdjustMode)nextValue;
    }

    private void UpdateGrabbing(bool isLeft, ref GrabState grabState)
    {
        bool isGrabbing = isLeft ? CVRInputManager.Instance.gripLeftValue > 0.9f : CVRInputManager.Instance.gripRightValue > 0.9f;
        bool isInteracting = isLeft ? CVRInputManager.Instance.interactLeftValue > 0.9f : CVRInputManager.Instance.interactRightValue > 0.9f;
        Transform handTracker = isLeft ? IKSystem.Instance.leftHandTracker.transform : IKSystem.Instance.rightHandTracker.transform;

        if (grabState.tracker == null && !grabState.handGrabbed && isGrabbing)
        {
            OnGrab(handTracker, grabState);
        }
        else if (grabState.tracker != null)
        {
            if (!isGrabbing)
            {
                OnRelease(grabState);
            }
            else if (isInteracting)
            {
                OnReset(grabState);
            }
            else
            {
                Holding(handTracker, grabState);
            }
        }

        grabState.handGrabbed = isGrabbing;
    }

    private void OnGrab(Transform handTracker, GrabState grabState)
    {
        Transform nearestTransform = FindNearestTransform(handTracker);
        if (nearestTransform != null && Vector3.Distance(nearestTransform.GetChild(0).position, handTracker.position) <= Setting_MaxGrabDistance)
        {
            grabState.tracker = IKSystem.Instance.AllTrackingPoints.Find(tp => tp.referenceTransform == nearestTransform);
            if (grabState.otherGrab.tracker == grabState.tracker) OnRelease(grabState.otherGrab);
            grabState.displayOffset = grabState.tracker.displayObject.transform.position - handTracker.position;
            grabState.displayOffsetRotation = Quaternion.Inverse(handTracker.rotation) * grabState.tracker.displayObject.transform.rotation;
            grabState.tracker.offsetTransform.SetParent(grabState.tracker.displayObject.transform, true);
        }
    }

    private void OnRelease(GrabState grabState)
    {
        grabState.tracker.offsetTransform.SetParent(grabState.tracker.referenceTransform, true);
        grabState.tracker.ClearLineTarget();
        grabState.tracker = null;
    }

    private void OnReset(GrabState grabState)
    {
        grabState.tracker.displayObject.transform.localRotation = Quaternion.identity;
        grabState.tracker.displayObject.transform.localPosition = Vector3.zero;

        grabState.tracker.offsetTransform.SetParent(grabState.tracker.referenceTransform, true);
        grabState.tracker.ClearLineTarget();
        grabState.tracker = null;
    }

    private void Holding(Transform handTracker, GrabState grabState)
    {
        switch (Setting_AdjustMode)
        {
            case AdjustMode.Position:
                grabState.tracker.displayObject.transform.position = handTracker.position + grabState.displayOffset;
                break;
            case AdjustMode.Rotation:
                grabState.tracker.displayObject.transform.rotation = handTracker.rotation * grabState.displayOffsetRotation;
                break;
            case AdjustMode.Both:
                grabState.tracker.displayObject.transform.rotation = handTracker.rotation * grabState.displayOffsetRotation;
                grabState.tracker.displayObject.transform.position = handTracker.position + grabState.displayOffset;
                break;
            default:
                break;
        }
        grabState.tracker.SetLineTarget(grabState.tracker.referenceTransform.position);
    }

    private Transform FindNearestTransform(Transform handTransform)
    {
        var validTrackingPointTransforms = IKSystem._validTrackingPointTransforms;
        if (validTrackingPointTransforms == null || validTrackingPointTransforms.Count == 0) return null;
        return validTrackingPointTransforms.OrderBy(t => Vector3.Distance(handTransform.position, t.GetChild(0).position)).FirstOrDefault();
    }
}