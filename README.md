# AutoPlayAndRecord
These #C scripts allow you to read in logfiles with X, Y, Z coordinates of (previous) exploration/navigation behavior, to replay that behavior and visualize what people saw in Unity, and to record this with Open Broadcaster Studio (OBS). 

1. Prepare your logfiles in the following format: [time, X, Y, X], including these headers.
2. Specify the path for the logfiles in LogFilePlayer.cs in logFolderPath.
3. Specify the path for the recorded video's of each player's behavior in LogFilePlayer.cs in recordingFolder.
4. Install OBS Studio (I used 27.2.4, 64-bit Windows).
5. In OBSController.cs check the WebSocket server port. Default is often "4444" or "4455". You can find the address under Tools > Websocket Server Settings. Disable authentication (uncheck "Enable authentication").
6. Add WebSocketSharp.dll in your "Assets\Plugins" folder. If it does not exist yet, create the Plugins-folder yourself.
7. Attach PathReplayer.cs, OBSController.cs and LogFilePlayer to a game object in your Unity project. I used the FPSController object for this, but make sure to uncheck Camera for this object.
8. Select the replay camera in the Instructor-window of PathReplayer.
9. In the Speed Multiplier box of the same Instructor-window define the speed of the replay. Imagine your sampling rate was 60Hz (i.e., 60 lines per second), you would have to use a multiplication factor of 60 to get a replay at the original speed.
10. In the data that I used we did not have mouse movements logged. In order to make navigation behavior look natural heading direction was used to make the looking behavior more natural (i.e., the camera is pointed X-steps ahead). I recommend playing around with the settings a bit. For our data 50-100 steps made it look relatively natural. Define this number under Look Ahead Steps in the Instructor-window of PathReplayer.
11. Make sure that OBS is running, and that your task screen or the full display is selected as a source.
12. Hit play in Unity. The task will now loop over the logfiles that can be found in your logFolderPath, will replay the epxloration/navigation behavior and will create .mkv video's in your recordingFolder. The video files will be saved with the original logfile-names.
