using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpenAI;
using UnityEngine;

public class MemoryManager : MonoBehaviour
{
    private string memoryFilePath;
    [System.Serializable]
    public class MemoryData
    {
        public List<ChatMessage> memories;

        public MemoryData(List<ChatMessage> memories)
        {
            this.memories = memories;
        }
    }
    
    string memoryPrompt = $"This is player's answer to the journal entries last time." +
                          $"This information is provided to give you a context about the earlier discussion." +
                          $"During the conversation, whenever similar topics show up, naturally and adequately integrate" +
                          $"these memories in the conversation.";
    private void Awake()
    {
        memoryFilePath = Path.Combine(Application.dataPath, "memories.json");
    }

    public void SaveMemories(List<ChatMessage> memories)
    {
        string json = JsonUtility.ToJson(new MemoryData(memories));
        File.WriteAllText(memoryFilePath, json);
        Debug.Log($"[MemoryManager] Memories saved to {memoryFilePath}");
    }
    
    public List<ChatMessage> LoadMemories()
    {
        List<ChatMessage> loadedMemories = new List<ChatMessage>();

        if (File.Exists(memoryFilePath))
        {
            string json = File.ReadAllText(memoryFilePath);
            MemoryData data = JsonUtility.FromJson<MemoryData>(json);
            loadedMemories = data.memories;
            Debug.Log($"[MemoryManager] Memories loaded from {memoryFilePath}");
        }
        else
        {
            Debug.LogWarning("[MemoryManager] No memory file found. Starting fresh.");
        }

        if (loadedMemories.Count > 0)
        {
            ChatMessage memoryContext = new ChatMessage { Content = memoryPrompt, Role = "system" };
            loadedMemories.Insert(0, memoryContext);
            Debug.Log("[MemoryManager] Memory prompt integrated.");
        }

        return loadedMemories;
    }
}
