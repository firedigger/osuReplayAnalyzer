# osu!ReplayAnalyzer

About
--
This is a program that works as a client-sided 'anti-cheat' for the game [osu!](https://osu.ppy.sh/).  
It accepts replay files and beatmaps as an input, then performs heuristic analysis on the provided replay files and points out suspicious parameters that show up while analyzing the replay. Including *but not limited to*:

* Indicators of macro-bots ('relax bots') such as key press time.
* Over-aims and edge-hits ('pixel perfect hits').
* Incorrect interval of timing between each replay frame (timescale detection).
* Behavior that suggests robotic aim.
* Extremely fast singletaps ('relax')
* Interval between key presses.
* Cursor teleportations, indicator of badly coded replay stealing software or touchscreen plays. (Depends on the case, should be studied by hand)

Usage
--
CLI
___
Simply open the program (`osuDodgyMomentsFinder.exe`) and it will provide you the following text:

	Welcome the firedigger's replay analyzer. Use one of 3 options
	-i (path to replay) for getting info about a certain replay
	-ia for getting info about all replays in the current folder
	-c for comparing all the replays in the current folder against each other
	-cr for comparing the replays from command line args

GUI
___
Alternatively, you can use the GUI.  
It's very self-explainatory! Choose a map or navigate to your osu! database file, select a replay or a folder that contains replay files and choose your desired analyzer setting.

Screenshots
--
### CLI
___
![Screenshot of the osu!ReplayAnalyzer CLI](https://i.imgur.com/OEsGmpe.png)

### GUI
___
![Screenshot of the osu!ReplayAnalyzer GUI](https://i.imgur.com/cIrzSG0.png)