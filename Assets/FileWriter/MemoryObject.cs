using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class MemoryObject : MonoBehaviour
{

	public InputField memoryName;
	public Dropdown memoryType;
	public Dropdown memoryOperation;
	public InputField memoryValue;
	public Dropdown memoryValueBoolean;

	public GameObject deleteButton;

	[HideInInspector]
	public MemoryBase currentMemory;

	static List<Dropdown.OptionData> allOperations = new List<Dropdown.OptionData>();
	static List<Dropdown.OptionData> setAddOperations = new List<Dropdown.OptionData>();
	static List<Dropdown.OptionData> setOnlyOperations = new List<Dropdown.OptionData>();

	string[] operations = new string[] { "set", "+", "-" };

	public void SwitchType (Dropdown drop) {
		SwitchByInt(drop.value);
	}

	public void UpdateMemory () {
		currentMemory.key = memoryName.text;
		if (allOperations.Count == 0) {
			// If a file is loaded, Start() may never have been run - somehow.
			Start();
		}
		string memType = currentMemory.GetTemplatedType();
		if (memType == "NONE") return;
		if (memType == "Int32") {
			int attempt = 0;
			System.Int32.TryParse(memoryValue.text, out attempt);
			((Memory<int>)currentMemory).value = attempt;
		} else if (memType == "String") {
			((Memory<string>)currentMemory).value = memoryValue.text;
		} else if (memType == "Boolean") {
			((Memory<bool>)currentMemory).value = (memoryValueBoolean.value == 0);
		}
		currentMemory.operation = memoryOperation.options[Mathf.Max(0, memoryOperation.value)].text;
	}

	// Switch the UI based on the type of destination given:
	//		0 = Int
	//		1 = String
	//		2 = Bool
	// Unlike Destinations and Events, there is no "base" memory.
	void SwitchByInt (int val) {
		memoryValue.gameObject.SetActive(true);
		memoryValueBoolean.gameObject.SetActive(false);
		memoryValue.text = "";
		memoryOperation.value = 0;
		int oldID = currentMemory.id;
		if (val == 0) {
			currentMemory = new Memory<int>(currentMemory);
			memoryValue.contentType = InputField.ContentType.IntegerNumber;
			memoryOperation.options = allOperations;
		} else if (val == 1) {
			currentMemory = new Memory<string>(currentMemory);
			memoryValue.contentType = InputField.ContentType.Alphanumeric;
			memoryOperation.options = setAddOperations;
		} else if (val == 2) {
			currentMemory = new Memory<bool>(currentMemory);
			memoryValue.gameObject.SetActive(false);
			memoryValueBoolean.gameObject.SetActive(true);
			memoryOperation.options = setOnlyOperations;
		}
		currentMemory.id = oldID;
	}

	public void Setup (MemoryBase mem) {
		currentMemory = mem;
		string memType = mem.GetTemplatedType();
		memoryName.text = mem.key;
		if (memType == "Int32") {
			memoryType.value = 0;
			memoryValue.text = ((Memory<int>)mem).value.ToString();
			memoryValue.contentType = InputField.ContentType.IntegerNumber;
			memoryOperation.options = allOperations;
		} else if (memType == "String") {
			memoryType.value = 1;
			memoryValue.text = ((Memory<string>)mem).value;
			memoryValue.contentType = InputField.ContentType.Alphanumeric;
			memoryOperation.options = setAddOperations;
		} else if (memType == "Boolean") {
			memoryType.value = 2;
			memoryValue.gameObject.SetActive(false);
			memoryValueBoolean.gameObject.SetActive(true);
			if (((Memory<bool>)mem).value) {
				memoryValueBoolean.value = 0;
			} else {
				memoryValueBoolean.value = 1;
			}
			memoryOperation.options = setOnlyOperations;
		}
		// Set the value last, since it may be changed when the options change.
		memoryOperation.value = System.Array.IndexOf(operations, mem.operation);
	}

	public void SelectForDelete () {
		NodeManager.instance.DeleteMemory(currentMemory.id);
	}

	void Start () {
		// If this is the first DestinationObject, set up the check lists.
		if (allOperations.Count == 0) {
			allOperations.Add(new Dropdown.OptionData("set"));
			allOperations.Add(new Dropdown.OptionData("+"));
			allOperations.Add(new Dropdown.OptionData("-"));
			setAddOperations.Add(new Dropdown.OptionData("set"));
			setAddOperations.Add(new Dropdown.OptionData("+"));
			setOnlyOperations.Add(new Dropdown.OptionData("set"));
		}
	}

}
