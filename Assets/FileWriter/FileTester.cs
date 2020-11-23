using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using InsomniaSystemTypes;
using UnityEngine.UI;

public class FileTester : MonoBehaviour
{

    string temp;

    public DialogueEventHandler events;
    public DialogueMemories memories;
    public RectTransform ScrollChoice;
    public RectTransform ScrollR;
    [System.NonSerialized]
    public int logDepth = 0;

    [Header("Text")]
    public TextAsset[] dialogues;

    [Header("UI")]
    public Text bodyBox;
    public Text speakerBox;
    // List of choices.
    public List<Text> choiceTexts;
    public GameObject DebugText;
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
    int j = 0;
    string s;

    // Node Array creator, reading from a TextAsset.
    List<Node> ReadFromFile(TextAsset file) {
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
    List<Node> ReadFromArray(string[] lines) {
        List<Node> nodes = new List<Node>();
        for (int i = 0; i < lines.Length; ++i) {
            if (lines[i].Length == 0) continue;
            nodes.Add(JsonUtility.FromJson<Node>(lines[i]));
        }
        nodes.Sort(Utilities.SortNode);
        return nodes;
    }

    // Called by dialogue buttons when a choice is made.
    public void PlayerChoice(int choiceID) {
        i = dests[choiceID].dest;
        click = true;
        choices = false;
    }

    // Uses the regexCharacter to identify any sections of text in a dialogue node that should be replaced
    // by a memory.
    // Example: regexCharacter = '%'. "My name is %name%." could become "My name is John."
    public string ReplaceByMemory(string line) {
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


    
    


    public void PrintInTextBox(string s)
    {
        if ((logDepth + 1) * 80 >= ScrollR.sizeDelta.y)
        {
            ScrollR.sizeDelta = new Vector2(0, (logDepth + 1) * 80);
            ScrollR.anchoredPosition = new Vector2(0, -ScrollR.sizeDelta.y / 2);
        }
        GameObject text = Instantiate(DebugText, ScrollR);
        text.GetComponent<RectTransform>().localPosition = new Vector2(0, -80 * logDepth + 200);
        text.GetComponent<Text>().text = s;
        logDepth += 1;
        print(s);
    }

    // An IEnumerator to read through a dialogue file.
    IEnumerator ReadFile(List<Node> nodes) {
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
                // If there are memory destinations, check all of them using the MemoryDictionary's CheckMemory method.
                // If the MemoryCheck returns true, add it to memDests. If it is true and the memory destination is forced,
                // set i to point to that memory.
                dests = new List<Destination>();
                for (j = 0; j < node.destinations.Count; ++j) {
                    dests.Add(node.destinations[j]);
                }
                for (j = 0; j < node.intDestinations.Count; ++j) {
                    if (memories.memories.CheckMemoryInt(node.intDestinations[j])) {
                        if (node.intDestinations[j].forced) {
                            usingMemDest = true;
                            i = node.intDestinations[j].dest;
                            break;
                        } else {
                            dests.Add(node.intDestinations[j]);
                        }
                    }
                }
                for (j = 0; j < node.stringDestinations.Count; ++j) {
                    if (memories.memories.CheckMemoryString(node.stringDestinations[j])) {
                        if (node.stringDestinations[j].forced) {
                            usingMemDest = true;
                            i = node.stringDestinations[j].dest;
                            break;
                        } else {
                            dests.Add(node.stringDestinations[j]);
                        }
                    }
                }
                for (j = 0; j < node.boolDestinations.Count; ++j) {
                    if (memories.memories.CheckMemoryBool(node.boolDestinations[j])) {
                        if (node.boolDestinations[j].forced) {
                            usingMemDest = true;
                            i = node.boolDestinations[j].dest;
                            break;
                        } else {
                            dests.Add(node.boolDestinations[j]);
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

            // Manage events.
            s = "";
            for (j = 0; j < node.events.Count; ++j) {
                events.TriggerEvent(node.events[j]);
                s += "Triggered event '" + node.events[j].ToString() + "'\n";
            }
            for (j = 0; j < node.intEvents.Count; ++j) {
                events.TriggerEvent(node.intEvents[j]);
                s += "Triggered event '" + node.intEvents[j].ToString() + "'\n";
            }
            for (j = 0; j < node.stringEvents.Count; ++j) {
                events.TriggerEvent(node.stringEvents[j]);
                s += "Triggered event '" + node.stringEvents[j].ToString() + "'\n";
            }
            for (j = 0; j < node.boolEvents.Count; ++j) {
                events.TriggerEvent(node.boolEvents[j]);
                s += "Triggered event '" + node.boolEvents[j].ToString() + "'\n";
            }
            if (s != "")
            {
                PrintInTextBox(s);
            }


            // Manage memories.
            s = "";
            for (j = 0; j < node.intMemories.Count; ++j) {
                memories.memories.SetMemoryInt(node.intMemories[j]);
                s += "Set a new memory '" + node.intMemories[j].ToString() + "'\n";
            }
            for (j = 0; j < node.stringMemories.Count; ++j) {
                memories.memories.SetMemoryString(node.stringMemories[j]);
                s += "Set a new memory '" + node.stringMemories[j].ToString() + "'\n";
            }
            for (j = 0; j < node.boolMemories.Count; ++j) {
                memories.memories.SetMemoryBool(node.boolMemories[j]);
                s += "Set a new memory '" + node.boolMemories[j].ToString() + "'\n";
            }
            if (s != "")
            {
                PrintInTextBox(s);
            }

            // Wait until the player continues.
            ready = true;
            yield return new WaitUntil(() => click);

            //Destroy all children in the horizontal rect


            click = false;
        }
        currentNodes = new List<Node>();
        End();
    }

    // Methods for displaying dialogue, to be overridden in readers.
    public void SetTextBox(string body, string speaker, bool isPlayer = false)
    {
        bodyBox.text = ReplaceByMemory(body);
        speakerBox.text = speaker;
    }

    // Sets each of the choices that need to be set up with their text and making them interactable.
    public void SetChoices(List<Destination> dests, string speaker)
    {
        for (i = 0; i < choiceTexts.Count; ++i)
        {
            if (i >= dests.Count)
            {
                choiceTexts[i].text = "";
                choiceTexts[i].transform.parent.GetComponent<Button>().interactable = false;
                continue;
            }
            choiceTexts[i].text = ReplaceByMemory(currentNodes[dests[i].dest].body);
            choiceTexts[i].transform.parent.GetComponent<Button>().interactable = true;
        }
    }


    //Make a button Prefab and save it as a variable (assuming a width of 300)
    //  i = 0
    //  for choices in dests{
    //  if ( i + 1 * 80 >= ScrollDest.sizeDelta.x){
    //      ScrollR.siz
    //
    //  }
    //  choice.GetComponent<RectTransform>().localPosition = new Vector2(325 * i + 50);
    //  choices.GetComponent<Button>().onClick.AddListener(() => PlayerChoice([i]));
    //  choices.GetComponent<Text>.text = ReplaceByMemory(currentNodes[dests[i].dest].body);
    //  i += 1;
    //}






    // Turning all choices off.
    public void ResetChoices()
    {
        for (i = 0; i < choiceTexts.Count; ++i)
        {
            choiceTexts[i].text = "";
            choiceTexts[i].transform.parent.GetComponent<Button>().interactable = false;
        }
    }

    public void End()
    {
        ResetChoices();
        bodyBox.text = "";
        speakerBox.text = "";
    }

	public virtual void StartReading (int fileIndex) {
		StartCoroutine(ReadFile(ReadFromFile(dialogues[fileIndex])));
	}

    void Update()
    {
        // Advances text forward.
        if (ready && Input.GetMouseButtonUp(0) && !choices)
        {
            click = true;
        }
    }

    void Start()
    {
        StartReading(0);
    }



}