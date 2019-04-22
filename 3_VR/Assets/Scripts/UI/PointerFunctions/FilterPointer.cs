using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FilterPointer : MonoBehaviour {

	private PointerDelegates pd;
	private Vector3 actualSize;
	private Color originalColor;
	private bool isFilterOn;

	void Awake() {
		pd = GetComponent<PointerDelegates>();
		actualSize = transform.localScale;
	}

	private void Start() {
		isFilterOn = true;
		originalColor = GetComponentInChildren<SpriteRenderer>().color;
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
		transform.localScale += Vector3.one * 0.05f;
	}

	private void OnPointerExit() {
		transform.localScale = actualSize;
	}

	private void OnPointerSet() {
		transform.localScale = actualSize;

		if (isFilterOn) {
			GetComponent<TextMeshPro>().color = Color.white * 0.4f;
			GetComponentInChildren<SpriteRenderer>().color = Color.white * 0.4f;
		} else {
			GetComponent<TextMeshPro>().color = Color.white;
			GetComponentInChildren<SpriteRenderer>().color = originalColor;
		}

		isFilterOn = !isFilterOn;

		string column = transform.parent.name;
		string value = name;

		if (value != "Others") {
			foreach (Patent p in PatentManager.Patents) {
				if (p.fields[column] == value) {
					p.Hidden = !isFilterOn;
				}
			}
		} else {

			int n = transform.parent.childCount;
			List<string> allValues = new List<string>();
			for (int i = 0; i < n - 1; i++) { //Excluding "Others"
				allValues.Add(transform.parent.GetChild(i).name);
			}

			foreach (Patent p in PatentManager.Patents) {
				if (!allValues.Contains(p.fields[column])) {
					p.Hidden = !isFilterOn;
				}
			}
		}
	}
}
