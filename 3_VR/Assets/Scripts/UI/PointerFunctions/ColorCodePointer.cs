using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCodePointer : MonoBehaviour {

	public static Color defaultColor = Color.white * 170f / 255f;

	private PointerDelegates pd;
	private Vector3 actualSize;

	void Awake() {
		pd = GetComponent<PointerDelegates>();
		actualSize = transform.localScale;
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

		foreach (SpriteRenderer s in transform.parent.GetComponentsInChildren<SpriteRenderer>()) {
			if (s.transform == transform) {
				s.color = Color.white;
				s.GetComponentInChildren<TextMesh>().color = Color.white;
			} else {
				s.color = Color.white * 0.4f;
				s.GetComponentInChildren<TextMesh>().color = Color.white * 0.4f;
			}
		}

		ColorByFilter();
	}

	private void ColorByFilter() {
		string column = transform.name;
		foreach (Patent p in PatentManager.Patents) {
			Color c;
			var gotColor = DataManager.columnValueToColor[column].TryGetValue(p.fields[column], out c);
			p.Color = gotColor ? c : DataManager.defaultColor;
		}

		SetFilters.instance.Enable(column);
	}
}
