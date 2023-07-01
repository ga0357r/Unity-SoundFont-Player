using UnityEngine;
using NAudio.SoundFont;
using System.IO;
using NAudio.Wave;
using NAudio.Midi;
using System.Threading;

public class SoundFontPlayer : MonoBehaviour
{
    [SerializeField] private string soundfontPath;
    [SerializeField] private AudioSource audioSource;

    private SoundFont soundfont;
    private MidiOut midiOut;
    private AudioClip audioClip;

    public SoundFont Soundfont => soundfont;


    private void Start()
    {
        //LoadSoundFont();
    }

    public void LoadSoundFont()
    {
        if (string.IsNullOrEmpty(soundfontPath) || !File.Exists(soundfontPath)) 
        {
            Debug.LogError("Could not find SoundFont File");
            return;
        }

        //load the SoundFont file
        soundfont = new SoundFont(soundfontPath);

        // Display the available instruments and their associated samples
        foreach (var instrument in soundfont.Instruments)
        {
            Debug.Log("Instrument Name: " + instrument.Name);

            foreach (var zone in instrument.Zones)
            {
                foreach (var generator in zone.Generators)
                {
                    if (generator.GeneratorType == GeneratorEnum.SampleID)
                    {
                        var sampleIndex = generator.UInt16Amount;
                        var sample = soundfont.SampleHeaders[sampleIndex];
                        Debug.Log("Sample Name: " + sample.SampleName);
                        break;  // Assuming only one SampleID generator per zone
                    }
                }
            }
        }
    }

    public void PlayInstrument(int instrumentIndex)
    {
        if(soundfont == null)
        {
            Debug.LogError(" Cannot Play Instrument From null SoundFont data");
            return;
        }

        if (instrumentIndex < 0 || instrumentIndex >= soundfont.Instruments.Length)
        {
            Debug.LogError("Invalid instrument index.");
            return;
        }

        //Stop Playing existing Instrument first
        //StopInstrument();

        // Create a new MIDI output device
        //midiOut = new MidiOut(0);

        var instrument = soundfont.Instruments[instrumentIndex];

        // Iterate over the samples in the instrument
        foreach (var zone in instrument.Zones)
        {
            foreach (var generator in zone.Generators)
            {
                if (generator.GeneratorType == GeneratorEnum.SampleID)
                {
                    var sampleIndex = generator.UInt16Amount;
                    var sample = soundfont.SampleHeaders[sampleIndex];

                    // Calculate the note frequency based on the sample's original pitch and pitch correction
                    var frequency = CalculateNoteFrequency(sample.OriginalPitch, sample.PitchCorrection);

                    // Set the desired buffer size and sample rate for the audio output
                    int sampleRate = (int)sample.SampleRate;

                    // Create an AudioClip to hold the sample data
                    audioClip = AudioClip.Create(sample.SampleName, (int)(sample.End - sample.Start), (int)sample.SFSampleLink, sampleRate, false);

                    // Copy the sample data to the AudioClip
                    byte sampleData = soundfont.SampleData[sampleIndex];

                    // Convert byte to float and Normalize byte value to float range (0 to 1)
                    float floatData = sampleData / 255f; 

                    audioClip.SetData(new float[] { floatData }, 0);

                    // Create a new AudioSource and play the AudioClip
                    audioSource.clip = audioClip;
                    audioSource.pitch = 3;
                    audioSource.Play();
                    return;


                    // Wait for the sample to finish playing
                    //Thread.Sleep((int)((sample.End - sample.Start) * 1000 / sampleRate));

                    //Destroy the AudioSource and AudioClip
                    //audioSource.Stop();
                    //audioSource.clip = null;
                    //audioClip = null;
                }
            }
        }
    }

    public void SetSoundfontPath(string newSoundfontPath)
    {
        soundfontPath = newSoundfontPath;
    }

    public void StopInstrument()
    {
        if (midiOut != null)
        {
            // Stop all notes and release the MIDI output device
            midiOut.Reset();
            midiOut.Dispose();
            midiOut = null;
        }
    }

    public float CalculateNoteFrequency(in int originalPitch, in int pitchCorrection)
    {
        // A4 is MIDI note number 69 with a frequency of 440 Hz
        const float a4Frequency = 440f;
        const int a4MidiNote = 69;

        // Calculate the semitone difference from A4
        int semitoneDifference = originalPitch - a4MidiNote;

        //there are 12 semitones in an octave
        const float octaveIncrement = 12f;

        // Calculate the pitch factor based on the semitone difference and pitch correction
        //Pitch ratio is 2^(n/12)
        float pitchFactor = Mathf.Pow(2f, (semitoneDifference + pitchCorrection) / octaveIncrement);

        // Calculate the note frequency
        float noteFrequency = a4Frequency * pitchFactor;

        return noteFrequency;
    }
}
