using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using SharpConfig;
using UnityEngine;
using VRTK;

public class PatentPointer : MonoBehaviour {

	public DraggerControl dragger;
	public GameObject draggedObject;

	private PointerDelegates pd;
	public static GameObject selectedPatent;
	private DraggerControl draggerObject;

	private void Awake() {
		pd = GetComponent<PointerDelegates>();
	}


	private void OnEnable() {
		pd.OnEnter += OnPointerEnter;
		pd.OnExit += OnPointerExit;
		pd.OnSet += OnPointerSet;
		pd.OnHoverDragStart += OnDragging;
		pd.OnHoverDragEnd += OnReleasing;
	}

	private void OnDisable() {
		pd.OnEnter -= OnPointerEnter;
		pd.OnExit -= OnPointerExit;
		pd.OnSet -= OnPointerSet;
		pd.OnHoverDragStart -= OnDragging;
		pd.OnHoverDragEnd -= OnReleasing;
	}

	private void OnPointerEnter() {
		Patent pat = PatentManager.GameObjectToPatent[gameObject];
		UpdatePatentInfo(pat);
	}

	private void OnPointerExit() {
		if (selectedPatent != null) {
			Patent pat = PatentManager.GameObjectToPatent[selectedPatent];
			UpdatePatentInfo(pat);
		} else {
			UpdatePatentInfo();
		}
	}

	private void OnPointerSet() {
		Patent pat = PatentManager.GameObjectToPatent[gameObject];
		if (selectedPatent != gameObject) {

			if (selectedPatent != null) {
				PatentManager.GameObjectToPatent[selectedPatent].ShowCitationArrows = false;
			}

			selectedPatent = gameObject;

			PatentManager.Patents.ForEach(p => p.highlight = Patent.HighlightLevel.Faded);

			pat.ShowCitationArrows = true;
			pat.highlight = Patent.HighlightLevel.Highlighted;

			UpdatePatentInfo(pat);
		} else {
			PatentManager.Patents.ForEach(p => p.highlight = Patent.HighlightLevel.Default);
			pat.ShowCitationArrows = false;
			UpdatePatentInfo();
			selectedPatent = null;
		}
	}

	private void UpdatePatentInfo(Patent pat = null) {
		if (pat == null) {
			SetPatentInformation.instance.Set("--- Select patent for more information ---");
		} else {
			SetPatentInformation.instance.Set(pat.patentID, pat.fields["title"],
												pat.fields["assignee"], pat.fields["grantDate"], pat.fields["abstract"]);
		}
	}

	private void OnDragging(Vector3 startPosition) {
		draggerObject = Instantiate(dragger);
		draggerObject.transform.position = startPosition;
	}

	private void OnReleasing() {
		GameObject patentPopUp = draggerObject.EndDrag(draggedObject);
		Patent pat = PatentManager.GameObjectToPatent[gameObject];
		patentPopUp.GetComponentInChildren<SetPatentInformation>().Set(pat.patentID, pat.fields["title"],
												pat.fields["assignee"], pat.fields["grantDate"], pat.fields["abstract"]);
		patentPopUp.GetComponentInChildren<TextMesh>().text = pat.patentID;
	}
}