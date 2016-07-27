# osuReplayAnalyzer

This is an osu! anticheat based on heuristic analysis. It analyzes a player's replay and points to specific moments that should be investigated further. For example, it might tell you the location of an over-aimed hit, which is often seen in relax hackers.

![](https://puu.sh/qaRdJ/5ec2d47a27.png)


# How to use

In all instances, you start by navigating command prompt towards the .exe, typing the executable name followed up by arguments. Arguments will be used to tell the program what you want it to do.

Example usage: > osuDodgyMomentsFinder.exe -ia

###Argument formats: 
**single replay** Argument "-i", the program will ask you to pick a replay/beatmap combo from the same folder as the executable.

**analyze all replays in folder** Argument "-ia". A FullAnalysis.osi file will be created. Open it with notepad or another editor.

**compare replays** Arguments "-cr <replay1> <replay2> <..:>".

**compare all replays in folder** Argument "-c".

**calculate cursor speed info** Argument "-s". Program will ask you to pick a replay.
