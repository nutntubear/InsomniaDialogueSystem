using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class UIManager : MonoBehaviour
{

	[HideInInspector]
	public bool paused = false;
	[HideInInspector]
	public string mode = "";
	int lifted = -1;
	int showing = -1;
	Vector3 liftedDepth = new Vector3(0, 0, -1);

	public NodeManager nodes;

	[Header("Pause Screen UI")]
	public GameObject pauseScreen;

	[Header("Universal UI")]
	public GameObject nodePanel;
	public Button addNode;
	public Button deleteNode;

	[Header("Node General UI")]
	public GameObject nodeGeneral;
	public Text nodeID;
	public InputField speaker;
	public InputField body;

	[Header("Node Property UI")]
	public GameObject nodeSettings;
	public List<GameObject> sections;
	public List<Image> sectionButtons;
	public RectTransform destinationList;
	public RectTransform memoryList;
	public RectTransform eventList;
	public Text destinationGuide;

	[Header("Other Settings")]
	public Color enabledButton;
	public Color disabledButton;

	[Header("UI Lists")]
	public List<Image> buttons;
	InputField[] fields;

	[Header("UI Templates")]
	public GameObject destinationTemplate;
	public GameObject memoryTemplate;
	public GameObject eventTemplate;

	public void SetActiveButton (Image button) {
		SetAvailableButtons();
		if (button != null) {
			button.color = disabledButton;
		}
	}

	public void SetAvailableButtons () {
		for (int i = 0; i < buttons.Count; ++i) {
			buttons[i].color = enabledButton;
		}
	}

	public void NewFile () {
		UnityEngine.SceneManagement.SceneManager.LoadScene("main");
	}

	public void SaveFile () {
		string writeToFile = "";
		for (int i = 0; i < nodes.nodes.Count; ++i) {
			writeToFile += nodes.nodes[i].node.SaveNode() + "\n";
		}
		string path = Application.dataPath + "/TestFiles/";
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
		File.WriteAllText(path + "test3" + ".json", writeToFile);
	}

	public void AddNode () {
		if (mode == "add") {
			mode = "";
			SetAvailableButtons();
		} else {
			mode = "add";
		}
	}

	public void DeleteNode () {
		if (mode == "delete") {
			mode = "";
			SetAvailableButtons();
		} else {
			mode = "delete";
		}
	}

	public void AddDestination () {
		if (mode == "addDest") {
			mode = "";
			SetAvailableButtons();
			destinationGuide.text = "";
		} else {
			mode = "addDest";
			destinationGuide.text = "Click a node to add.";
		}
	}

	void AddDestinationObject (Destination dest, int destinationIndex) {
		if ((destinationIndex + 1) * 120 >= destinationList.sizeDelta.y) {
			destinationList.sizeDelta = new Vector2(0, (destinationIndex + 1) * 120);
			destinationList.anchoredPosition = new Vector2(0, -destinationList.sizeDelta.y / 2);
		}
		Transform newDest = Instantiate(destinationTemplate, destinationList).transform;
		newDest.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60 - 120 * destinationIndex);
		newDest.GetComponent<DestinationObject>().Setup(dest);
		nodes.destinations.Add(newDest.GetComponent<DestinationObject>());
		nodes.destinations[nodes.destinations.Count - 1].currentDest.id = nodes.destinations.Count - 1;
	}

	public void DeleteDestination () {
		if (mode == "deleteDest") {
			mode = "";
			SetAvailableButtons();
			destinationGuide.text = "";
		} else {
			mode = "deleteDest";
			destinationGuide.text = "Click a node to remove.";
		}
	}

	public void AddMemory () {

	}

	public void DeleteMemory () {

	}

	public void AddEvent () {
		mode = "";
		AddEventObject(new DialogueEvent(), nodes.events.Count);
	}

	void AddEventObject (DialogueEvent ev, int eventIndex) {
		if ((eventIndex + 1) * 120 >= eventList.sizeDelta.y) {
			eventList.sizeDelta = new Vector2(0, (eventIndex + 1) * 120);
			eventList.anchoredPosition = new Vector2(0, -eventList.sizeDelta.y / 2);
		}
		Transform newEvent = Instantiate(eventTemplate, eventList).transform;
		newEvent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60 - 120 * eventIndex);
		newEvent.GetComponent<EventObject>().Setup(ev);
		nodes.events.Add(newEvent.GetComponent<EventObject>());
		nodes.events[nodes.events.Count - 1].currentEvent.id = nodes.events.Count - 1;
	}

	public void DeleteEvent () {

	}

	public void UpdateNode () {
		if (showing == -1) return; // This should never happen, but just in case.
		nodes.nodes[showing].node.speaker = speaker.text;
		nodes.nodes[showing].node.body = body.text;
		nodes.nodes[showing].SetText();
	}

	public void SwitchNode (int id) {
		if (id == -1) {
			nodeGeneral.SetActive(false);
			nodeSettings.SetActive(false);
		} else {
			nodeGeneral.SetActive(true);
			nodeSettings.SetActive(true);
			Node node = nodes.nodes[id].node;
			nodeID.text = node.id.ToString();
			speaker.text = node.speaker;
			body.text = node.body;
			// Clear destinations...
			foreach (Transform child in destinationList.transform) {
				Destroy(child.gameObject);
			}
			nodes.destinations = new List<DestinationObject>();
			// ...and add the new ones.
			// Used to track the current progression in each type of destination in a node.
			// Index 0 = base, 1 = int, 2 = string, 3 = bool.
			int[] types = new int[] {0, 0, 0, 0};
			for (int i = 0; i < node.destTotal; ++i) {
				if (node.destinations.Count > 0 && node.destinations[types[0]].id == i) {
					AddDestinationObject(node.destinations[types[0]], i);
					types[0]++;
				} else if (node.intDestinations.Count > 0 && node.intDestinations[types[1]].id == i) {
					AddDestinationObject(node.intDestinations[types[1]], i);
					types[1]++;
				} else if (node.stringDestinations.Count > 0 && node.stringDestinations[types[2]].id == i) {
					AddDestinationObject(node.stringDestinations[types[2]], i);
					types[2]++;
				} else if (node.boolDestinations.Count > 0 && node.boolDestinations[types[3]].id == i) {
					AddDestinationObject(node.boolDestinations[types[3]], i);
					types[3]++;
				}
			}
			// Clear events...
			foreach (Transform child in eventList.transform) {
				Destroy(child.gameObject);
			}
			nodes.events = new List<EventObject>();
			// ...and add the new ones.
			types = new int[] {0, 0, 0, 0};
			for (int i = 0; i < node.evTotal; ++i) {
				if (node.events.Count > 0 && node.events[types[0]].id == i) {
					AddEventObject(node.events[types[0]], i);
					types[0]++;
				} else if (node.intEvents.Count > 0 && node.intEvents[types[1]].id == i) {
					AddEventObject(node.intEvents[types[1]], i);
					types[1]++;
				} else if (node.stringEvents.Count > 0 && node.stringEvents[types[2]].id == i) {
					AddEventObject(node.stringEvents[types[2]], i);
					types[2]++;
				} else if (node.boolEvents.Count > 0 && node.boolEvents[types[3]].id == i) {
					AddEventObject(node.boolEvents[types[3]], i);
					types[3]++;
				}
			}
		}
	}

	public void SwitchTab (int id) {
		for (int i = 0; i < 4; ++i) {
			if (i == id) {
				sections[i].SetActive(true);
				sectionButtons[i].color = enabledButton;
			} else {
				sections[i].SetActive(false);
				sectionButtons[i].color = disabledButton;
			}
		}
	}

	public void SetupFields () {
		fields = Resources.FindObjectsOfTypeAll<InputField>();
	}

	bool IsTyping () {
		for (int i = 0; i < fields.Length; ++i) {
			if (fields[i].isFocused) return true;
		}
		return false;
	}

	void Start () {
		SwitchTab(0);
		SetupFields();
	}

	void Update () {
		if (Input.GetKeyDown("space") && !paused && !IsTyping()) {
			nodePanel.SetActive(!nodePanel.activeSelf);
		}
		if (Input.GetKeyDown("escape")) {
			paused = !paused;
			pauseScreen.SetActive(paused);
		}
		if (paused) {
			return;
		}
		// Mouse control:
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetMouseButtonDown(1)) {
			if (mode == "add") {
				if (nodes.CreateNode(mousePos)) {
					mode = "";
					SetAvailableButtons();
				}
			} else if (mode == "delete") {
				if (nodes.DeleteNode(mousePos)) {
					mode = "";
					SetAvailableButtons();
					SwitchNode(-1);
				}
			} else if (mode == "addDest") {
				int dest = nodes.AddDestination(mousePos);
				if (dest != -1 && !nodes.nodes[showing].node.HasDestination(dest)) {
					mode = "";
					SetAvailableButtons();
					AddDestinationObject(new Destination(dest), nodes.destinations.Count);
					destinationGuide.text = "";
				}
			} else if (mode == "deleteDest") {
				int dest = nodes.DeleteDestination(mousePos);
				if (dest != -1) {
					mode = "";
					Destroy(destinationList.transform.GetChild(dest).gameObject);
					for (int i = dest; i < destinationList.transform.childCount; ++i) {
						destinationList.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition +=
							new Vector2(0, 120);
						destinationGuide.text = "";
					}
				}
			} else if (mode == "") {
				nodes.UpdateAll(showing);
				int selected = nodes.SelectNode(mousePos);
				if (showing != selected) {
					SwitchNode(selected);
					showing = selected;
				}
			}
		} else if (Input.GetMouseButtonDown(2)) {
			if (mode == "") {
				lifted = nodes.LiftNode(mousePos);
				if (lifted != -1) {
					mode = "moving";
				}
			} else if (mode == "moving") {
				if (nodes.PlaceNode(mousePos)) {
					mode = "";
					lifted = -1;
				}
			}
		}

		if (mode == "moving") {
			nodes.nodes[lifted].transform.position = (Vector3)mousePos + liftedDepth;
		}
	}

}
