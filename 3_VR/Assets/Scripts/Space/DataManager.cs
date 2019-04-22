using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using SharpConfig;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;


public class DataManager : MonoBehaviour {

	public List<Color> colorSet;

	public static bool ShowBottomText;
	public static bool ShowTopText;

	private List<Patent> patents;
	private List<Color> colors;
	public static Color defaultColor;

	private string[] columns;
	private List<Patent> _focalPatents;
	private Dictionary<Patent, float> _patentDistanceToNearestFocalPatent = new Dictionary<Patent, float>();
	private float _furthestDistanceToFocalPatent = float.MinValue;

	// column name, value => color
	public static Dictionary<string, Dictionary<string, Color>> columnValueToColor;
	public static Dictionary<string, int> valueCount;

	// Use this for initialization
	private void Start() {
		// load settings
		var config = Configuration.LoadFromFile("Assets/Scripts/DataManagement/config.ini");
		var section = config["UserInterface"];

		defaultColor = colorSet[colorSet.Count - 1];
		colors = colorSet;
		colors.RemoveAt(colors.Count - 1);

		valueCount = new Dictionary<string, int>();

		columns = section["GroupByColumns"].StringValueArray;

		patents = PatentManager.Patents;

		var focalPatentIdSet = new HashSet<string>(section["FocalPatents"].StringValueArray);
		_focalPatents = patents.Where(p => focalPatentIdSet.Contains(p.patentID)).ToList();
		if (_focalPatents.Count == 0) {
			Debug.LogWarning("No focal patents found!");
		}

		foreach (var p in patents) {
			var minDistance = float.MaxValue;
			foreach (var focalPatent in _focalPatents) {
				var distance = (focalPatent.AssociatedGameObject.transform.position -
								p.AssociatedGameObject.transform.position).magnitude;
				if (distance < minDistance)
					minDistance = distance;
			}

			_patentDistanceToNearestFocalPatent.Add(p, minDistance);
			if (minDistance > _furthestDistanceToFocalPatent)
				_furthestDistanceToFocalPatent = minDistance;
		}

		columnValueToColor = new Dictionary<string, Dictionary<string, Color>>();
		columns.ForEach(columnName => columnValueToColor.Add(columnName, ColorCodeValues(columnName)));

		string column = "examiners";
		foreach (Patent p in PatentManager.Patents) {
			Color c;
			var gotColor = columnValueToColor[column].TryGetValue(p.fields[column], out c);
			p.Color = gotColor ? c : defaultColor;
		}
	}

	private Dictionary<string, Color> ColorCodeValues(string column) {
		Dictionary<string, Color> d = new Dictionary<string, Color>();
		List<string> values = new List<string>();
		List<int> counts = new List<int>();

		foreach (Patent p in PatentManager.Patents) {
			if (!values.Contains(p.fields[column])) {
				values.Add(p.fields[column]);
				counts.Add(1);
			} else {
				int pos = values.IndexOf(p.fields[column]);
				if (counts[pos] == 0) print("zero");
				counts[pos] += 1;
				if (pos > 0) {
					while (counts[pos] > counts[pos - 1]) {
						string s = values[pos];
						values[pos] = values[pos - 1];
						values[pos - 1] = s;
						int j = counts[pos];
						counts[pos] = counts[pos - 1];
						counts[pos - 1] = j;
						pos--;
						if (pos == 0) break;
					}
				}
			}
		}

		int i = 0;
		foreach (Color c in colors) {
			if (i < values.Count) {
				valueCount.Add(values[i], counts[i]);
				d.Add(values[i++], c);
			} else {
				break;
			}
		}

		return d;
	}

	private static Color HexStringToColor(string hex) {
		hex = hex.Replace("#", "");
		float[] colorArray =
		{
			int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
			int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
			int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f
		};
		return new Color(colorArray[0], colorArray[1], colorArray[2]);
	}

}