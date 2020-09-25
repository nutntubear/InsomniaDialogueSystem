using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class SimpleTester : DialogueSystem
{

	[Header("UI Elements")]
	public Text bodyBox;
	public Text speakerBox;
	// List of choices.
	public List<Text> choiceTexts;

	public override void SetTextBox (string body, string speaker, bool isPlayer=false) {
		bodyBox.text = body;
		speakerBox.text = speaker;
	}

	public override void SetChoices (List<Destination> dests, string speaker) {
		for (i = 0; i < choiceTexts.Count; ++i) {
			if (i >= dests.Count) {
				choiceTexts[i].text = "";
				choiceTexts[i].transform.parent.GetComponent<Button>().interactable = false;
				continue;
			}
			choiceTexts[i].text = currentNodes[dests[i].dest].body;
			choiceTexts[i].transform.parent.GetComponent<Button>().interactable = true;
		}
	}

	public override void ResetChoices () {
		for (i = 0; i < choiceTexts.Count; ++i) {
			choiceTexts[i].text = "";
			choiceTexts[i].transform.parent.GetComponent<Button>().interactable = false;
		}
	}

	public override void End () {
		ResetChoices();
		bodyBox.text = "";
		speakerBox.text = "";
	}

}
