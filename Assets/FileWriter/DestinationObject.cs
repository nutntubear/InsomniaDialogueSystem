using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class DestinationObject : MonoBehaviour
{

	public Text destinationNode;
	public InputField memoryName;
	public InputField memoryValue;
	public Dropdown memoryValueBoolean;
	public Dropdown check;
	public Toggle forceDestination;

	List<Dropdown.OptionData> allChecks = new List<Dropdown.OptionData>();
	List<Dropdown.OptionData> equalsOnly = new List<Dropdown.OptionData>();

	[HideInInspector]
	public Destination currentDest = new Destination();

	public void SwitchType (Dropdown drop) {
		SwitchByInt(drop.value);
	}

	public void UpdateMemory () {
		((MemoryDestination)currentDest).memoryKey = memoryName.text;
		((MemoryDestination)currentDest).checkCode = allChecks[check.value].text;
		((MemoryDestination)currentDest).forced = forceDestination.isOn;
		if (currentDest.GetType() == Utilities.memoryDestinationInt.GetType()) {
			int attempt = 0;
			Int32.TryParse(memoryValue.text, out attempt);
			((MemoryDestinationInt)currentDest).value = attempt;
		} else if (currentDest.GetType() == Utilities.memoryDestinationString.GetType()) {
			((MemoryDestinationString)currentDest).value = memoryValue.text;
		} else {
			((MemoryDestinationBool)currentDest).value = (memoryValueBoolean.value == 0);
		}
	}

	void SwitchByInt (int val) {
		memoryValue.gameObject.SetActive(true);
		memoryValueBoolean.gameObject.SetActive(false);
		if (val == 0) {
			currentDest = (Destination)currentDest;
			memoryName.text = "";
			memoryName.interactable = false;
			memoryValue.interactable = false;
			check.interactable = false;
			forceDestination.interactable = false;
		} else {
			memoryName.interactable = true;
			memoryValue.interactable = true;
			check.interactable = true;
			forceDestination.interactable = true;
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

	public void Setup (Destination dest) {
		currentDest = dest;
		Type destinationType = dest.GetType();
		if (destinationType == Utilities.memoryDestinationInt.GetType()) {
			SwitchByInt(1);
			memoryName.text = ((MemoryDestinationInt)dest).memoryKey;
			memoryValue.text = ((MemoryDestinationInt)dest).value.ToString();
		} else if (destinationType == Utilities.memoryDestinationString.GetType()) {
			SwitchByInt(2);
			memoryName.text = ((MemoryDestinationString)dest).memoryKey;
			memoryValue.text = ((MemoryDestinationString)dest).value;
		} else if (destinationType == Utilities.memoryDestinationBool.GetType()) {
			SwitchByInt(3);
			memoryName.text = ((MemoryDestinationBool)dest).memoryKey;
			if (((MemoryDestinationBool)dest).value) {
				memoryValueBoolean.value = 0;
			} else {
				memoryValueBoolean.value = 1;
			}
		} else {
			SwitchByInt(0);
		}
		destinationNode.text = "→" + dest.dest.ToString();
	}

	void Start () {
		allChecks.Add(new Dropdown.OptionData("="));
		allChecks.Add(new Dropdown.OptionData(">"));
		allChecks.Add(new Dropdown.OptionData("<"));
		allChecks.Add(new Dropdown.OptionData(">="));
		allChecks.Add(new Dropdown.OptionData("<="));
		equalsOnly.Add(new Dropdown.OptionData("="));
	}

}
