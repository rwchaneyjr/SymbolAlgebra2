using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class CardVisuals
    {
        public static Color HandFaceBackground(BoardCard card) =>
            CardFlipRules.IsLight(card)
                ? new Color(0.98f, 0.84f, 0.14f)
                : new Color(0.08f, 0.08f, 0.12f);

        public static Color HandFaceBorder(BoardCard card) =>
            CardFlipRules.IsLight(card)
                ? new Color(0.72f, 0.48f, 0.04f)
                : new Color(0.35f, 0.35f, 0.42f);

        public static Color Background(CardKind kind) => kind switch
        {
            CardKind.Box => new Color(0.78f, 0.28f, 0.22f),
            CardKind.DayCreature => new Color(0.35f, 0.78f, 0.95f),
            CardKind.NightCreature => new Color(0.22f, 0.28f, 0.55f),
            CardKind.PositiveConstant => new Color(0.72f, 0.88f, 0.55f),
            CardKind.NegativeConstant => new Color(0.48f, 0.52f, 0.68f),
            CardKind.One => new Color(0.95f, 0.95f, 0.88f),
            CardKind.DivideTool => new Color(0.95f, 0.55f, 0.18f),
            _ => Color.white
        };

        public static Color Border(CardKind kind) => kind switch
        {
            CardKind.Box => new Color(0.45f, 0.12f, 0.08f),
            CardKind.DayCreature => new Color(0.15f, 0.35f, 0.55f),
            CardKind.NightCreature => new Color(0.05f, 0.05f, 0.12f),
            CardKind.One => new Color(0.55f, 0.45f, 0.2f),
            CardKind.DivideTool => new Color(0.55f, 0.25f, 0.05f),
            _ => new Color(0.2f, 0.2f, 0.2f, 0.6f)
        };

        public static string Label(BoardCard card) => card.Kind switch
        {
            CardKind.Box => "BOX",
            CardKind.DayCreature => $"DAY x{card.Value}",
            CardKind.NightCreature => $"NIGHT x{card.Value}",
            CardKind.PositiveConstant => $"+{card.Value}",
            CardKind.NegativeConstant => $"-{card.Value}",
            CardKind.One => "ONE",
            CardKind.DivideTool => "DIV",
            _ => "?"
        };

        public static string Emoji(BoardCard card) => card.Kind switch
        {
            CardKind.Box => "📦",
            CardKind.DayCreature => CreatureArt.LightEmojiFor(card),
            CardKind.NightCreature => CreatureArt.DarkEmojiFor(card),
            CardKind.PositiveConstant => "🎲",
            CardKind.NegativeConstant => "🎲",
            CardKind.One => "😊",
            CardKind.DivideTool => "➗",
            _ => "?"
        };

        public static int EmojiFontSize(BoardCard card) => card.Kind switch
        {
            CardKind.Box => 44,
            CardKind.DayCreature => 52,
            CardKind.NightCreature => 52,
            CardKind.PositiveConstant => 46,
            CardKind.NegativeConstant => 46,
            CardKind.One => 44,
            CardKind.DivideTool => 40,
            _ => 38
        };

        public static bool PreferEmoji(BoardCard card) => false;

        public static Sprite CreatureSprite(BoardCard card)
        {
            if (card.Kind is CardKind.DayCreature or CardKind.NightCreature or CardKind.Box)
            {
                Sprite loaded = CardSpriteLoader.ForCard(card);
                if (loaded != null)
                {
                    return loaded;
                }
            }

            return card.Kind switch
            {
                CardKind.DayCreature => CreatureArt.LightSprite(card),
                CardKind.NightCreature => CreatureArt.DarkSprite(card),
                CardKind.Box => SpriteFactory.BoxSprite,
                _ => null
            };
        }

        public static Sprite IconSprite(BoardCard card)
        {
            Sprite creature = CreatureSprite(card);
            if (creature != null)
            {
                return creature;
            }

            return card.Kind switch
            {
                CardKind.PositiveConstant => SpriteFactory.PositiveDiceSprite,
                CardKind.NegativeConstant => SpriteFactory.NegativeDiceSprite,
                CardKind.One => SpriteFactory.SmileySprite,
                CardKind.DivideTool => SpriteFactory.DiceSprite,
                _ => null
            };
        }

        public static string AlgebraLabel(BoardCard card) => card.Kind switch
        {
            CardKind.Box => "x",
            CardKind.DayCreature => card.Value == 1 ? "x" : $"{card.Value}x",
            CardKind.NightCreature => card.Value == 1 ? "-x" : $"-{card.Value}x",
            CardKind.PositiveConstant => $"+{card.Value}",
            CardKind.NegativeConstant => $"-{card.Value}",
            CardKind.One => "1",
            CardKind.DivideTool => "÷",
            _ => "?"
        };
    }
}
