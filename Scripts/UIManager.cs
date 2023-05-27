using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ElevenLabs.Voices;

public class UIManager : MonoBehaviour
{
    public GameManager GameManager;

    public TMP_Text errorTMPText;
    public TMP_Dropdown microphoneDropdown;
    public TMP_Dropdown voiceDropdown;
    public List<Voice> allVoices;
    public int maxFreq;
    public string micName;
    public GameObject textBox;
    public CanvasGroup userChatboxCG;

    public TMP_InputField userChatboxIP;
    public TMP_Text recordButtonText;

    public List<TMP_InputField> defaultInstructIPs;
    public List<string> defaultInstructStrings;

    public List<TMP_Text> allSpeedTestTMP_Text;
    public List<long> allSpeedLongs;

    private void OnApplicationQuit() {
        ResetChatText();    
    }
    public void ErrorText(string newText){
        errorTMPText.text = newText;
        if(ClearErrorTextCo != null){StopCoroutine(ClearErrorTextCo);}
        ClearErrorTextCo = StartCoroutine(ClearErrorText());
    }
    private Coroutine ClearErrorTextCo;
    public IEnumerator ClearErrorText(){
        yield return new WaitForSeconds(3f);
        errorTMPText.text = "";
        StopCoroutine(ClearErrorTextCo);
    }
    public Button recordButton;
    public void SetRecordButtonText(string newText){
        recordButtonText.text = newText;
    }

    void Start(){
        foreach(TMP_InputField tmpip in defaultInstructIPs){
            defaultInstructStrings.Add(tmpip.text);
        }
        allDefaultTMP_Objects[0].SetActive(false);
        #if !UNITY_WEBGL
        if(Microphone.devices.Length <= 0){
            errorTMPText.text = "ERROR: No Microphone detected. Connect microphone and restart program";
            microphoneDropdown.interactable = false;
            recordButton.interactable = false;
            return;
        }
        var newList = new List<string>();
        foreach(string newD in Microphone.devices){
            newList.Add(newD);
        }
        SetDropdownOptions(newList);
        #endif
    }

    public void SetDropdownOptions(List<string> newOptionStrings){
        var newList = new List<TMP_Dropdown.OptionData>();
        foreach(string newString in newOptionStrings){
            newList.Add(new TMP_Dropdown.OptionData(){text = newString});
        }
        microphoneDropdown.options = newList;
        SetValue();
    }

    public void SetValue(){
        #if !UNITY_WEBGL
        var newValue = microphoneDropdown.value;
        micName = microphoneDropdown.options[newValue].text.ToString();
        var minFreq = 0;
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);    
            if(minFreq == 0 && maxFreq == 0){ 
                maxFreq = 44100;    
            }
            #endif
    }
    
    public void VoiceSetDropdownOptions(List<Voice> newVoiceList){
        allVoices = newVoiceList;
        var newList = new List<TMP_Dropdown.OptionData>();
        foreach(Voice newVoice in allVoices){
            newList.Add(new TMP_Dropdown.OptionData(){text = newVoice.Name});
        }
        voiceDropdown.options = newList;
        VoiceSetValue();
    }

    public void VoiceSetValue(){
        var newValue = voiceDropdown.value;
        var chosenVoice = allVoices[newValue];
        GameManager.ElevenLabsManager.SetVoice(chosenVoice);
    }
    
    public ScrollRect chatScrollRect;
    public Transform contentTransform;
    public List<GameObject> allDefaultTMP_Objects;
    public List<GameObject> allAutoGenTextObjects;
    public void NewAutoGenText(string roleName, string newText){
        var newObj = Instantiate(allDefaultTMP_Objects[0], contentTransform);
         newObj.GetComponent<TMP_Text>().text = roleName + ": " + newText;
        newObj.gameObject.SetActive(true);
        chatScrollRect.verticalNormalizedPosition = 0f;
        allAutoGenTextObjects.Add(newObj);
    }

    public void SubmitChatbox(){
        ResetSpeedTest();
        if(String.IsNullOrWhiteSpace(userChatboxIP.text)){return;}
        GameManager.OpenAIManager.UserMessage(userChatboxIP.text);
        userChatboxIP.text = "";
        GameManager.StopAudio();
    }

    public void ResetChatText(){
        foreach(GameObject newObj in allAutoGenTextObjects){
            Destroy(newObj);
        }
        allAutoGenTextObjects.Clear();
    }

    public void ResetToDefault(int instructIndex){
        defaultInstructIPs[instructIndex].text = defaultInstructStrings[instructIndex];
        ResetChatText();
    }

    public void ResetSpeedTest(){
        if(ThinkCo != null){StopCoroutine(ThinkCo);ThinkCo = null;}
        foreach(TMP_Text newText in allSpeedTestTMP_Text){
            newText.text = "";
        }
        allSpeedLongs.Clear();
    }

    public void SetSpeedText(int speedIndex, long newText){
        if(ThinkCo != null){StopCoroutine(ThinkCo);ThinkCo = null;}
        allSpeedTestTMP_Text[speedIndex].text = "" + newText + "ms";
        allSpeedLongs.Add(newText);
        if(speedIndex == 2){
            var total = (long) 0;
            foreach(long newL in allSpeedLongs){
                total += newL;
            }
        allSpeedTestTMP_Text[3].text = "" + total + "ms";
        }
    }
    public float colorDelay = 0.2f;

    public Coroutine ThinkCo;
    public void DoThink(int textIndex){
        if(ThinkCo != null){StopCoroutine(ThinkCo);ThinkCo = null;}
        ThinkCo = StartCoroutine(AnimateDoThink(textIndex));
    }
    public IEnumerator AnimateDoThink(int textIndex){
    string originalText = ".  .  .";
    string[] animationSteps = new string[]
    {
        "<b>.  <color=white>.  .</color>",
        "<b>.  .  <color=white>.</color>",
        "<b>.  .  .",
        "<b><color=white>.</color>  .  .",
        "<b><color=white>.  .</color>  ."
    };

    while (true){
        for (int i = 0; i < animationSteps.Length; i++)
        {
            allSpeedTestTMP_Text[textIndex].text = animationSteps[i];
            yield return new WaitForSeconds(colorDelay);

            if (i == animationSteps.Length - 1)
            {
                i = -1;
            }
        }
    }
}


}
