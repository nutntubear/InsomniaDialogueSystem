using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class DialogueSystem : MonoBehaviour
{

	string temp;

	public enum DialogueMode { Standard, Messaging, Speech, Cinematic };

	public DialogueEventHandler events;
	public DialogueMemories memories;

	[Header("Text")]
	public TextAsset[] mainConversations;
	public TextAsset[] altConversations;

	[Header("UI Prefabs")]
	// For use with systems that create text bubbles/messages.
	public GameObject message;

	[Header("UI Elements")]
	// For use with a back-and-forth "RPG" system; showing who is speaking at present.
	public Text body;
	public Text speaker;
	// For use with a text bubble/messaging system; the parents of messages.
	public RectTransform otherParent;
	public RectTransform playerParent;
	// List of choices.
	public Text[] choiceTexts;

	// For enabling/disabling the system at large.
	public GameObject fullSystem;

	public float bottomYPos;

	[Header("Preferences")]
	public DialogueMode mode = DialogueMode.Standard;
	public bool delayBetweenMessages = false;
	public float delayTime = 1;
	public float spaceBetweenMessages = 25;

	bool click = false;
	[System.NonSerialized]
	public bool ready = true;
	bool choices = false;
	bool usingMemDest = false;
	int i = 0;

	// Temporary variables used in the reading of nodes and their destinations.
	Node node;
	List<Destination> dests = new List<Destination>();
	List<MemoryDestination> memdests = new List<MemoryDestination>();

	// Node Array creator, reading from a TextAsset.
	Node[] ReadFromFile (TextAsset file) {
		string fileText = file.text;
		string[] lines = fileText.Split('\n');
		Node[] nodes = new Node[lines.Length];
		for (int i = 0; i < lines.Length; ++i) {
			nodes[i] = JsonUtility.FromJson<Node>(lines[i]);
		}
		return nodes;
	}

	// Node Array creator, reading from a string array (should an array of strings ever
	// be created from an Insomnia JSON file).
	Node[] ReadFromArray (string[] lines) {
		Node[] nodes = new Node[lines.Length];
		for (int i = 0; i < lines.Length; ++i) {
			nodes[i] = JsonUtility.FromJson<Node>(lines[i]);
		}
		return nodes;
	}

	public void PlayerChoice (int dest, string text) {
		i = dest;
		click = true;
		choices = false;
	}

	IEnumerator ReadConversation (Node[] nodes) {
		i = 0;
		while (i != -1) {
			// Reset any booleans used in reading a node and its destinations.
			ready = false;
			usingMemDest = false;
			node = nodes[i];
			SetTextBox(node.body, node.speaker);
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
					SetChoices(dests, node.speaker);
				}
			} else if (node.type == 'n') {
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
	}

	public virtual void SetTextBox (string body, string speaker) {
		
	}

	public virtual void SetChoices (List<Destination> dests, string speaker) {
		
	}

	public void StartConversation (int conversationIndex, string conversationLine) {
		StartCoroutine(ReadConversation(ReadFromFile(mainConversations[conversationIndex])));
	}

	void Start () {
		StartCoroutine(ReadConversation(ReadFromFile(mainConversations[0])));
	}

	void Update () {
		if (ready && Input.GetMouseButtonUp(0) && !choices) {
			click = true;
		}
	}

}