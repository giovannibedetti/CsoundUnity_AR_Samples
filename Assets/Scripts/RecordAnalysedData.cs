using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loads the tracks into Csound tables.
/// Records the analysed data getting channels 'rms' and 'freq' from Csound
/// The recording starts as soon as the scene is played, and ends when the end of the file is reached.
/// </summary>
public class RecordAnalysedData : MonoBehaviour
{
    CsoundUnity _csound;
    [Tooltip("Prefabs with AudioTrack component")]
    [SerializeField] AudioClip[] _tracks;
    [SerializeField] string _outputFileName;
    [SerializeField] Image _progressImage;
    [SerializeField] Text _infoText;

    private string _fullpath;
    bool _recording = true;

    IEnumerator Start()
    {
        _csound = GetComponent<CsoundUnity>();

        while (!_csound.IsInitialized)
        {
            yield return null;
        }

        var count = 0;
        foreach (var track in _tracks)
        {
            var clip = "audio/" + track.name;

            Debug.Log("loading clip " + clip);
            var samples = CsoundUnity.GetSamples(clip);
            Debug.Log("samples read: " + samples.Length);
            if (samples.Length > 0)
            {
                var nChan = track.channels;
                var tn = 900 + count;
                var res = _csound.CreateTable(tn, samples);
                _csound.SetChannel($"sampletable{tn}", tn);
                Debug.Log(res == 0 ? $"<color=green>Table {tn} created</color>" : $"<color=red>Error: Couldn't create Table {tn}</color>");
                count++;
            }
        }

        var dir = Path.GetFullPath(Application.dataPath + "/../Recordings/");//Path.Combine(Application.dataPath, "/../Recordings/");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        _fullpath = Path.Combine(dir, _outputFileName);
        Debug.Log("Recording analysis data in file: " + _fullpath);
        var header = "PROGRESS,TRACK,RMS,FREQ\n";
        File.AppendAllText(_fullpath, header);
        _infoText.text = "Recording...";
        yield return Recording();
    }

    IEnumerator Recording()
    {
        while(_recording)
        {
            var progress = (float)_csound.GetChannel("progress");

            _progressImage.fillAmount = progress;

            // TODO
            // the progress value depends on the length of the audio file, and on the project sampling rate (number of samples in a second).
            // CsoundUnity reads the samples of the audio file at the sampling rate (speed of reading is SR/Length) and fills the progress channel (from 0 to 1) . 
            // When the file ends it starts again from the beginning (looped), so in fact this value never reaches 1
            // we need to take the latest value before having 0 (the beginning) again
            if (progress >= 0.992f || !Application.isPlaying)
            {
                _recording = false;
                _infoText.text = "Finished!";
            }
            else
            {
                var count = 0;
                var text = "";
                foreach (var track in _tracks)
                {
                    var rms = (float)_csound.GetChannel("rms" + count);
                    var freq = (float)_csound.GetChannel("freq" + count);
                    text += $"{progress},{count},{rms},{freq}\n";
                    // Debug.Log(text);
                    count++;
                }
                File.AppendAllText(_fullpath, text);
            }
            yield return null;
        }
    }

    void Update()
    {
        //if (_recording)
        //{
        //    var progress = (float)_csound.GetChannel("progress");
        //    // TODO
        //    // the progress value depends on the length of the audio file, and on the project sampling rate (number of samples in a second).
        //    // CsoundUnity reads the samples of the audio file at the sampling rate (speed of reading is SR/Length) and fills the progress channel (from 0 to 1) . 
        //    // When the file ends it starts again from the beginning (looped), so in fact this value never reaches 1
        //    // we need to take the latest value before having 0 (the beginning) again
        //    if (progress >= 0.992f || !Application.isPlaying)
        //    {
        //        _recording = false;
        //    }
        //    else
        //    {
        //        var count = 0;
        //        var text = "";
        //        foreach (var track in _tracks)
        //        {
        //            var rms = (float)_csound.GetChannel("rms" + count);
        //            var freq = (float)_csound.GetChannel("freq" + count);
        //            text += $"{progress},{count},{rms},{freq}\n";
        //            // Debug.Log(text);
        //            count++;
        //        }
        //        File.AppendAllText(_fullpath, text);
        //    }
        //}
    }
}
