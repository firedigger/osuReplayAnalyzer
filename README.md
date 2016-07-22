# osuReplayAnalyzer

This is an osu! anticheat based on heuristic analysis. It analyzes a player's replay and points to specific moments that should be investigated further. For example, it might tell you the location of an over-aimed hit, which is often seen in relax hackers.

![](https://puu.sh/qaRdJ/5ec2d47a27.png)


# How to use

There are two methods of using the program.

### 1st method
Place the beatmap (.osu) file and the replay (.osr) both in the same folder as the osuReplayAnalyzer program.
Run the program and select the corresponding beatmap/replay. The names of the files must match the osu! standard.

### 2nd method
Run the program using specific command line arguments. The first argument will be the path to the beatmap, and the second the path to the given replay.


### Result
You will receive a list of all suspicious moments the program has found. The information currently given by the program is their location in the beatmap, more precisely, which time they appear at. You can use the osu! beatmap editor or an external replay viewer to find that specific moment.
