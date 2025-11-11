using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Step")]
public class TutorialStep : ScriptableObject
{
    public string stepID;
    public int stepOrder;

    [TextArea(3, 10)]
    public string instructionText;

    public TutorialObjectiveType objectiveType;
    public int requiredCount = 1;

    public float waitDuration = 3f;
}

public enum TutorialObjectiveType
{
    Move,
    Dig,
    Plant,
    Harvest,
    CastSpell,
    EnterHouse,
    UseRitualAltar,
    StartNight,
    DefeatEnemy,
    SurviveNight,
    OpenInventory,
    OpenCrafting,
    OpenRestoration,
    ExploreHouse,
    Wait
}