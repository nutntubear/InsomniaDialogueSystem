using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

	[System.NonSerialized]
	public bool paused = false;
	string mode = "";

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

	public void NewFile () {

	}

	public void AddNode () {

	}

	public void DeleteNode () {

	}

	public void AddDestination () {

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
	}

}
