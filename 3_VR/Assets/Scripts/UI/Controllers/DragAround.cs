using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class DragAround : MonoBehaviour {

	private Transform controller;
	private Vector3 previousPos;

	private void Start() {
		controller = VRTK_DeviceFinder.GetControllerRightHand(true).transform;

		controller.GetComponentInChildren<VRTK_StraightPointerRenderer>().maximumLength = 0;

		Vector3 spherePos = transform.position;
		transform.position = controller.position;
		transform.rotation = controller.rotation;
		transform.GetChild(0).position = spherePos;

		previousPos = controller.position;
	}

	private void Update() {
		transform.rotation = controller.rotation;
		transform.position += controller.position - previousPos;
		previousPos = controller.position;
	}

	private void OnEnable() {
		if (controller)
			controller.GetComponentInChildren<VRTK_StraightPointerRenderer>().maximumLength = 0;
	}

	private void OnDisable() {
		if (controller)
			controller.GetComponentInChildren<VRTK_StraightPointerRenderer>().maximumLength = 100;
	}
}
