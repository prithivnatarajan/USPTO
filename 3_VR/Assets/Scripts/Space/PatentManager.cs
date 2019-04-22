using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using Newtonsoft.Json;
using SharpConfig;
using TMPro;
using Valve.VR.InteractionSystem;
using Object = UnityEngine.Object;
using System.Collections;

/// <summary>
/// Represents a Patent with a location in space and associated GameObject for visualization
/// </summary>
public class Patent {
	private static GameObject patentPrefab;
	private static Transform parentTransform;
	private static GameObject arrowPrefab;

	public readonly SortedList<DateTime, int> allCitations;
	public readonly List<string> citedBy;

	public readonly Dictionary<string, string> fields;
	public readonly DateTime dateGranted;
	public readonly string patentID;
	public readonly float X;
	public readonly float Y;
	public readonly float Z;
	public readonly int rankNumber;
	public GameObject AssociatedGameObject;

	private Color _orbColor;
	private float _desiredSize;
	private bool _hidden;
	private float _alpha;
	private HighlightLevel _highlightLevel;
	private bool showCitationArrows = false;
	private List<GameObject> citationArrows;


	public Renderer OrbRenderer { get; private set; }
	public Renderer RingRenderer { get; private set; }
	private DateTime? _firstCitation;

	/// <summary>
	/// Initializes the patent from a Json file. AddPrefab must be called after to initialize the GameObject.
	/// </summary>
	/// <param name="grantDate"></param>
	/// <param name="x">x location in 3d space</param>
	/// <param name="y">y location in 3d space</param>
	/// <param name="z">z location in 3d space</param>
	/// <param name="patentId"></param>
	/// <param name="rank">Rank of patent relative to focal patents (lower is more similar)</param>
	/// <param name="citedByFiltered">List of times this patent has been cited by other patents</param>
	/// <param name="textFields">Assorted text fields, such as the title of the patent</param>
	/// <param name="citations">A dictionary of citations. Key=CitationDate, Value=Cumulative citations on that date</param>

	[JsonConstructor]
	public Patent(string grantDate, float x, float y, float z, string patentId, int rank, List<string> citedByFiltered,
		Dictionary<string, string> textFields,
		SortedList<DateTime, int> citations) {
		fields = textFields;
		dateGranted = DateTime.Parse(grantDate);
		patentID = patentId;
		X = x;
		Y = y;
		Z = z;
		rankNumber = rank;
		allCitations = citations;
		citedBy = citedByFiltered;
		if (!textFields.ContainsKey("assignee"))
			textFields.Add("assignee", "");
		AddPrefab();
	}

	public enum HighlightLevel {
		Default,
		Faded,
		Highlighted
	}

	/// <summary>
	/// Associates a prefab with the Patent
	/// </summary>
	private void AddPrefab() {
		if (!patentPrefab)
			patentPrefab = Resources.Load<GameObject>("Patent");
		if (!parentTransform)
			parentTransform = PatentManager.Instance.transform;

		AssociatedGameObject = Object.Instantiate(patentPrefab, parentTransform);
		AssociatedGameObject.name = patentID;
		AssociatedGameObject.transform.localPosition = new Vector3(X, Y, Z);
		AssociatedGameObject.transform.localScale = Vector3.one;

		OrbRenderer = AssociatedGameObject.transform.Find("Sphere").GetComponent<Renderer>();
		if (rankNumber < 0) {
			//OrbRenderer.gameObject.transform.localScale = .85f * Vector3.one;
			OrbRenderer.gameObject.SetActive(false);
			RingRenderer = AssociatedGameObject.transform.Find("Cube").GetComponent<Renderer>();
			RingRenderer.gameObject.SetActive(true);
		}

		TrySetTextureByAssignee();

		_orbColor = OrbRenderer.sharedMaterial.GetColor("_Color");
	}

	private bool TrySetTextureByAssignee() {
		if (PatentManager.AssigneeTextures == null) {
			return false;
		}

		string assignee;
		fields.TryGetValue("assignee", out assignee);
		if (assignee == null) {
			return false;
		}

		Texture2D tex;
		PatentManager.AssigneeTextures.TryGetValue(assignee, out tex);
		if (tex == null)
			return false;

		OrbRenderer.material.SetTexture("_MainTex", tex);
		return true;
	}

