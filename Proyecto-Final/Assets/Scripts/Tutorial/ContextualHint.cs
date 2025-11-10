using UnityEngine;

[CreateAssetMenu(fileName = "ContextualHint", menuName = "Tutorial/Contextual Hint")]
public class ContextualHint : ScriptableObject
{
    public string hintID;

    public string title;
    [TextArea(3, 8)]
    public string description;
}