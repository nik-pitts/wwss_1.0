using System;
using System.Collections;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_InputField message;
        [SerializeField] private Dropdown dropdown;
        
        private readonly string fileName = "output.wav";
        private readonly int duration = 30;
        
        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();

        private GamepadControl gamepadControl;
        private float recordingOnProgress;

        private void Awake()
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                gamepadControl = player.GetComponentInChildren<GamepadControl>();
            }        
        }

        private void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
            #endif
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }
        
        private void StartRecording()
        {
            if (isRecording) return;  // Prevent multiple recordings

            isRecording = true;
            message.text = "Recording on progress...";
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            
            #if !UNITY_WEBGL
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
            #endif
        }

        private async void EndRecording()
        {
            if (!isRecording) return;  // Prevent stopping when not recording

            isRecording = false;
            
            message.text = "Transcripting...";
            
            #if !UNITY_WEBGL
            Microphone.End(null);
            #endif
            
            byte[] data = SaveWav.Save(fileName, clip);
            
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            progressBar.fillAmount = 0;
            message.text = res.Text;
            StartCoroutine(DelayedOnEndEdit());
        }
        
        private IEnumerator DelayedOnEndEdit()
        {
            yield return new WaitForSeconds(0.5f);
            message.onEndEdit.Invoke(message.text);
        }

        private void Update()
        {
            recordingOnProgress = gamepadControl.recordingOnProgress;

            // Start recording when recordingOnProgress is TRUE
            if (recordingOnProgress > 0.5f && !isRecording)
            {
                StartRecording();
            }

            // Stop recording when recordingOnProgress is FALSE
            if (recordingOnProgress <= 0.5f && isRecording)
            {
                EndRecording();
                time = 0;
            }

            // Update progress bar while recording
            if (isRecording)
            {
                time += Time.deltaTime;
                progressBar.fillAmount = time / duration;
            }
        }
    }
}
