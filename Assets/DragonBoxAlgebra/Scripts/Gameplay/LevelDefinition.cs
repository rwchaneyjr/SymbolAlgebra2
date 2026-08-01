using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    [Serializable]
    public class LevelDefinition
    {
        public string Title;
        public int CreatureTheme;
        public List<CardKind> LeftCards = new();
        public List<CardKind> RightCards = new();
        public List<int> LeftValues = new();
        public List<int> RightValues = new();
        public List<CardKind> HandCards = new();
        public List<int> HandValues = new();
        public List<int> HandVisualThemes = new();
        public List<int> LeftVisualThemes = new();
        public List<int> RightVisualThemes = new();
        public int ParMoves = 6;
        public int ParCards = 2;

        public LevelDefinition Clone()
        {
            return new LevelDefinition
            {
                Title = Title,
                CreatureTheme = CreatureTheme,
                LeftCards = new List<CardKind>(LeftCards),
                RightCards = new List<CardKind>(RightCards),
                LeftValues = new List<int>(LeftValues),
                RightValues = new List<int>(RightValues),
                HandCards = new List<CardKind>(HandCards),
                HandValues = new List<int>(HandValues),
                HandVisualThemes = new List<int>(HandVisualThemes),
                LeftVisualThemes = new List<int>(LeftVisualThemes),
                RightVisualThemes = new List<int>(RightVisualThemes),
                ParMoves = ParMoves,
                ParCards = ParCards
            };
        }

        public BoardSide BuildSide(List<CardKind> kinds, List<int> values, List<int> visualThemes = null)
        {
            var side = new BoardSide();
            for (int i = 0; i < kinds.Count; i++)
            {
                int value = values != null && i < values.Count ? values[i] : 1;
                int visualTheme = visualThemes != null && i < visualThemes.Count ? visualThemes[i] : -1;
                side.Cards.Add(new BoardCard(kinds[i], value, 1, visualTheme));
            }

            return side;
        }

        public List<BoardCard> BuildHand()
        {
            var hand = new List<BoardCard>();
            for (int i = 0; i < HandCards.Count; i++)
            {
                int value = HandValues != null && i < HandValues.Count ? HandValues[i] : 1;
                int visualTheme = HandVisualThemes != null && i < HandVisualThemes.Count
                    ? HandVisualThemes[i]
                    : -1;
                hand.Add(new BoardCard(HandCards[i], value, 1, visualTheme));
            }

            return hand;
        }
    }
}
