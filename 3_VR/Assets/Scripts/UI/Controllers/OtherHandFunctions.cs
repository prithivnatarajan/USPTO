using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class OtherHandFunctions : MonoBehaviour {

	/*
	 * This script handles the following:
	 * 1) Movement in the direction of sight - Controlled using touchpadTwo
	 * 2) Settings panel - Activated by pressing the touchpad
	 * 
	 */

	public VRTK_ControllerEvents controllerEvents;
	public GameObject spaceSettings;
	[Range(0.3f, 1f)] public float movementSpeed = 0.5f;
	[Range(0.05f, 0.3f)] public float panelRotationSpeed = 0.1f;

	private bool rotatingPanel;
	private float speed;
	private float deltaPanelAngle;
	private Vector2 touchpadDir;
	private Vector2 previousTouchpadAxis;

	private void Update() {
		// Movement control
		if (speed > 0.2f) {
			Transform head = VRTK_DeviceFinder.HeadsetCamera();
			Vector3 moveDir = head.forward * touchpadDir.y + head.right * touchpadDir.x;
			VRTK_SDKManager.instance.transform.position += moveDir * speed * 0.07f * movementSpeed;
		}

		// Panel control
		if (spaceSettings.activeSelf) {
			spaceSettings.transform.position = controllerEvents.transform.position;
			spaceSettings.transform.rotation = controllerEvents.transform.rotation;
			spaceSettings.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, deltaPanelAngle);
		}
	}

	private void OnEnable() {
		controllerEvents.TouchpadTwoAxisChanged += DoTouchpadTwoAxisChanged;
		controllerEvents.TriggerClicked += DoTriggerClicked;
		controllerEvents.TouchpadTouchStart += DoTouchpadTouchStart;
		controllerEvents.TouchpadAxisChanged += DoTouchpadAxisChanged;
		controllerEvents.GripPressed += DoGripPressed;
	}

	private void OnDisable() {
		if (controllerEvents != null) {
			controllerEvents.TouchpadTwoAxisChanged -= DoTouchpadTwoAxisChanged;
			controllerEvents.TriggerClicked -= DoTriggerClicked;
			controllerEvents.TouchpadTouchStart -= DoTouchpadTouchStart;
			controllerEvents.TouchpadAxisChanged -= DoTouchpadAxisChanged;
			controllerEvents.GripPressed -= DoGripPressed;
		}
	}

	// Movement
	private void DoTouchpadTwoAxisChanged(object sender, ControllerInteractionEventArgs e) {
		touchpadDir = e.touchpadTwoAxis;
		speed = e.touchpadTwoAxis.magnitude;
	}

	private void DoTriggerClicked(object sender, ControllerInteractionEventArgs e) {
		spaceSettings.SetActive(!spaceSettings.activeSelf);
		spaceSettings.transform.position = controllerEvents.transform.position;
		spaceSettings.transform.rotation = controllerEvents.transform.rotation;
	}

	private void DoTouchpadTouchStart(object sender, ControllerInteractionEventArgs e) {
		previousTouchpadAxis = e.touchpadAxis;
	}

	private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e) {
		Vector2 dir = previousTouchpadAxis - e.touchpadAxis;
		if (dir.magnitude > 0.2f && !rotatingPanel) {
			StartCoroutine(RotateSettingsPanel(dir.x));
		}

		previousTouchpadAxis = e.touchpadAxis;
	}

	private void DoGripPressed(object sender, ControllerInteractionEventArgs e) {
		PatentManager.Instance.ShowEvolution();
	}

	IEnumerator RotateSettingsPanel(float dir) {
		rotatingPanel = true;

		dir = Mathf.Sign(dir);
		float finalAng = deltaPanelAngle + dir * 120f;

		while (Mathf.Abs(finalAng - deltaPanelAngle) > 3f) {
			deltaPanelAngle = Mathf.Lerp(deltaPanelAngle, finalAng, panelRotationSpeed);
			yield return null;
		}

		deltaPanelAngle = finalAng;

		rotatingPanel = false;
	}
}