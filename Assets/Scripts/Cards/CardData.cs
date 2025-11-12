using UnityEngine;

namespace FirstRound
{
    /// <summary>
    /// ScriptableObject that stores card information
    /// </summary>
    [CreateAssetMenu(fileName = "NewCardData", menuName = "First Round/New Card Data", order = 1)]
    public class CardData : ScriptableObject
    {
        [Header("Card Identity")]
        public int id;  
        
        [Header("Visual")]
        public Sprite frontSprite;

        [Header("Optional Info")]
        [TextArea(2, 4)]
        public string description;  
    }
}

