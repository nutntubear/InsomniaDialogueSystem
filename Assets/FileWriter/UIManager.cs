using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

	[System.NonSerialized]
	public bool paused = false;
	public string mode = "";
	int lifted = -1;
	Vector3 liftedDepth = new Vector3(0, 0, -1);

	public NodeManager nodes;

	[Header("Universal UI")]
	public GameObject pauseScreen;
	public GameObject nodePanel;
	public Button addNode;
	public Button deleteNode;

	[Header("Node General UI")]
	public Text nodeID;

	[Header("Node Property UI")]
	public List<GameObject> sections;
	public List<Image> sectionButtons;

	[Header("Other Settings")]
	public Color enabledButton;
	public Color disabledButton;

	[Header("UI Lists")]
	public List<Image> buttons;

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

	void Start () {
		SwitchTab(0);
	}

	void Update () {
		if (Input.GetKeyDown("space") && !paused) {
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
				nodes.SelectNode(mousePos);
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
