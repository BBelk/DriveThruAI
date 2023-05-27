using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using OpenAI.Models;
using TMPro;
using OpenAI.Images;
using UnityEngine.Assertions;
using System;
using System.IO;
using UnityEngine.UI;
using OpenAI.Chat;
using System.Text.RegularExpressions;
using System.Linq;
// using Cysharp.Threading.Tasks;
using OpenAI.Moderations;
using UnityEngine.Analytics;
using OpenAI.Audio;
using System.Threading.Tasks;

public class OpenAIManager : MonoBehaviour
{
  public GameManager GameManager;
  public List<Message> allChatMessages;
    
  private string apiKey = "";
  private string orgId = "";

    // Start is called before the first frame update
    void Start()
    {
      apiKey = GameManager.OpenAIAPIKey;
      orgId = GameManager.OpenAIOrgID;
      Invoke("ResetAllChatMessages", 0.5f);
        
    }
    public void ResetAllChatMessages(){
      allChatMessages.Clear();
      var getInstructIP = GameManager.UIManager.defaultInstructIPs;
      foreach(TMP_InputField tmpip in getInstructIP){
        allChatMessages.Add(new Message(Role.System, tmpip.text));
      }
    }
    public OpenAIClient NewOpenAIClient(){
      return new OpenAIClient(new OpenAIAuthentication(apiKey, orgId));
    }

    public async Task TestAudioTranscription(AudioClip audioClip){
      GameManager.StartStopwatch(0);
      var api = NewOpenAIClient();
      var request = new AudioTranscriptionRequest(audioClip, language: "en");
      var result = await api.AudioEndpoint.CreateTranscriptionAsync(request);
      GameManager.EndStopwatch(0);
      UserMessage(result);
    }

    public void UserMessage(string newMessage){
      AddNewMessage(1, newMessage);
      TestChatCompletion();
    }
    

    public async Task TestChatCompletion(){
      var api = NewOpenAIClient();
      GameManager.StartStopwatch(1);
      var chatRequest = new ChatRequest(allChatMessages, Model.GPT3_5_Turbo);
      var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
      GameManager.EndStopwatch(1);
      AddNewMessage(0, result);   
    }

    public void AddNewMessage(int role, string messageString){
      var roleName = Role.Assistant;
      var tag = "<b>";
      if(role == 0){GameManager.DoSpeech(messageString);}
      if(role != 0){roleName = Role.User;tag = "<i>";}
      allChatMessages.Add(new Message(roleName, messageString));
      GameManager.UIManager.NewAutoGenText("" + tag + roleName.ToString(), messageString);

    }
    

}
