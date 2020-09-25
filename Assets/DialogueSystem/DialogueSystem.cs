using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using InsomniaSystemTypes;

public class DialogueSystem : MonoBehaviour
{

	string temp;

	public enum DialogueMode { Standard, Messaging, Speech, Cinematic };

	public DialogueEventHandler events;
	public DialogueMemories memories;

	[Header("Text")]
	public TextAsset[] mainConversations;

	[Header("UI")]
	// For enabling/disabling the system at large.
	public GameObject fullSystem;

	[Header("Preferences")]
	public char regexCharacter = '%';

	bool click = false;
	[System.NonSerialized]
	public bool ready = true;
	bool choices = false;
	bool usingMemDest = false;
	[System.NonSerialized]
	public int i = 0;

	// Variables used in the reading of dialogue files and their destinations.
	[System.NonSerialized]
	public List<Node> currentNodes;
	Node node;
	List<Destination> dests = new List<Destination>();
	List<MemoryDestination> memdests = new List<MemoryDestination>();

	// Node Array creator, reading from a TextAsset.
	List<Node> ReadFromFile (TextAsset file) {
		string fileText = file.text;
		string[] lines = fileText.Split('\n');
		List<Node> nodes = new List<Node>();
		for (int i = 0; i < lines.Length; ++i) {
			if (lines[i].Length == 0) continue;
			nodes.Add(JsonUtility.FromJson<Node>(lines[i]));
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

	public void PlayerChoice (int choiceID) {
		i = dests[choiceID].dest;
		click = true;
		choices = false;
	}

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

	IEnumerator ReadConversation (List<Node> nodes) {
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
				for (int j = 0; j < node.destinations.Count; ++j) {
					if (memdests.GetType() == Utilities.memoryDestination.GetType()) {
						memdests.Add((MemoryDestination)node.destinations[j]);
					} else {
						dests.Add(node.destinations[j]);
					}
				}
				if (memdests.Count > 0) {
					for (int j = 0; j < memdests.Count; ++j) {
						if (memories.memories.CheckMemory(memdests[i])) {
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

	public void StartConversation (int conversationIndex, string conversationLine) {
		StartCoroutine(ReadConversation(ReadFromFile(mainConversations[conversationIndex])));
	}

	void Start () {
		StartCoroutine(ReadConversation(ReadFromFile(mainConversations[0])));
	}

	void Update () {
		// Advances text forward.
		if (ready && Input.GetMouseButtonUp(0) && !choices) {
			click = true;
		}
	}

}