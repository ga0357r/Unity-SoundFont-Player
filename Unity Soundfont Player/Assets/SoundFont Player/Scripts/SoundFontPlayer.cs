using UnityEngine;
using NAudio.SoundFont;
using System.IO;
using NAudio.Wave;
using NAudio.Midi;
using System.Threading;

public class SoundFontPlayer : MonoBehaviour
{
    private SoundFont soundfont;
    [SerializeField] private string soundfontPath;
    private MidiOut midiOut;

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
        StopInstrument();

        // Create a new MIDI output device
        midiOut = new MidiOut(0);

        // Select the desired instrument using the MIDI program change message
        // we can use the value 192 directly to represent the program change status byte.
        //int programChangeStatus = 192 + instrumentIndex;
        //var programChangeMessage = new MidiMessage(programChangeStatus, 0, 0);
        //midiOut.Send(programChangeMessage.RawData);

        var instrument = soundfont.Instruments[instrumentIndex];

        // Iterate over the samples in the instrument
        foreach (var zone in instrument.Zones)
        {
            foreach (var generator in zone.Generators)
            {
                if (generator.GeneratorType == GeneratorEnum.SampleID)
                {
                    var sampleIndex = generator.UInt16Amount;

                    // Play a note using the MIDI note on message
                    var noteOnMessage = new MidiMessage((int)MidiCommandCode.NoteOn, sampleIndex, 100);
                    midiOut.Send(noteOnMessage.RawData);

                    // Wait for a short duration to allow the sample to play
                    Thread.Sleep(500); // Adjust the duration as needed

                    // Send the corresponding note off message to stop the sample
                    var noteOffMessage = new MidiMessage((int)MidiCommandCode.NoteOff, sampleIndex, 0);
                    midiOut.Send(noteOffMessage.RawData);
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
}
