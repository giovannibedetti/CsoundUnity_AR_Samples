<Cabbage> bounds(0, 0, 0, 0)
form caption("Analyser") size(600, 350)

hslider bounds(5, 5, 300, 30), channel("iUpdate") range(0.001, 0.5, 0.01, 1, 0.001) valueTextBox(1) text("Update")
hslider bounds(5, 30, 300, 30), channel("kGain") range(0.001, 5, .8, 1, 0.001) valueTextBox(1) text("Gain") popupText("Gain")
label bounds(5, 60, 100, 20), channel("pianoLabel"), text("PIANO")
rslider bounds(5, 85, 70, 70), channel("kLow0") range(4, 13, 8, 1, 0.001) valueTextBox(1) text("Low 0") popupText("Low 0")
rslider bounds(75, 85, 70, 70), channel("kHi0") range(4, 13, 9, 1, 0.001) valueTextBox(1) text("High 0") popupText("High 0")
rslider bounds(5, 165, 70, 70), channel("kFiltFreq0") range(0, 22000, 2000, 0.5, 0.001) valueTextBox(1) text("FilterFreq 0") popupText("FilterFreq 0")
rslider bounds(75, 165, 70, 70), channel("kThresh0") range(0, 140, 30, 1, 0.001) valueTextBox(1) text("Thresh 0") popupText("Thresh 0")
rslider bounds(5, 245, 70, 70), channel("kFreqs0") range(1, 120, 9, 1, 1) valueTextBox(1) text("Freqs 0") popupText("Freqs 0")
rslider bounds(75, 245, 70, 70), channel("kConf0") range(4, 13, 9, 1, 0.001) valueTextBox(1) text("Conf 0") popupText("Conf 0")
label bounds(160, 60, 100, 20), channel("bassLabel"), text ("BASS")
rslider bounds(170, 85, 70, 70), channel("kLow2") range(4, 13, 8, 1, 0.001) valueTextBox(1) text("Low 2") popupText("Low 2")
rslider bounds(240, 85, 70, 70), channel("kHi2") range(4, 13, 9, 1, 0.001) valueTextBox(1) text("High 2") popupText("High 2")
rslider bounds(170, 165, 70, 70), channel("kFiltFreq2") range(0, 22000, 1000, 0.5, 0.001) valueTextBox(1) text("FilterFreq 2") popupText("FilterFreq 2")
rslider bounds(240, 165, 70, 70), channel("kThresh2") range(0, 140, 30, 1, 0.001) valueTextBox(1) text("Thresh 2") popupText("Thresh 2")
rslider bounds(170, 245, 70, 70), channel("kFreqs2") range(1, 120, 9, 1, 1) valueTextBox(1) text("Freqs 2") popupText("Low 2") 
rslider bounds(240, 245, 70, 70), channel("kConf2") range(4, 13, 9, 1, 0.001) valueTextBox(1) text("Conf 2") popupText("Low 2")
label bounds(315, 60, 100, 20), channel("leadLabel"), text ("LEAD")
rslider bounds(325, 85, 70, 70), channel("kLow3") range(4, 13, 8, 1, 0.001) valueTextBox(1) text("Low 3") popupText("Low 3")
rslider bounds(395, 85, 70, 70), channel("kHi3") range(4, 13, 9, 1, 0.001) valueTextBox(1) text("High 3") popupText("High 3")
rslider bounds(325, 165, 70, 70), channel("kFiltFreq3") range(0, 22000, 2000, 0.5, 0.001) valueTextBox(1) text("FilterFreq 3") popupText("FilterFreq 3")
rslider bounds(395, 165, 70, 70), channel("kThresh3") range(0, 140, 30, 1, 0.001) valueTextBox(1) text("Thresh 3") popupText("Thresh 3")
rslider bounds(325, 245, 70, 70), channel("kFreqs3") range(1, 120, 9, 1, 1) valueTextBox(1) text("Freqs 3") popupText("Freqs 3") 
rslider bounds(395, 245, 70, 70), channel("kConf3") range(4, 13, 9, 1, 0.001) valueTextBox(1) text("Conf 3") popupText("Conf 3")

</Cabbage>
<CsoundSynthesizer>
<CsOptions>
-n -d
</CsOptions>
<CsInstruments>
sr = 48000
ksmps = 32
nchnls = 1
0dbfs = 1

; -------------------------
; OPCODES
; -------------------------

opcode Read, a, ai

aPhs, iNumTable xin
; use the phasor to scan the tables
aRead table aPhs, 900 + iNumTable, 1
xout aRead

endop

opcode Analyse, a, Sikkkkkkkk
SChan, iNumTable, kFiltFreq, kUpdate, kLow, kHigh, kThresh, kFreqs, kConf, kStart xin 

prints "Analyse %d", iNumTable
aRead chnget SChan
; lowpass files to achieve better tracking
aTone tone aRead, kFiltFreq

; kFiltFreq:        cutoff frequency of the lowpass filter applied to input, to improve tracking
; kLow - kHigh:     range in which pitch is detected, expressed in octave point decimal
; kThresh:          amplitude, expressed in decibels, necessary for the pitch to be detected. Once started it continues until it is 6 dB down.
; kFreqs:           number of divisons of an octave. Default is 12 and is limited to 120.
; kConf:            the number of conformations needed for an octave jump. Default is 10.
; kStart:           starting pitch for tracker. Default value is (ilo + ihi)/2.

kOct, kAmp init 0.0

kChanged changed2 kFiltFreq, kUpdate, kLow, kHigh, kThresh, kFreqs, kConf, kStart

