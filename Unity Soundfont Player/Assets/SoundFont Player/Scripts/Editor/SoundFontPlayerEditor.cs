using System.IO;
using NAudio.SoundFont;
using NAudio.Wave;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(SoundFontPlayer))]
public class SoundFontPlayerEditor : Editor
{
    private int selectedInstrumentIndex = -1;

    public override void OnInspectorGUI()
    {
        SoundFontPlayer soundFontPlayer = (SoundFontPlayer)target;
        // Draw the default inspector GUI
        DrawDefaultInspector();
        LoadSoundFont(soundFontPlayer);
        BrowseInstrument(soundFontPlayer);
        PlayInstrument(soundFontPlayer);
        StopInstrument(soundFontPlayer);
    }

    private void LoadSoundFont(in SoundFontPlayer soundFontPlayer)
    {
        // Add the browse button
        if (GUILayout.Button("Load SoundFont"))
        {
            string filePath = EditorUtility.OpenFilePanel("Select SoundFont File", "", "sf2");
            if (!string.IsNullOrEmpty(filePath))
            {
                soundFontPlayer.SetSoundfontPath(filePath);
                soundFontPlayer.LoadSoundFont();
            }
        }
    }

    private void BrowseInstrument(in SoundFontPlayer soundFontPlayer)
    {
        if (soundFontPlayer.Soundfont == null) return;
        
        // Get the list of available instruments
        string[] instrumentNames = new string[soundFontPlayer.Soundfont.Instruments.Length];

        for (int i = 0; i < soundFontPlayer.Soundfont.Instruments.Length; i++)
        {
            instrumentNames[i] = soundFontPlayer.Soundfont.Instruments[i].Name;
        }

        // Show the instrument selection dropdown
        selectedInstrumentIndex = EditorGUILayout.Popup("Select Instrument", selectedInstrumentIndex, instrumentNames);
    }

    private void PlayInstrument(in SoundFontPlayer soundFontPlayer)
    {
        if (soundFontPlayer.Soundfont == null) return;

        // Add the play button for the selected instrument
        if (GUILayout.Button("Play Instrument"))
        {
            if (selectedInstrumentIndex >= 0 && selectedInstrumentIndex < soundFontPlayer.Soundfont.Instruments.Length)
            {
                soundFontPlayer.PlayInstrument(selectedInstrumentIndex);
            }
            else
            {
                Debug.LogError("Invalid instrument index.");
            }
        }
    }

    private void StopInstrument(in SoundFontPlayer soundFontPlayer)
    {
        if (soundFontPlayer.Soundfont == null) return;

        // Add the play button for the selected instrument
        if (GUILayout.Button("Stop Instrument"))
        { 
            soundFontPlayer.StopInstrument();  
        }
    }

    public void ExtractSamples(in SoundFontPlayer soundFontPlayer)
    {
        if (soundFontPlayer.Soundfont == null) return;
        
        //Create Extracted Soundfonts Directory
        string extractedSoundFontsPath = Application.dataPath + "/SoundFont Player" + "/Extracted SoundFonts";
        
        if(!Directory.Exists(extractedSoundFontsPath))
        {
            Directory.CreateDirectory(extractedSoundFontsPath);
        }
        
        // Create the output folder if it doesn't exist
        string outputFolder = extractedSoundFontsPath + soundFontPlayer.Soundfont.FileInfo.BankName;
        
        if(!Directory.Exists(extractedSoundFontsPath))
        {
            Directory.CreateDirectory(outputFolder);
        }
        

        // Iterate over each instrument in the SoundFont
        foreach (var instrument in soundFontPlayer.Soundfont.Instruments)
        {
            // Iterate over each sample in the instrument
            foreach (var zone in instrument.Zones)
            {
                foreach (var generator in zone.Generators)
                {
                    if (generator.GeneratorType == GeneratorEnum.SampleID)
                    {
                        var sampleIndex = generator.UInt16Amount;
                        var sample = soundFontPlayer.Soundfont.SampleHeaders[sampleIndex];

                        // Extract sample data as PCM audio
                        var audioData = soundFontPlayer.Soundfont.SampleData[sampleIndex];

                        // Create a WAV file path
                        var wavFilePath = Path.Combine(outputFolder, $"{instrument.Name}_{sample.SampleName}.wav");

                        using(var writer = new WaveFileWriter(wavFilePath, new WaveFormat((int)sample.SampleRate, (int)sample.SFSampleLink)))
                        {
                            //writer.Write(audioData, 0, audioData.Length);
                        }
                    }
                }
            }
        }
    }
}
#endif