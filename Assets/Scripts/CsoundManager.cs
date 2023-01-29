using System.Collections;
using UnityEngine;

/// <summary>
/// This class loads the AudioClips of the tracks into Csound tables, and routes the output to audio buffers. 
/// These buffers can be selected in the inspector of CsoundUnityChild instances, and their AudioSource output will be filled with these buffers.
/// Audio analysis is performed by Csound, and freq and amp values can be retrieved with GetChannel("freq"+track id) and GetChannel("rms"+track id).
/// The csd can be updated to support any number of tracks.
/// </summary>
[RequireComponent(typeof(CsoundUnity))]
public class CsoundManager : MonoBehaviour
{
    CsoundUnity csound;
    [Tooltip("Prefabs with AudioTrack component")]
    [SerializeField] AudioTrack[] tracks;

    int currentTrack = 0;
    IEnumerator Start()
    {
        csound = GetComponent<CsoundUnity>();

        if (csound == null)
        {
            Debug.LogError($"Please add a CsoundUnity component to {this.name}");
            yield break;
        }

        while (!csound.IsInitialized)
            yield return null;

        yield return new WaitForSeconds(0.125f);

        var count = 0;
        // Load the AudioClip of every track
        // and save the samples in Csound tables
        foreach (var track in tracks)
        {
            if (track == null) { Debug.LogError("Track requires the AudioTrack Component"); yield break; }

            var clip = "audio/" + track.AudioClip.name;

            Debug.Log("loading clip " + clip);
            var samples = CsoundUnity.GetSamples(clip);
            Debug.Log("samples read: " + samples.Length);
            if (samples.Length > 0)
            {
                var nChan = track.AudioClip.channels;
                var tn = 900 + track.id;
                var res = csound.CreateTable(tn, samples);
                Debug.Log(res == 0 ? $"<color=green>Table {tn} created</color>" : $"<color=red>Error: Couldn't create Table {tn}</color>");
                // set the id of the track
                track.id = count;
                // start with disabled tracks
                track.Enable(false);
                count++;
            }
        }
    }

    /// <summary>
    /// Sets the world position of the current track
    /// </summary>
    /// <param name="location"></param>
    public void PlaceTrack(Vector3 location)
    {
        if (currentTrack < 0 || currentTrack >= tracks.Length)
        {
            foreach (var track in tracks)
                track.Enable(false);
            currentTrack = 0;
        }
        else
        {
            tracks[currentTrack].transform.position = location;
            tracks[currentTrack].Enable(true);
            currentTrack++;
        }
    }

    /// <summary>
    /// Enable/disable visibility and audio output of all the tracks
    /// </summary>
    /// <param name="enable"></param>
    public void EnableTracks(bool enable)
    {
        foreach (var track in tracks)
            track.Enable(enable);
        // reset the currentTrack
        currentTrack = 0;
    }

    /// <summary>
    /// Enable/disable the debug for all the tracks
    /// </summary>
    /// <param name="enable"></param>
    public void EnableTracksDebug(bool enable)
    {
        foreach (var track in tracks)
            track.ShowDebug(enable);
    }

    void Update()
    {
        foreach (var track in tracks)
        {
            if (track.IsEnabled)
            {
                //set amplitude and frequency gathering data from csound channels
                track.SetAmplitude((float)csound.GetChannel("rms" + track.id));
                track.SetFrequency((float)csound.GetChannel("freq" + track.id));
            }
        }
    }
}
