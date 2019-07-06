using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetFilters : MonoBehaviour {

	public GameObject filterObject;
	private static SetFilters instanceObject;
	private GameObject activeFilter;

	public static SetFilters instance {
		get {
			if (instanceObject == null) {
				Debug.LogWarning("Instance of SetPatentInfomartion Not Created. Attach script to a gameobject");
				return null;
			} else {
				return instanceObject;
			}
		}
		set { }
	}

	private void Awake() {
		instanceObject = gameObject.GetComponentInChildren<SetFilters>();
	}

	private void Start() {
		Invoke("Initiate", 0.1f);
	}

	private void Initiate() {
		CreateFilters();
		Enable("assignee");
	}

	private void CreateFilters() {
		GameObject column;
		GameObject value;

		foreach (KeyValuePair<string, Dictionary<string, Color>> entry in DataManager.columnValueToColor) {
			column = Instantiate(new GameObject(), transform);
			column.transform.localPosition = Vector3.up * 0.25f;
			column.name = entry.Key;
			column.SetActive(false);
			int i = 0;
			int sumOfValueCount = 0;

			foreach (KeyValuePair<string, Color> valueEntry in entry.Value) {
				value = Instantiate(filterObject, column.transform);
				int count = DataManager.valueCount[valueEntry.Key];
				value.GetComponent<TextMeshPro>().text = valueEntry.Key + " (" + count + ")";
				sumOfValueCount += count;
				value.GetComponentInChildren<SpriteRenderer>().color = valueEntry.Value;
				value.transform.localPosition = Vector3.down * 0.08f * i++;
				value.name = valueEntry.Key;
			}

			value = Instantiate(filterObject, column.transform);
			value.GetComponent<TextMeshPro>().text = "Others" + " (" + (PatentManager.Patents.Count - sumOfValueCount)  + ")";
			value.GetComponentInChildren<SpriteRenderer>().color = DataManager.defaultColor;
			value.transform.localPosition = Vector3.down * 0.08f * i++;
			value.name = "Others";
		}
	}

	public void Enable(string column) {
		if (activeFilter) {
			activeFilter.SetActive(false);
		}

		activeFilter = transform.Find(column).gameObject;

		activeFilter.SetActive(true);
	}
}