	public bool Hidden {
		get { return _hidden; }
		set {
			if (value.Equals(_hidden)) return;
			_hidden = value;
			UpdateSize();
		}
	}

	public HighlightLevel highlight {
		get { return _highlightLevel; }
		set {
			_highlightLevel = value;
			if (value == HighlightLevel.Faded) {
				Alpha = PatentManager.FadedAlpha + (rankNumber == 0 ? 0.05f : 0f);
			} else if (value == HighlightLevel.Default) {
				Alpha = PatentManager.DefaultAlpha;
			} else {
				Alpha = PatentManager.HighlightedAlpha;
			}
		}
	}

	public float Alpha {
		get { return _alpha; }
		set {
			_alpha = value;
			if (Math.Abs(Color.a - value) < 0.001) return;
			//if (RingRenderer != null)
			//	RingRenderer.material.SetColor("_Color",
			//		RingRenderer.material.GetColor("_Color").ColorWithAlpha(value));
			var c = Color;
			c.a = value;
			Color = c;
		}
	}

	public Color Color {
		get { return _orbColor; }
		set {
			if (value.Equals(_orbColor)) return;
			value.a = Alpha; // don't let opacity change without using Opacity
			_orbColor = value;
			//OrbRenderer.material.SetColor("_Color", value);
			OrbRenderer.material.color = _orbColor;
		}
	}

	public float Size {
		get { return _desiredSize; }
		set {
			_desiredSize = value * (float)Math.Log10(citedBy.Count + 10f);
			UpdateSize();
		}
	}

	private void UpdateSize() {
		AssociatedGameObject.transform.localScale =
			Hidden ? new Vector3(0, 0, 0) : new Vector3(_desiredSize, _desiredSize, _desiredSize);
	}

	// TODO: (somehow) make citation arrows update when patents are hidden/faded
	public bool ShowCitationArrows {
		get { return showCitationArrows; }
		set {
			showCitationArrows = value;
			CreateCitationArrows();
		}
	}

	private void CreateCitationArrows() {
		if (citationArrows == null) {
			citationArrows = new List<GameObject>();
		}

		if (showCitationArrows) {
			if (arrowPrefab == null) {
				arrowPrefab = Resources.Load<GameObject>("CitationArrow");
			}

			foreach (Patent p in PatentManager.Patents) {
				//if (allCitations.ContainsValue(p.rankNumber) && !p.Hidden) {
				if (citedBy.Contains(p.patentID) && !p.Hidden) {
					p.highlight = HighlightLevel.Default;

					GameObject arrow = Object.Instantiate(arrowPrefab);
					arrow.transform.position = p.AssociatedGameObject.transform.position;
					arrow.transform.LookAt(AssociatedGameObject.transform);
					arrow.transform.localScale = new Vector3(
						arrow.transform.localScale.x,
						arrow.transform.localScale.y,
						(p.AssociatedGameObject.transform.position - AssociatedGameObject.transform.position).magnitude);

					citationArrows.Add(arrow);
				}
			}
		} else {
			for (int i = citationArrows.Count - 1; i >= 0; i--) {
				GameObject g = citationArrows[i];
				Object.Destroy(g);
				citationArrows.Remove(g);
			}
		}
	}
}



/// <inheritdoc />
/// <summary>
/// Creates patent GameObjects from json file. Do not instantiate more than one! 
/// </summary>
/// TODO: Make non-static except for .Instance to refer to the primary instance
public class PatentManager : MonoBehaviour {

	public static List<Patent> Patents { private set; get; }
	public static Dictionary<GameObject, Patent> GameObjectToPatent { private set; get; }
	public static Dictionary<string, Texture2D> AssigneeTextures { private set; get; }
	public static Dictionary<string, Patent> PatentIdToPatent { private set; get; }

	private static bool evolutionShowing = false;

