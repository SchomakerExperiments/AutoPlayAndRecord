using UnityEngine;
using WebSocketSharp;
using System.Collections;

public class OBSController : MonoBehaviour
{
	private WebSocket ws;
	private int messageId = 1;

	public void Connect()
	{
		if (ws != null && ws.ReadyState == WebSocketState.Open)
			return;

		ws = new WebSocket("ws://127.0.0.1:4444");

		// Logging to ensure WebSocket events are firing correctly
		ws.OnOpen += (sender, e) => 
		{
			Debug.Log("OBS WebSocket connected.");
			// Confirm if the connection is established
		};

		ws.OnError += (sender, e) => 
		{
			Debug.LogError("OBS WebSocket error: " + e.Message);
			// Log detailed error messages
		};

		ws.OnClose += (sender, e) => 
		{
			Debug.Log("OBS WebSocket closed.");
		};

		ws.OnMessage += (sender, e) => 
		{
			Debug.Log("OBS WebSocket message received: " + e.Data);
			// Log any messages received from OBS
		};

		// Try connecting asynchronously
		ws.ConnectAsync();
	}


	public void StartRecording(string fileName)
	{
		if (ws == null || ws.ReadyState != WebSocketState.Open)
		{
			Debug.LogWarning("WebSocket not connected. ReadyState: " + ws.ReadyState);
			return;
		}

		string messageSetFilename = "{\"request-type\": \"SetRecordingFilenameFormatting\", \"message-id\": \"" + (messageId++) + "\", \"format\": \"" + fileName + "\"}";
		string messageStartRecording = "{\"request-type\": \"StartRecording\", \"message-id\": \"" + (messageId++) + "\"}";

		Debug.Log("Sending StartRecording message: " + messageStartRecording); // Log the message to be sent

		ws.Send(messageSetFilename);
		ws.Send(messageStartRecording);
	}


	public void StopRecording()
	{
		if (ws == null || ws.ReadyState != WebSocketState.Open)
			return;

		string message = "{\"request-type\": \"StopRecording\", \"message-id\": \"" + (messageId++) + "\"}";
		ws.Send(message);
	}

	public void Disconnect()
	{
		if (ws != null)
		{
			ws.CloseAsync();
			ws = null;
		}
	}

	private void OnApplicationQuit()
	{
		Disconnect();
	}
}
