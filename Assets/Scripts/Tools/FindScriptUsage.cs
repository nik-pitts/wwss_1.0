using UnityEngine;
using UnityEditor;

public class FindScriptUsage
{
    [MenuItem("Tools/Find Script Usage")]
    static void FindAllObjectsWithScript()
    {
        string scriptName = "OpenAIApi"; // 改成你要找的类名
        var all = GameObject.FindObjectsOfType<MonoBehaviour>();
        foreach (var mb in all)
        {
            if (mb.GetType().Name == scriptName)
            {
                Debug.Log($"Found on: {mb.gameObject.name}", mb.gameObject);
            }
        }

        Debug.LogWarning($"Found no class named {scriptName}.cs");
    }
}