	// load settings file
	private const string PatentJsonFileLocation = "assets/data/patent.json"; // defined in combine_patent_data.py

	private const string
		ClusterJsonFileLocation = "assets/data/cluster_terms.json"; // defined in combine_patent_data.py

	private const string
		AssigneeToLogoFileLocation = "assets/data/assignee_to_logo_file_name.json"; // defined in get_assignee_logos.py

	private static readonly Configuration Config = Configuration.LoadFromFile("Assets/Scripts/DataManagement/config.ini");
	private static readonly Section PatentConfig = Config["Patents"];
	private static readonly Section PatentGenerationConfig = Config["PatentInfoGenerator"];

	// defined in Start
	private static DateTime _beginningTime;
	private static DateTime _endTime;

	public static bool Paused = PatentConfig["StartPaused"].BoolValue;

	private static float _patentSpread;
	private static float _basePatentSize;

	private static PatentManager _instance;

	public static readonly float FadedAlpha = PatentConfig["FadedAlpha"].FloatValue;
	public static readonly float DefaultAlpha = PatentConfig["DefaultAlpha"].FloatValue;
	public static readonly float HighlightedAlpha = PatentConfig["HighlightedAlpha"].FloatValue;
	public static readonly bool TimelineEnabled = PatentGenerationConfig["TimelineEnabled"].BoolValue;
	public static readonly float TimelineSpread = PatentGenerationConfig["TimelineSpread"].FloatValue;

	private void Awake() {
		using (var r = new StreamReader(PatentJsonFileLocation)) {
			var json = r.ReadToEnd();
			Patents = JsonConvert.DeserializeObject<List<Patent>>(json);
			Patents.ForEach(p => p.Alpha = PatentConfig["DefaultAlpha"].FloatValue);
			Patents.ForEach(p => p.Size = PatentConfig["InitialSize"].FloatValue);
			GameObjectToPatent = new Dictionary<GameObject, Patent>(Patents.Count);
			Patents.ForEach(p => GameObjectToPatent.Add(p.AssociatedGameObject, p));
			PatentIdToPatent = new Dictionary<string, Patent>(Patents.Count);
			Patents.ForEach(p => PatentIdToPatent.Add(p.patentID, p));
		}
	}

	private void Start() {
		if (TimelineEnabled) {
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				PatentConfig["InitialHeight"].FloatValue,
				transform.localPosition.z - TimelineSpread * transform.localScale.z);
		} else {
			transform.localPosition =
				new Vector3(
					transform.localPosition.x,
					PatentConfig["InitialHeight"].FloatValue,
					transform.localPosition.z);
		}

		PatentSize = 0.3f;
	}

	public float PatentSpread {
		get { return _patentSpread; }
		set {
			_patentSpread = value;
			Patents.ForEach(p =>
				p.AssociatedGameObject.transform.localPosition = new Vector3(p.X, p.Y, p.Z) * _patentSpread);
		}
	}

	public float PatentSize {
		get { return _basePatentSize; }
		set {
			_basePatentSize = value;
			Patents.ForEach(p => p.Size = value);
		}
	}

	public static PatentManager Instance {
		get {
			if (_instance == null)
				_instance = GameObject.Find("PatentManager").GetComponent<PatentManager>();
			return _instance;
		}
	}

	public void ShowEvolution() {
		if (!evolutionShowing) {
			StartCoroutine("Evolve");
		}
	}

	IEnumerator Evolve() {
		evolutionShowing = true;

		Patents.Sort((p1, p2) => p1.dateGranted.CompareTo(p2.dateGranted));
		Patents.ForEach(p => p.AssociatedGameObject.SetActive(false));

		DateTime currentTime = DateTime.MinValue;
		currentTime = currentTime.AddYears(2000);

		while (currentTime < DateTime.Today) {
			foreach (Patent p in Patents) {
				if (currentTime > p.dateGranted) {
					p.AssociatedGameObject.SetActive(true);
				}
			}
			currentTime = currentTime.AddMonths(1);
			
			yield return null;
		}

		evolutionShowing = false;
	}

}