using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using InsomniaSystemTypes;

public class FileTester : MonoBehaviour
{

	string temp;

	public DialogueEventHandler events;
	public DialogueMemories memories;

	[Header("Text")]
	public TextAsset[] dialogues;

	[Header("UI")]
	// For enabling/disabling the system at large.
	public GameObject fullSystem;

	[Header("Preferences")]
	[Tooltip("A character used for replacing sections of dialogue with memories.")]
	public char regexCharacter = '%';

	[System.NonSerialized]
	public bool click = false;
	[System.NonSerialized]
	public bool ready = true;
	[System.NonSerialized]
	public bool choices = false;
	bool usingMemDest = false;
	[System.NonSerialized]
	public int i = 0;

	// Variables used in the reading of dialogue files and their destinations.
	[System.NonSerialized]
	public List<Node> currentNodes;
	Node node;
	List<Destination> dests = new List<Destination>();
	List<MemoryDestination> memdests = new List<MemoryDestination>();
	Regex numCheck = new Regex(System.String.Format(@"{0}value{0}:(-?[0-9]+)", "\""));
	Regex stringCheck = new Regex(System.String.Format(@"{0}value{0}:{0}(.+?){0}", "\""));
	Regex boolCheck = new Regex(System.String.Format(@"{0}value{0}:(true|false)", "\""));

	// Node Array creator, reading from a TextAsset.
	List<Node> ReadFromFile (TextAsset file) {
		string fileText = file.text;
		string[] lines = fileText.Split('\n');
		List<Node> nodes = new List<Node>();
		for (int i = 0; i < lines.Length; ++i) {
			if (lines[i].Length == 0) continue;
			Node adding = JsonUtility.FromJson<Node>(lines[i]);
			adding.destinations = GetDestinations(lines[i]);
			nodes.Add(adding);
		}
		nodes.Sort(Utilities.SortNode);
		return nodes;
	}

	// Node Array creator, reading from a string array (should an array of strings ever
	// be created from an Insomnia JSON file).
	List<Node> ReadFromArray (string[] lines) {
		List<Node> nodes = new List<Node>();
		for (int i = 0; i < lines.Length; ++i) {
			nodes.Add(JsonUtility.FromJson<Node>(lines[i]));
		}
		nodes.Sort(Utilities.SortNode);
		return nodes;
	}

	// Destination list creation, to make sure that MemoryDestinations are created.
	List<Destination> GetDestinations (string node) {
		List<Destination> dests = new List<Destination>();
		// Find the section of the node JSON string where destinations are found.
		int start = node.IndexOf("],\"destinations\":[") + 2;
		int end = node.IndexOf("],\"memories\":[") + 1;
		string[] destinations = node.Substring(start, end - start).Split('{');
		string destString;
		for (int i = 1; i < destinations.Length; ++i) {
			destString = "{" + destinations[i].Substring(0, destinations[i].Length - 1);
			if (!destString.Contains("memoryKey")) {
				dests.Add(JsonUtility.FromJson<Destination>(destString));
				continue;
			}
			// If it is a memory destination, use regex to check each type:
			MatchCollection mc = numCheck.Matches(destString);
			if (mc.Count > 0) {
				dests.Add(JsonUtility.FromJson<MemoryDestinationInt>(destString));
			} else {
				mc = stringCheck.Matches(destString);
				if (mc.Count > 0) {
					dests.Add(JsonUtility.FromJson<MemoryDestinationString>(destString));
				} else {
					mc = boolCheck.Matches(destString);
					if (mc.Count > 0) {
						dests.Add(JsonUtility.FromJson<MemoryDestinationBool>(destString));
					} else {
						// If it somehow gets to this point, add it as a generic MemoryDestination.
						dests.Add(JsonUtility.FromJson<MemoryDestination>(destString));
					}
				}
			}
		}
		return dests;
	}

