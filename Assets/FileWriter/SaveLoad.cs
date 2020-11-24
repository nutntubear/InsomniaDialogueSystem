using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SFB;

public class SaveLoad : MonoBehaviour
{

	[HideInInspector]
	public static SaveLoad instance;

	[Header("UI Objects")]
	public GameObject pauseScreen;
	public Text currentFile;

	void Awake () {
		// Singleton
		if (instance == null) {
			instance = this;
		} else {
			Destroy(this.gameObject);
		}
		DontDestroyOnLoad(this.gameObject);
	}

	public string[] loadingFile = null;
	ExtensionFilter[] extensions = { new ExtensionFilter("IDS JSON Files", "json") };
	public bool paused = true;
	public string editingFile = "";

	string GetFileName (string path) {
		string[] split = path.Split('\\');
		return split[split.Length - 1];
	}

	public void NewFile () {
		SceneManager.LoadScene("main");
	}

	public void OpenInsomniaFile () {
		string[] paths = StandaloneFileBrowser.OpenFilePanel("Open Insomnia System File", "", "json", false);
		if (paths.Length == 1) {
			loadingFile = File.ReadAllLines(paths[0]);
			editingFile = paths[0];
			currentFile.text = "Editing " + GetFileName(editingFile);
			// Load the scene again, resetting all values in the editor, but keeping the loaded file.
			SceneManager.LoadScene("main");
		}
	}

	public void SaveInsomniaFileAs () {
		string saved = StandaloneFileBrowser.SaveFilePanel("Save Insomnia System File", "", "NewDialogue", extensions);
		if (saved == null || saved == "") return;
		editingFile = saved;
		currentFile.text = "Editing " + GetFileName(editingFile);
		Save(saved);
	}

	public void SaveInsomniaFile () {
		if (editingFile == "") {
			SaveInsomniaFileAs();
		} else {
			Save(editingFile);
		}
	}

	void Save (string path) {
		string writeToFile = "";
		for (int i = 0; i < NodeManager.instance.nodes.Count; ++i) {
			writeToFile += NodeManager.instance.nodes[i].node.SaveNode() + "\n";
		}
		File.WriteAllText(path, writeToFile);
	}

	void Update () {
		if (Input.GetKeyDown("escape")) {
			paused = !paused;
			pauseScreen.SetActive(paused);
		}
		if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown("s")) {
			SaveInsomniaFile();
		}
	}

}
