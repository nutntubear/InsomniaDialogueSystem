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

	[HideInInspector]
	public Destination currentDest = new Destination();

	public void SwitchType (Dropdown drop) {
		SwitchByInt(drop.value);
	}

	// Used to update the currentDest variable if it is a MemoryDestination.
	public void UpdateMemory () {
		string destType = Utilities.GetDestinationType(currentDest);
		if (destType == "base") return;
		((MemoryDestination)currentDest).memoryKey = memoryName.text;
		((MemoryDestination)currentDest).checkCode = allChecks[check.value].text;
		((MemoryDestination)currentDest).forced = forceDestination.isOn;

		if (destType == "int") {
			int attempt = 0;
			Int32.TryParse(memoryValue.text, out attempt);
			((MemoryDestinationInt)currentDest).value = attempt;
		} else if (destType == "string") {
			((MemoryDestinationString)currentDest).value = memoryValue.text;
		} else if (destType == "bool") {
			((MemoryDestinationBool)currentDest).value = (memoryValueBoolean.value == 0);
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
		if (val == 0) {
			currentDest = new Destination(currentDest.dest);
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
				currentDest = new MemoryDestinationInt(currentDest);
				check.options = allChecks;
				memoryValue.contentType = InputField.ContentType.IntegerNumber;
				return;
			} else if (val == 2) {
				currentDest = new MemoryDestinationString(currentDest);
				memoryValue.contentType = InputField.ContentType.Alphanumeric;
			} else if (val == 3) {
				currentDest = new MemoryDestinationBool(currentDest);
				memoryValue.gameObject.SetActive(false);
				memoryValueBoolean.gameObject.SetActive(true);
			}
			check.options = equalsOnly;
		}
	}

	// Setup the UI and currentDest destination; to be used when a DestinationObject is created.
	public void Setup (Destination dest) {
		currentDest = dest;
		Type destinationType = dest.GetType();
		if (destinationType == Utilities.memoryDestinationInt.GetType()) {
			memoryType.value = 1;
			memoryName.text = ((MemoryDestinationInt)dest).memoryKey;
			memoryValue.text = ((MemoryDestinationInt)dest).value.ToString();
			forceDestination.isOn = ((MemoryDestination)dest).forced;
		} else if (destinationType == Utilities.memoryDestinationString.GetType()) {
			memoryType.value = 2;
			memoryName.text = ((MemoryDestinationString)dest).memoryKey;
			memoryValue.text = ((MemoryDestinationString)dest).value;
			forceDestination.isOn = ((MemoryDestination)dest).forced;
		} else if (destinationType == Utilities.memoryDestinationBool.GetType()) {
			memoryType.value = 3;
			memoryName.text = ((MemoryDestinationBool)dest).memoryKey;
			if (((MemoryDestinationBool)dest).value) {
				memoryValueBoolean.value = 0;
			} else {
				memoryValueBoolean.value = 1;
			}
			forceDestination.isOn = ((MemoryDestination)dest).forced;
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
