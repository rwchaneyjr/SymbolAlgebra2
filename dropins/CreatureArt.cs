using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class CreatureArt
    {
        public const int ThemeCount = 8;

        public static readonly string[] CreatureNames =
        {
            "Fish", "Turtle", "Clam", "Dolphin", "Eel", "Lobster", "Sea Horse", "Starfish"
        };

        private static int _themeIndex;

        public static int ThemeIndex => _themeIndex;

        public static void SetTheme(int themeIndex)
        {
            _themeIndex = ((themeIndex % ThemeCount) + ThemeCount) % ThemeCount;
        }

        public static int ResolveTheme(BoardCard card) =>
            card.VisualTheme >= 0 ? card.VisualTheme : _themeIndex;

        public static Sprite LightSprite(BoardCard card) =>
            SpriteFactory.LightCreature(ResolveTheme(card), card.Value);

        public static Sprite DarkSprite(BoardCard card) =>
            SpriteFactory.DarkCreature(ResolveTheme(card), card.Value);

        public static string LightEmojiFor(BoardCard card) => LightEmojiForTheme(ResolveTheme(card));

        public static string DarkEmojiFor(BoardCard card) => DarkEmojiForTheme(ResolveTheme(card));

        public static string LightEmojiForTheme(int theme) => EmojiForTheme(NormalizeTheme(theme));

        public static string DarkEmojiForTheme(int theme) => EmojiForTheme(NormalizeTheme(theme));

        private static string EmojiForTheme(int theme) => theme switch
        {
            0 => "🐠",
            1 => "🐢",
            2 => "🦪",
            3 => "🐬",
            4 => "🐍",
            5 => "🦞",
            6 => "🐴",
            7 => "⭐",
            _ => "🐟"
        };

        private static int NormalizeTheme(int theme) =>
            ((theme % ThemeCount) + ThemeCount) % ThemeCount;

        public static string LightEmoji => LightEmojiForTheme(_themeIndex);

        public static string DarkEmoji => DarkEmojiForTheme(_themeIndex);

        public static string ThemeName => ThemeNameFor(_themeIndex);

        public static string ThemeNameFor(int theme)
        {
            int row = NormalizeTheme(theme);
            return row < CreatureNames.Length ? CreatureNames[row] : "Sea Creature";
        }
    }
}
