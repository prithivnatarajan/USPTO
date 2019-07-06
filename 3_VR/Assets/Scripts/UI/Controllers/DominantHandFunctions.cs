using UnityEngine;
using VRTK;
using System.Collections;

public class DominantHandFunctions : MonoBehaviour {

	/*
	 * This script handles the following:
	 * 1) Movement control - Rotation and up-down movement - Controlled using touchpadtwo
	 * 2) Settings pointer - Controlled using touchpad
	 * 
	 */

	public delegate void voidDelegate();
	public static voidDelegate OnTriggerClicked;
	public static voidDelegate OnTouchpadPressed;

	public VRTK_ControllerEvents controllerEvents;
	public VRTK_DestinationMarker pointer;
	[Range(0.3f, 1f)] public float rotationSpeed = 0.5f;

	private bool rotationAllowed = true;
	private float moveUD;
	private Vector3 rotationPoint;
	private Transform head;
	private Transform headParent;
	private DragState dragState = DragState.idle;
	private PointerDelegates draggingPointerDelegate;

	public enum DragState {
		idle,
		hovering,
		caught,
		dragging
		//released = back to idle
	}

	private void Update() {
		// Movement control

		if (head == null) {
			head = VRTK_DeviceFinder.HeadsetTransform();
			headParent = VRTK_SDKManager.instance.transform;
		}

		if (Mathf.Abs(moveUD) > 0.2f) {
			headParent.position += head.up * moveUD * 0.035f;
		}
	}

	private void OnEnable() {
		controllerEvents.TouchpadTwoAxisChanged += DoTouchpadTwoAxisChanged;
		controllerEvents.TouchpadPressed += DoTouchpadPressed;
		controllerEvents.TouchpadReleased += DoTouchpadReleased;
		controllerEvents.TriggerClicked += DoTriggerClicked;

		pointer.DestinationMarkerEnter += DestinationMarkerEnter;
		pointer.DestinationMarkerExit += DestinationMarkerExit;
		pointer.DestinationMarkerHover += DestinationMarkerHover;
		pointer.DestinationMarkerSet += DestinationMarkerSet;
	}

	private void OnDisable() {
		controllerEvents.TouchpadTwoAxisChanged -= DoTouchpadTwoAxisChanged;
		controllerEvents.TouchpadPressed -= DoTouchpadPressed;
		controllerEvents.TouchpadReleased -= DoTouchpadReleased;
		controllerEvents.TriggerClicked -= DoTriggerClicked;

		pointer.DestinationMarkerEnter -= DestinationMarkerEnter;
		pointer.DestinationMarkerHover -= DestinationMarkerHover;
		pointer.DestinationMarkerExit -= DestinationMarkerExit;
		pointer.DestinationMarkerSet -= DestinationMarkerSet;
	}

	// Movement
	private void DoTouchpadTwoAxisChanged(object sender, ControllerInteractionEventArgs e) {
		if (Mathf.Abs(e.touchpadTwoAxis.x) > 4f * Mathf.Abs(e.touchpadTwoAxis.y) && Mathf.Abs(e.touchpadTwoAxis.x) > 0.5f) {
			moveUD = 0;
			if (rotationAllowed) {
				rotationAllowed = false;
				StartCoroutine(Rotate(Mathf.Sign(e.touchpadTwoAxis.x)));
			}
		} else if (Mathf.Abs(e.touchpadTwoAxis.y) > 4f * Mathf.Abs(e.touchpadTwoAxis.x)) {
			moveUD = e.touchpadTwoAxis.y;
		} else {
			moveUD = 0;
		}
	}

	IEnumerator Rotate(float dir) {
		float ang = 0;
		float speed = 5;

		while (ang < 30) {
			ang += speed;
			headParent.Rotate(headParent.up, dir * speed);
			yield return null;
		}

		yield return new WaitForSeconds(0.2f);
		rotationAllowed = true;
	}

	private void DoTouchpadPressed(object sender, ControllerInteractionEventArgs e) {
		if (dragState == DragState.hovering) {
			dragState = DragState.caught;
		}

		if (OnTouchpadPressed != null) {
			OnTouchpadPressed();
		}
	}

	private void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e) {

		if (dragState == DragState.dragging) {
			Transform controllerTransform = e.controllerReference.actual.transform;
			draggingPointerDelegate.HoverDragEnd();
			draggingPointerDelegate = null;
		}

		dragState = DragState.idle;
	}

	private void DoTriggerClicked(object sender, ControllerInteractionEventArgs e) {
		if (OnTriggerClicked != null) {
			OnTriggerClicked();
		}
	}

	// Pointer
	protected virtual void DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e) {
		PointerDelegates pf = e.target.GetComponent<PointerDelegates>();
		if (pf != null) {
			pf.Enter();
		}
	}

	protected virtual void DestinationMarkerHover(object sender, DestinationMarkerEventArgs e) {
		PointerDelegates pf = e.target.GetComponent<PointerDelegates>();
		if (pf != null) {
			pf.Hover();
			if (dragState == DragState.idle) {
				dragState = DragState.hovering;

			}
		}
	}

	protected virtual void DestinationMarkerExit(object sender, DestinationMarkerEventArgs e) {
		PointerDelegates pf = e.target.GetComponent<PointerDelegates>();
		if (pf != null) {
			pf.Exit();

			if (dragState == DragState.caught) {
				dragState = DragState.dragging;
				draggingPointerDelegate = pf;
				pf.HoverDragStart(e.destinationPosition);
			}
		}
	}

	protected virtual void DestinationMarkerSet(object sender, DestinationMarkerEventArgs e) {
		PointerDelegates pf = e.target.GetComponent<PointerDelegates>();
		if (pf != null) {
			pf.Set();
		}
	}
}