using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetPatentInformation : MonoBehaviour {

	private static SetPatentInformation controllerInstance;

	public static SetPatentInformation instance {
		get {
			if (controllerInstance == null) {
				Debug.LogWarning("Instance of SetPatentInfomartion Not Created. Attach script to a gameobject");
				return null;
			} else {
				return controllerInstance;
			}
		}
		set { }
	}

	private void Awake() {
		if (controllerInstance == null) {
			controllerInstance = gameObject.GetComponentInChildren<SetPatentInformation>();
		}
	}

	public void Set(string id = "", string title = "", string assignee = "", string grantDate = "", string abs = "") {
		string info = "";

		if (title != "") {
			info += "<color=#FFFFFF><b>ID:</b><color=#9CB0C7>\n" + id;
			info += "\n\n<color=#FFFFFF><b>Title:</b><color=#9CB0C7>\n" + title;
			info += "\n\n<color=#FFFFFF><b>Assignee:</b><color=#9CB0C7>\n" + assignee;
			info += "\n\n<color=#FFFFFF><b>Grant Date:</b><color=#9CB0C7>\n" + grantDate;
			info += "\n\n<color=#FFFFFF><b>Abstract:</b><color=#9CB0C7>\n" + abs;
		} else {
			info += "<color=#9CB0C7>" + id; // In case no patent is selected. Only the first argument is shown i.e. Select patent for more information.
		}

		GetComponentInChildren<TextMeshPro>().SetText(info);
	}
}
