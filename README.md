# CsoundUnity AR Samples

A collection of AR samples for CsoundUnity

Tested on Android 64bit

## Analyse
This scene performs the analysis of 4 audio tracks and uses the retrieved frequency and rms amplitude to drive VFXs or animations.  
After building, frame the floor or any flat surface. Tap a surface to place an audio track, it will start playing. After placing all 4 tracks, the next tap will remove them and you can start again.   
VFXs are very basic, but it should be enough to get started implementing your own effects.

The Csound GameObject contains a CsoundManager, where you can set the tracks that you want to analyse.
All the tracks must have exactly the same length in samples to be able to read them all correctly. The same phasor reads all the tables.
 
At the moment the Analyse csd supports 4 tracks, but any number of tracks can be set.
The second track is an example of a track that has only the amplitude analysed, using a different iUpdate value than the other 3 tracks.
When iUpdate is 0 the "Analyse" opcode doesn't perform the frequency analysis.
 
You can change the analysis parameters during playmode, the analysis will be reinitialised.
Be sure to grab legal values for the Low/High settings for each track (the minimum and maximum frequency in octave format), otherwise the analysis will fail for that track logging "illegal lo-hi values"

## AnalyseAndRecord
This scene allows you to record the entire analysis of the Analyse scene in a text file that can be used instead of having to do the analysis on every run, to save resources.
There's no need to build this scene, you can perform the computation on the editor.  
Code for reading the txt file is not provided here.