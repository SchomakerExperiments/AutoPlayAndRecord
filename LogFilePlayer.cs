using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

public class LogFilePlayer : MonoBehaviour
{
	[Header("Folder Paths")]
	public string logFolderPath = @"C:\Users\judit\OneDrive\Desktop\ML_agents\CSV\CSV";
	public string recordingFolder = @"C:\Users\judit\Videos"; // <-- Update in Inspector to match OBS

	private string[] logFiles;
	private int currentFileIndex = 0;

	private OBSController obs;
	private PathReplayer pathReplayer;

	IEnumerator Start()
	{
		obs = FindObjectOfType<OBSController>();
		pathReplayer = FindObjectOfType<PathReplayer>();

		logFiles = Directory.GetFiles(logFolderPath, "*.csv");

		if (logFiles.Length == 0)
		{
			Debug.LogWarning("No log files found in: " + logFolderPath);
			yield break;
		}

		if (obs != null)
		{
			obs.Connect();
			yield return new WaitForSeconds(1f); // Give OBS time to connect
		}

		yield return StartCoroutine(ReplayAndRecord());
	}

	IEnumerator ReplayAndRecord()
	{
		while (currentFileIndex < logFiles.Length)
		{
			string logFilePath = logFiles[currentFileIndex];
			string fileName = Path.GetFileNameWithoutExtension(logFilePath) + ".mkv";

			Debug.Log("Starting replay: " + logFilePath);

			if (obs != null)
			{
				obs.StartRecording(fileName);
			}

			if (pathReplayer != null)
			{
				yield return StartCoroutine(pathReplayer.LoadAndPlay(logFilePath));
			}

			if (obs != null)
			{
				obs.StopRecording();
				yield return new WaitForSeconds(2f); // Give OBS time to finalize the file

				// Rename based on original log filename
				yield return StartCoroutine(RenameLastRecording(fileName));
			}

			yield return new WaitForSeconds(1f); // Optional buffer

			currentFileIndex++;
		}

		Debug.Log("All replays finished.");

		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}

	IEnumerator RenameLastRecording(string newName)
	{
		const int maxRetries = 10;
		const float retryDelay = 0.5f;

		if (!Directory.Exists(recordingFolder))
		{
			Debug.LogError($"Recording folder not found: {recordingFolder}");
			yield break;
		}

		var dir = new DirectoryInfo(recordingFolder);
		var lastFile = dir.GetFiles("*.mkv")
			.OrderByDescending(f => f.LastWriteTime)
			.FirstOrDefault();

		if (lastFile == null)
		{
			Debug.LogWarning("No recent recording found to rename.");
			yield break;
		}

		string newPath = Path.Combine(recordingFolder, newName);

		for (int i = 0; i < maxRetries; i++)
		{
			try
			{
				if (File.Exists(newPath))
				{
					File.Delete(newPath);
				}

				File.Move(lastFile.FullName, newPath);
				Debug.Log("Renamed recording to: " + newName);
				yield break;
			}
			catch (IOException ex)
			{
				Debug.LogWarning($"Rename attempt {i + 1} failed: {ex.Message}");
			}

			yield return new WaitForSeconds(retryDelay);
		}

		Debug.LogError("Failed to rename recording after multiple attempts.");
	}

}
