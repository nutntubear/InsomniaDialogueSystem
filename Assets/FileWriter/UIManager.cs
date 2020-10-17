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

	[Header("Universal UI")]
	public GameObject pauseScreen;
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
		} else {
			mode = "addDest";
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
	}

	public void DeleteDestination () {

	}

	public void AddMemory () {

	}

	public void DeleteMemory () {

	}

	public void AddEvent () {

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
			int i = 0;
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
			for (i = 0; i < node.destinations.Count; ++i) {
				AddDestinationObject(node.destinations[i], i);
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

	bool IsTyping () {
		for (int i = 0; i < fields.Length; ++i) {
			if (fields[i].isFocused) return true;
		}
		return false;
	}

	void Start () {
		SwitchTab(0);
		fields = Resources.FindObjectsOfTypeAll<InputField>();
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
					AddDestinationObject(new Destination(dest), nodes.nodes[showing].node.destinations.Count);
				}
			} else if (mode == "") {
				nodes.UpdateDestinations(showing);
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
