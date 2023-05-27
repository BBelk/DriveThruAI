using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElevenLabs;
using ElevenLabs.Voices;
using UnityEngine;

public class ElevenLabsManager : MonoBehaviour
{

    public GameManager GameManager;
    public OpenAIManager OpenAIManager;
    private string apiKey = "";

    public List<Voice> allVoices;
    public Voice chosenVoice;
    // Start is called before the first frame update
    void Start()
    {
        apiKey = GameManager.ElevenLabsAPIKey;
        GetVoiceOptions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task GetVoiceOptions(){
        var api = new ElevenLabsClient(apiKey);
        var voices = (await api.VoicesEndpoint.GetAllVoicesAsync());
        GameManager.UIManager.VoiceSetDropdownOptions((List<Voice>)voices);
    }

    public async Task DoTextToSpeech(string newTextString){
        GameManager.StartStopwatch(2);
        var api = new ElevenLabsClient(apiKey);
        var text = newTextString;
        var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
        var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
        var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, chosenVoice, defaultVoiceSettings);
        // Debug.Log(clipPath);
        GameManager.EndStopwatch(2);
        GameManager.SetAudioClipAndPlay(audioClip);
    }

    public void SetVoice(Voice newVoice){
        chosenVoice = newVoice;
    }
}
