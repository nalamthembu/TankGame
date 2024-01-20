using UnityEngine;

[CreateAssetMenu(fileName = "LoadingScreenTips", menuName = "Game/Loading Screen Tips")]
public class LoadingScreenTipsScriptable : ScriptableObject
{
    public string[] tips;

    public string GetRandomTip()
    {
        if (tips.Length <= 0)
        {
            Debug.LogError("There are no tips assigned!");
            return "FATAL ERROR";
        }

        return tips[Random.Range(0, tips.Length)];
    }
}