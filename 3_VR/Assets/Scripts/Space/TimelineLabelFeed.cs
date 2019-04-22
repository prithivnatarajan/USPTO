using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SharpConfig;
using TMPro;
using UnityEngine;

public class TimelineLabelFeed : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
		var timelineSection = Configuration.LoadFromFile("Assets/Scripts/DataManagement/config.ini")["PatentInfoGenerator"];

		if (!timelineSection["TimelineEnabled"].BoolValue) {
			enabled = false;
			return;
		}

		var spread = timelineSection["TimelineSpread"].FloatValue;
		List<string> timelineLabels;
		List<float> timelineLabelPositions;

		using (var r = new StreamReader(timelineSection["TimelineLabelsLocation"].StringValue)) {
			var json = r.ReadToEnd();
			var timelineLabelDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
			timelineLabels = timelineLabelDict["labels"];
			timelineLabelPositions = timelineLabelDict["positions"].Select(float.Parse).ToList();
		}

		for (int i = 0; i < timelineLabels.Count; i++) {
			var label = timelineLabels[i];
			var position = timelineLabelPositions[i];
			var labelTextMesh = Instantiate(Resources.Load<TextMeshPro>("GraphLabelPrefab"), transform);
			labelTextMesh.transform.localPosition = new Vector3(0, 0, position);
			labelTextMesh.text = label;
		}
	}

    // Update is called once per frame
    void Update()
    {

    }
}