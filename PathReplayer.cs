using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathReplayer : MonoBehaviour
{
	public Camera replayCamera; // Assign this in the Unity Inspector

	private List<PathCoordinate> coordinates = new List<PathCoordinate>();
	public float speedMultiplier = 10f; // You can tweak this in the Inspector
	public int lookAheadSteps = 200; // Number of steps to look ahead for smoother heading

	public bool isReplayDone = false; // This flag will be true when replay is finished

	public IEnumerator LoadAndPlay(string fullPath)
	{
		LoadCoordinates(fullPath);
		isReplayDone = false;  // Reset the flag before starting the replay
		yield return StartCoroutine(ReplayPath(coordinates));
	}

	public void LoadCoordinates(string filepath)
	{
		coordinates.Clear();
		var lines = File.ReadAllLines(filepath);

		for (int i = 1; i < lines.Length; i++) // Skip header
		{
			string[] parts = lines[i].Split(',');

			float time = float.Parse(parts[0]);
			float x = float.Parse(parts[1]);
			float y = float.Parse(parts[2]);
			float z = float.Parse(parts[3]);

			coordinates.Add(new PathCoordinate(time, x, y, z));
		}
	}

	IEnumerator ReplayPath(List<PathCoordinate> path)
	{
		// Reset isReplayDone to false at the start of each replay
		isReplayDone = false;

		for (int i = 0; i < path.Count - 1; i++)
		{
			Vector3 start = path[i].position;
			Vector3 end = path[i + 1].position;
			float duration = path[i + 1].time - path[i].time;

			float elapsed = 0f;
			while (elapsed < duration)
			{
				float t = elapsed / duration;
				Vector3 newPos = Vector3.Lerp(start, end, t);
				replayCamera.transform.position = newPos;

				// Calculate look-ahead index
				int lookAheadIndex = Mathf.Min(i + lookAheadSteps, path.Count - 1);
				Vector3 lookAheadPoint = path[lookAheadIndex].position;

				// Determine direction to look-ahead point
				Vector3 direction = (lookAheadPoint - newPos).normalized;
				if (direction != Vector3.zero)
				{
					Quaternion targetRotation = Quaternion.LookRotation(direction);
					replayCamera.transform.rotation = Quaternion.Slerp(
						replayCamera.transform.rotation,
						targetRotation,
						Time.deltaTime * 2f // Adjust smoothing factor as needed
					);
				}

				elapsed += Time.deltaTime * speedMultiplier;
				yield return null;
			}

			// Snap to final position and rotation
			replayCamera.transform.position = end;
			Vector3 finalDirection = (path[Mathf.Min(i + lookAheadSteps, path.Count - 1)].position - end).normalized;
			if (finalDirection != Vector3.zero)
			{
				replayCamera.transform.rotation = Quaternion.LookRotation(finalDirection);
			}
		}

		// Replay is finished, set the flag to true
		isReplayDone = true;
	}
}

public class PathCoordinate
{
	public float time;
	public Vector3 position;

	public PathCoordinate(float time, float x, float y, float z)
	{
		this.time = time;
		this.position = new Vector3(x, y, z);
	}
}
