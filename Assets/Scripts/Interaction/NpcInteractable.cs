using UnityEngine;

public class NpcInteractable : Interactable
{
    [Header("NPC")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField, TextArea(2, 5)] private string dialogueText = "你好。";

    protected override void OnInteract(Player player)
    {
        Debug.Log(npcName + ": " + dialogueText);
    }
}
