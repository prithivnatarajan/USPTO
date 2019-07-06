using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PageTurnPointer: MonoBehaviour {

	public bool forward;

	private PointerDelegates pd;
	private Vector3 actualSize;
	private TextMeshPro txt;

	void Awake() {
		pd = GetComponent<PointerDelegates>();
		actualSize = transform.localScale;
		txt = transform.parent.GetComponent<TextMeshPro>();
	}

	private void OnEnable() {
		pd.OnEnter += OnPointerEnter;
		pd.OnExit += OnPointerExit;
		pd.OnSet += OnPointerSet;
	}

	private void OnDisable() {
		pd.OnEnter -= OnPointerEnter;
		pd.OnExit -= OnPointerExit;
		pd.OnSet -= OnPointerSet;

		transform.localScale = actualSize;
	}

	private void OnPointerEnter() {
		transform.localScale += Vector3.one * 0.02f;
	}

	private void OnPointerExit() {
		transform.localScale = actualSize;
	}

	private void OnPointerSet() {
		transform.localScale = actualSize;

		if (forward) {
			txt.pageToDisplay = Mathf.Min(txt.pageToDisplay + 1, txt.textInfo.pageCount);
		} else {
			txt.pageToDisplay = Mathf.Max(txt.pageToDisplay - 1, 0);
		}
	}
}
