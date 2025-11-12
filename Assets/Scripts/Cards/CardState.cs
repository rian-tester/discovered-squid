namespace FirstRound
{
    /// <summary>
    /// Defines all possible states a card can be in during gameplay
    /// </summary>
    public enum CardState
    {
        /// <summary>Card is showing back side</summary>
        FaceDown,
        /// <summary>Card is currently animating</summary>
        Flipping,
        /// <summary>Card is showing front side</summary>
        FaceUp,
        /// <summary>Card has been matched and is waiting to disappear</summary>
        Matched,
        /// <summary>Card has been removed from play</summary>
        Disappeared   
    }
}