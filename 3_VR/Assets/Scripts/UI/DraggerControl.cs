using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class DraggerControl : MonoBehaviour {

	public GameObject EndDrag(GameObject draggedObject) {
		if (draggedObject == null) {
			Debug.LogWarning("Dragged object not set");
			return null;
		}

		GameObject g = Instantiate(draggedObject);
		g.transform.position = transform.GetChild(0).position;

		Destroy(gameObject);

		return g;
	}
}
