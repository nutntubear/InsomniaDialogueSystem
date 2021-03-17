using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class DestinationObject : MonoBehaviour
{

	public Text destinationNode;
	public Dropdown memoryType;
	public InputField memoryName;
	public InputField memoryValue;
	public Dropdown memoryValueBoolean;
	public Dropdown check;
	public Toggle forceDestination;

	static List<Dropdown.OptionData> allChecks = new List<Dropdown.OptionData>();
	static List<Dropdown.OptionData> equalsOnly = new List<Dropdown.OptionData>();

	string[] checks = new string[] { "=", ">", "<", ">=", "<=" };

	[HideInInspector]
	public Destination currentDest = new Destination();
	int destID;

	public void SwitchType (Dropdown drop) {
		SwitchByInt(drop.value);
	}

	// Used to update the currentDest variable if it is a MemoryDestination.
	public void UpdateMemory () {
		string destType = currentDest.GetTemplatedType();
		if (allChecks.Count == 0) {
			Start();
		}
		if (destType == "NONE") return;
		if (destType == "Int32") {
			int attempt = 0;
			Int32.TryParse(memoryValue.text, out attempt);
			currentDest = new MemoryDestination<int>(destID, currentDest.id, memoryName.text, attempt,
				allChecks[check.value].text, forceDestination.isOn);
		} else if (destType == "String") {
			currentDest = new MemoryDestination<string>(destID, currentDest.id, memoryName.text, memoryValue.text,
				allChecks[check.value].text, forceDestination.isOn);
		} else if (destType == "Boolean") {
			currentDest = new MemoryDestination<bool>(destID, currentDest.id, memoryName.text, memoryValueBoolean.value == 0,
				allChecks[check.value].text, forceDestination.isOn);
		}
	}

	// Switch the UI based on the type of destination given:
	// 		0 = Base
	//		1 = Int
	//		2 = String
	//		3 = Bool
	void SwitchByInt (int val) {
		memoryValue.gameObject.SetActive(true);
		memoryValueBoolean.gameObject.SetActive(false);
		check.value = 0;
		if (val == 0) {
			currentDest = new Destination(currentDest.dest, currentDest.id);
			memoryName.text = "";
			memoryName.interactable = false;
			memoryValue.interactable = false;
			check.interactable = false;
			forceDestination.interactable = false;
		} else {
			// UI Settings for all memory destinations:
			memoryName.interactable = true;
			memoryValue.interactable = true;
			check.interactable = true;
			forceDestination.interactable = true;
			// UI Settings for specific types of memory destinations:
			if (val == 1) {
				currentDest = new MemoryDestination<int>(destID, currentDest.id);
				check.options = allChecks;
				memoryValue.contentType = InputField.ContentType.IntegerNumber;
				return;
			} else if (val == 2) {
				currentDest = new MemoryDestination<string>(destID, currentDest.id);
				memoryValue.contentType = InputField.ContentType.Alphanumeric;
			} else if (val == 3) {
				currentDest = new MemoryDestination<bool>(destID, currentDest.id);
				memoryValue.gameObject.SetActive(false);
				memoryValueBoolean.gameObject.SetActive(true);
			}
			check.options = equalsOnly;
		}
	}

	// Setup the UI and currentDest destination; to be used when a DestinationObject is created.
	public void Setup (Destination dest) {
		currentDest = dest;
		destID = dest.dest;
		string destinationType = dest.GetTemplatedType();
		if (destinationType == "Int32") {
			memoryType.value = 1;
			MemoryDestination<int> temp = new MemoryDestination<int>(dest);
			memoryName.text = temp.memoryKey;
			memoryValue.text = temp.value.ToString();
			forceDestination.isOn = temp.forced;
			check.value = Mathf.Max(System.Array.IndexOf(checks, temp.checkCode), 0);
		} else if (destinationType == "String") {
			MemoryDestination<string> temp = new MemoryDestination<string>(dest);
			memoryType.value = 2;
			memoryName.text = temp.memoryKey;
			memoryValue.text = temp.value;
			forceDestination.isOn = temp.forced;
			check.value = Mathf.Max(System.Array.IndexOf(checks, temp.checkCode), 0);
		} else if (destinationType == "Boolean") {
			MemoryDestination<bool> temp = new MemoryDestination<bool>(dest);
			memoryType.value = 3;
			memoryName.text = temp.memoryKey;
			if (temp.value) {
				memoryValueBoolean.value = 0;
			} else {
				memoryValueBoolean.value = 1;
			}
			forceDestination.isOn = temp.forced;
			check.value = Mathf.Max(System.Array.IndexOf(checks, temp.checkCode), 0);
		} else {
			SwitchByInt(0);
		}
		destinationNode.text = "→" + dest.dest.ToString();
	}

	void Start () {
		// If this is the first DestinationObject, set up the check lists.
		if (allChecks.Count == 0) {
			allChecks.Add(new Dropdown.OptionData("="));
			allChecks.Add(new Dropdown.OptionData(">"));
			allChecks.Add(new Dropdown.OptionData("<"));
			allChecks.Add(new Dropdown.OptionData(">="));
			allChecks.Add(new Dropdown.OptionData("<="));
			equalsOnly.Add(new Dropdown.OptionData("="));
		}
	}

}
