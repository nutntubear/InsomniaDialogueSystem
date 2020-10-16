using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class UIManager : MonoBehaviour
{

	[System.NonSerialized]
	public bool paused = false;
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

	[Header("Other Settings")]
	public Color enabledButton;
	public Color disabledButton;

	[Header("UI Lists")]
	public List<Image> buttons;
	InputField[] fields;

	[Header("UI Objects")]
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

	public void SwitchNode () {

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
				}
			} else if (mode == "addDest") {
				if (nodes.AddDestination(mousePos)) {
					mode = "";
					SetAvailableButtons();
				}
			} else if (mode == "") {
				int selected = nodes.SelectNode(mousePos);
				if (showing != selected) {
					if (selected == -1) {
						nodeGeneral.SetActive(false);
						nodeSettings.SetActive(false);
					} else {
						nodeGeneral.SetActive(true);
						nodeSettings.SetActive(true);
						Node node = nodes.nodes[selected].node;
						nodeID.text = node.id.ToString();
					}
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