if kChanged == 1 then
    printks "Changed track %d: low: %f, hi: %f\n", 0, iNumTable, kLow, kHigh
    reinit initialization
endif

initialization:

iFilt = i(kFiltFreq)
iUpdate = i(kUpdate)
iLow = i(kLow)
iHigh = i(kHigh)
iThresh = i(kThresh)
iFreqs = i(kFreqs)
iConf = i(kConf)
iStart = i(kStart)

; only perform the pitch analysis if iUpdate is above 0
if iUpdate > 0 then

    prints "analysing table %d, update %f\n", iNumTable, iUpdate
    prints "update: %f, low: %f, high: %f, thresh: %f, freq: %f, conf: %f, start: %f\n\n", iUpdate, iLow, iHigh, iThresh, iFreqs, iConf, iStart

        ; use the 'pitch' opcode to get an estimate value for the signal frequency (in octave form) and amplitude 
    kOct, kAmp pitch aTone, iUpdate, iLow, iHigh, iThresh, iFreqs, iConf, iStart
    ;endif
endif

rireturn

; use the 'rms' opcode to get the root-mean-square amplitude of the signal (more usable for graphic representations)
kRMS rms aRead

; write the returned values into channels
chnset kOct, sprintf("freq%d", iNumTable) 
chnset kAmp, sprintf("amp%d", iNumTable)  
chnset kRMS, sprintf("rms%d", iNumTable)

; output the read audio file
xout aRead

endop


; -------------------------
; INSTRUMENTS
; -------------------------

instr 1

; read tables, they all have the same length
iLen = ftlen(900)
; so the reading phasor can be the same for all
aPhs phasor (sr/iLen)
kPhs = k(aPhs)
kGain       init 0.8
kGain       chnget "kGain"

aRead0 Read aPhs, 0
aRead1 Read aPhs, 1
aRead2 Read aPhs, 2
aRead3 Read aPhs, 3

chnset aRead0 * kGain, "track0"
chnset aRead1 * kGain, "track1"
chnset aRead2 * kGain, "track2"
chnset aRead3 * kGain, "track3"

; set a 'progress' channel to be read in Unity
chnset kPhs, "progress"

kUpdate     init 0.001
kUpdate     chnget "iUpdate"

; PIANO SETTINGS
iTab0       init 0
kLow0       init 9.0
kLow0       chnget "kLow0"      
kHi0        init 10.0
kHi0        chnget "kHi0"       
kThresh0    init 30
kThresh0    chnget "kThresh0"
kFreqs0     init 12
kFreqs0     chnget "kFreqs0"    
kConf0      init 10
kConf0      chnget "kConf0"     
kStart0     = (kLow0 + kHi0) / 2
kFilt0      init 2000
kFilt0      chnget "kFiltFreq0" 

; DRUMS SETTINGS (not really needed)
iTab1       init 1
kNoUpdate   init 0
kLow1       init 0
kHi1        init 0
kThresh1    init 0
kFreqs1     init 0
kConf1      init 0
kStart1     init 0
kFilt1      init 0

; BASS SETTINGS
iTab2       init 2
kLow2       init 8
kLow2       chnget "kLow2"     
kHi2        init 9
kHi2        chnget "kHi2"  
kThresh2    init 30
kThresh2    chnget "kThresh2"   
kFreqs2     init 12
kFreqs2     chnget "kFreqs2"  
kConf2      init 10
kConf2      chnget "kConf2"    
kStart2     = (kLow2 + kHi2) / 2
kFilt2      init 1000
kFilt2      chnget "kFiltFreq2" 

; LEAD SETTINGS
iTab3       init 3
kLow3       init 9.00
kLow3       chnget "kLow3" 
kHi3        init 10.0
kHi3        chnget "kHi3"      
kThresh3    init 30
kThresh3    chnget "kThresh3" 
kFreqs3     init 12
kFreqs3     chnget "kFreqs3"   
kConf3      init 10
kConf3      chnget "kConf3"    
kStart3     = (kLow3 + kHi3) / 2
kFilt3      init 3000
kFilt3      chnget "kFiltFreq3"

;                SChan,    TableNumber,     FilterFreq    UpdateRate,    LowFreqOct,     HiFreqOct,      DBThresh,       Freqs,      Conf,      Start       
a0 Analyse       "track0", iTab0,           kFilt0,       kUpdate,       kLow0,          kHi0,           kThresh0,       kFreqs0,    kConf0,    kStart0      ; PIANO
a1 Analyse       "track1", iTab1,           kFilt1,       kNoUpdate,     kLow1,          kHi1,           kThresh1,       kFreqs1,    kConf1,    kStart1      ; DRUMS (no pitch analysis, since it's useless)
a2 Analyse       "track2", iTab2,           kFilt2,       kUpdate,       kLow2,          kHi2,           kThresh2,       kFreqs2,    kConf2,    kStart2      ; BASS   
a3 Analyse       "track3", iTab3,           kFilt3,       kUpdate,       kLow3,          kHi3,           kThresh3,       kFreqs3,    kConf3,    kStart3      ; LEAD

endin

</CsInstruments>
<CsScore>
; Wait some seconds before starting the instruments, 
; so that Unity has some time to create the tables to be read
i1 3 z

; TEST FUNCTIONS
;f1 0 4096 10 1
;f900 0 4096 10 1
;f901 0 4096 10 1
;f902 0 4096 10 1
;f903 0 4096 10 1
</CsScore>
</CsoundSynthesizer>