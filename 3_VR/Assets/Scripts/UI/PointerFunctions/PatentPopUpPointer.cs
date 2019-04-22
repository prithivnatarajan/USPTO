using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatentPopUpPointer : MonoBehaviour {

	private PointerDelegates pd;
	private DragAround da;
	private bool hovering;

	void Awake() {
		pd = GetComponent<PointerDelegates>();
		da = transform.parent.GetComponent<DragAround>();
	}

	private void OnEnable() {
		pd.OnSet += OnPointerSet;
		pd.OnHover += OnPointerHover;
		pd.OnExit += OnPointerExit;
		DominantHandFunctions.OnTriggerClicked += DeletePopUp;
		DominantHandFunctions.OnTouchpadPressed += StopDragging;
	}

	private void OnDisable() {
		pd.OnSet -= OnPointerSet;
		pd.OnHover -= OnPointerHover;
		pd.OnExit -= OnPointerExit;
		DominantHandFunctions.OnTriggerClicked -= DeletePopUp;
		DominantHandFunctions.OnTouchpadPressed -= StopDragging;
	}

	private void OnPointerSet() {
		if (!da.enabled) {
			//da.enabled = true;
		}
	}

	private void OnPointerHover() {
		hovering = true;
	}

	private void OnPointerExit() {
		hovering = false;
	}

	private void StopDragging() {
		if (da.enabled) {
			//da.enabled = false;
		}
	}

	private void DeletePopUp() {
		if (hovering && !da.enabled) {
			Destroy(transform.parent.gameObject);
		}
	}
}
