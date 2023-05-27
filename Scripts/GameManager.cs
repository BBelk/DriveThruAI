using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
//Not really a game but whatever
{
    // you must supply your own API Keys
    //openAI https://platform.openai.com/account/api-keys
    //elevenlabs https://docs.elevenlabs.io/welcome/introduction
    public string OpenAIAPIKey;
    public string ElevenLabsAPIKey;
    private int minFreq;    
    private int maxFreq;
    private AudioSource goAudioSource;    
    public AudioClip userRecordedClip;
    public OpenAIManager OpenAIManager;
    public ElevenLabsManager ElevenLabsManager;
    public UIManager UIManager;
    public bool isRecording;
     
    void Start(){   
        if(String.IsNullOrWhiteSpace(OpenAIAPIKey)){UIManager.ErrorText("YOU MUST PROVIDE YOUR OWN API KEY FOR OPENAI IN GAMEMANAGER.CS");UnityEngine.Debug.Log("YOU MUST PROVIDE YOUR OWN API KEY FOR OPENAI IN GAMEMANAGER.CS");}
        if(String.IsNullOrWhiteSpace(ElevenLabsAPIKey)){UIManager.ErrorText("YOU MUST PROVIDE YOUR OWN API KEY FOR ELEVENLABS IN GAMEMANAGER.CS");UnityEngine.Debug.Log("YOU MUST PROVIDE YOUR OWN API KEY FOR ELEVENLABS IN GAMEMANAGER.CS");}
        UIManager.ResetChatText();
        UIManager.ResetSpeedTest();
        #if !UNITY_WEBGL
        if(Microphone.devices.Length <= 0){    
            UIManager.ErrorText("Microphone not connected!");
        }    
        else {
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);    
            if(minFreq == 0 && maxFreq == 0){ 
                maxFreq = 44100;    
            }      
            goAudioSource = this.GetComponent<AudioSource>();    
        }    
        #endif
    }

    public void SetAudioClipAndPlay(AudioClip newAudioClip){
        goAudioSource.clip = newAudioClip;
        goAudioSource.Play();
    }

     public void Test(){
        UnityEngine.Debug.Log("TEST");
    }

    public void Reset(int resetIndex){
        // 0 instruct, 1 menu, 2 chatlog
        if(resetIndex == 2){
            OpenAIManager.ResetAllChatMessages();
            UIManager.ResetChatText();
            return;
        }
        if(resetIndex != 2){
            UIManager.ResetToDefault(resetIndex);
            OpenAIManager.ResetAllChatMessages();
        }
    }

    public Toggle muteToggle;

    public void MuteToggle(){
        goAudioSource.mute = muteToggle.isOn;
    }

    public void RecordButton(){
        if(!isRecording){
            goAudioSource.Stop();
            isRecording = true;
            UIManager.SetRecordButtonText("Finish Recording");
            if(StopRecordCo != null){StopCoroutine(StopRecordCo);}
            StopRecordCo = StartCoroutine(StopRecordTimer());
            //
            #if !UNITY_WEBGL
            goAudioSource.clip = Microphone.Start(UIManager.micName, false, 20, UIManager.maxFreq);    
            #endif

            UIManager.ResetSpeedTest();
            return;
        }
        if(isRecording){
            FinishRecordButton();
            return;
        }
    }

    public void FinishRecordButton(){
        isRecording = false;
        if(StopRecordCo != null){StopCoroutine(StopRecordCo);}
        UIManager.SetRecordButtonText("Record");
        #if !UNITY_WEBGL
        Microphone.End(UIManager.micName);
        #endif 
        userRecordedClip = goAudioSource.clip;
        // OpenAIManager.StartTest(userRecordedClip);
        OpenAIManager.TestAudioTranscription(userRecordedClip);
    }

    public void AIGreetingButton(){
        UIManager.ResetSpeedTest();
        OpenAIManager.AddNewMessage(0, UIManager.defaultInstructIPs[2].text);
        // ElevenLabsManager.DoTextToSpeech(UIManager.defaultInstructIPs[2].text);
    }

    private Coroutine StopRecordCo;

    public IEnumerator StopRecordTimer(){
        yield return new WaitForSeconds(20f);
        FinishRecordButton();
    }

    public void DoSpeech(string newString){
        ElevenLabsManager.DoTextToSpeech(newString);
    }

    public Stopwatch Stopwatch;
        // 0 speech to text
        // 1 ai response
        // 2 text to speech
    public void StartStopwatch(int stopwatchIndex){
        Stopwatch = new Stopwatch();
        Stopwatch.Start();
        UIManager.DoThink(stopwatchIndex);
    }

    public void EndStopwatch(int stopwatchIndex){
        Stopwatch.Stop();
        var ms = Stopwatch.ElapsedMilliseconds;
        Stopwatch.Reset();
        UIManager.SetSpeedText(stopwatchIndex, ms);
    }

    public void StopAudio(){
        goAudioSource.Stop();
    }
}
