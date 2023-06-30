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
}
#endif