	// Called by dialogue buttons when a choice is made.
	public void PlayerChoice (int choiceID) {
		i = dests[choiceID].dest;
		click = true;
		choices = false;
	}

	// Uses the regexCharacter to identify any sections of text in a dialogue node that should be replaced
	// by a memory.
	// Example: regexCharacter = '%'. "My name is %name%." could become "My name is John."
	public string ReplaceByMemory (string line) {
		Regex r = new Regex(System.String.Format(@"{0}(.+?){0}", regexCharacter));
		MatchCollection mc = r.Matches(line);
		string tempReplace = "";
		foreach (Match m in mc) {
			if (memories.memories.Contains(m.Value.Trim('%'), out tempReplace)) {
				line = line.Replace(m.Value, tempReplace);
			}
		}
		return line;
	}

	// An IEnumerator to read through a dialogue file.
	IEnumerator ReadFile (List<Node> nodes) {
		ResetChoices();
		i = 0;
		currentNodes = nodes;
		while (i != -1) {
			// Reset any booleans used in reading a node and its destinations.
			ready = false;
			usingMemDest = false;
			node = nodes[i];
			SetTextBox(node.body, node.speaker, node.player);
			if (node.type == 'b') {
				// Branching node: Populate lists of memory based choices and standard choices.
				dests = new List<Destination>();
				memdests = new List<MemoryDestination>();
				string destType;
				for (int j = 0; j < node.destinations.Count; ++j) {
					// If a memory destination is found, add it to the list of memory destinations.
					destType = Utilities.GetDestinationType(node.destinations[j]);
					if (destType == "int") {
						memdests.Add((MemoryDestinationInt)node.destinations[j]);
					} else if (destType == "string") {
						memdests.Add((MemoryDestinationString)node.destinations[j]);
					} else if (destType == "bool") {
						memdests.Add((MemoryDestinationBool)node.destinations[j]);
					} else {
						// If it's a normal destination, just add it to the list of destinations.
						dests.Add(node.destinations[j]);
					}
				}
				// If there are memory destinations, check all of them using the MemoryDictionary's CheckMemory method.
				// If the MemoryCheck returns true, add it to memDests. If it is true and the memory destination is forced,
				// set i to point to that memory.
				if (memdests.Count > 0) {
					for (int j = 0; j < memdests.Count; ++j) {
						if (memories.memories.CheckMemory(memdests[j])) {
							if (memdests[j].forced) {
								usingMemDest = true;
								i = memdests[j].dest;
								break;
							} else {
								dests.Add(memdests[j]);
							}
						}
					}
				}
				// Set choices, as long as a forced MemoryDestination isn't being used.
				if (!usingMemDest) {
					if (dests.Count > 1) {
						choices = true;
						SetChoices(dests, node.speaker);
					} else {
						i = dests[0].dest;
					}
				}
			} else if (node.type == 'n') {
				ResetChoices();
				// Normal node: Set the destination.
				i = node.destinations[0].dest;
			} else if (node.type == 'e') {
				i = -1;
			}

			// Manage memories.
			if (node.memories.Count > 0) {
				for (int j = 0; j < node.memories.Count; ++j) {
					memories.memories.SetMemory(node.memories[j]);
				}
			}

			// Manage events.
			if (node.events.Count > 0) {
				for (int j = 0; j < node.events.Count; ++j) {
					events.TriggerEvent(node.events[j]);
				}
			}
			
			// Wait until the player continues.
			ready = true;
			yield return new WaitUntil(() => click);
			click = false;
		}
		currentNodes = new List<Node>();
		End();
	}

	// Methods for displaying dialogue, to be overridden in readers.
	public virtual void SetTextBox (string body, string speaker, bool isPlayer=false) {}
	public virtual void SetChoices (List<Destination> dests, string speaker) {}
	public virtual void ResetChoices () {}
	public virtual void End () {}

	public virtual void StartReading (int fileIndex) {
		StartCoroutine(ReadFile(ReadFromFile(dialogues[fileIndex])));
	}

}