using System;
using System.Collections.Generic;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.UI;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelGenerator
    {
        public const int TotalLevels = 50;
        private const int HandPlayFromIndex = 36;
        private const int MirroredHandFromIndex = 43;

        private static readonly string[] CreatureThemeNames = CreatureArt.CreatureNames;

        public static IReadOnlyList<LevelDefinition> GenerateAll(int seed = 20260703)
        {
            var levels = new List<LevelDefinition>(TotalLevels);
            levels.AddRange(GenerateMergeIntroLevels());
            levels.AddRange(GenerateHandPlayLevels());
            return levels;
        }

        public static int HandCountForLevelIndex(int levelIndex) =>
            levelIndex < HandPlayFromIndex ? 0 : 1;

        private static IEnumerable<LevelDefinition> GenerateMergeIntroLevels()
        {
            for (int i = 0; i < HandPlayFromIndex; i++)
            {
                int theme = i % CreatureArt.ThemeCount;
                yield return BuildMergeIntroLevel(i, theme);
            }
        }

        /// <summary>
        /// Levels 1-36: pre-placed light/dark pairs to combine on the board (empty hand).
        /// 1-7 left pair beside box, 8-14 right pair beside box,
        /// 15-25 pair on right (box on left), 26-36 two pairs on left.
        /// </summary>
        private static LevelDefinition BuildMergeIntroLevel(int index, int theme)
        {
            int display = index + 1;
            string title;

            CardKind[] left;
            CardKind[] right;
            int parMoves;

            if (index < 7)
            {
                title = $"Pair on Left {display}";
                left = new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature };
                right = Array.Empty<CardKind>();
                parMoves = 2;
            }
            else if (index < 14)
            {
                title = $"Pair on Right {display}";
                left = Array.Empty<CardKind>();
                right = new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature };
                parMoves = 2;
            }
            else if (index < 25)
            {
                title = $"Match on Right {display}";
                left = new[] { CardKind.Box };
                right = new[] { CardKind.DayCreature, CardKind.NightCreature };
                parMoves = 2;
            }
            else
            {
                title = $"Double Pair on Left {display}";
                left = new[]
                {
                    CardKind.Box,
                    CardKind.DayCreature,
                    CardKind.NightCreature,
                    CardKind.DayCreature,
                    CardKind.NightCreature
                };
                right = Array.Empty<CardKind>();
                parMoves = 3;
            }

            var level = new LevelDefinition
            {
                Title = title,
                CreatureTheme = theme,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                ParMoves = parMoves,
                ParCards = 0
            };

            level.LeftValues = ValuesForCreatures(left, 1);
            level.RightValues = ValuesForCreatures(right, 1);
            AssignMatchingPairThemes(level);
            return level;
        }

        private static List<LevelDefinition> GenerateHandPlayLevels()
        {
            var levels = new List<LevelDefinition>();

            for (int levelIndex = HandPlayFromIndex; levelIndex < TotalLevels; levelIndex++)
            {
                int theme = levelIndex % CreatureArt.ThemeCount;
                bool mirrorBox = levelIndex >= MirroredHandFromIndex;
                int sectionNumber = levelIndex - HandPlayFromIndex + 1;
                string themeName = CreatureThemeNames[theme];
                string title = mirrorBox
                    ? $"Opposite Cards {sectionNumber} • {themeName} (right)"
                    : $"Opposite Cards {sectionNumber} • {themeName}";

                levels.Add(BuildOppositeCardsLevel(title, theme, mirrorBox));
            }

            return levels;
        }

        /// <summary>
        /// Levels 37-43: box + light/dark pair on left, one dark tile on right, one hand tile.
        /// Levels 44-50: mirrored — one dark tile on left, box + pair on right, one hand tile.
        /// </summary>
        private static LevelDefinition BuildOppositeCardsLevel(string title, int theme, bool mirrorBox)
        {
            CardKind[] left;
            CardKind[] right;

            if (mirrorBox)
            {
                left = new[] { CardKind.NightCreature };
                right = new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature };
            }
            else
            {
                left = new[] { CardKind.Box, CardKind.DayCreature, CardKind.NightCreature };
                right = new[] { CardKind.NightCreature };
            }

            var level = new LevelDefinition
            {
                Title = title,
                CreatureTheme = theme,
                LeftCards = new List<CardKind>(left),
                RightCards = new List<CardKind>(right),
                LeftValues = ValuesForCreatures(left, 1),
                RightValues = ValuesForCreatures(right, 1),
                HandCards = new List<CardKind> { CardKind.NightCreature },
                HandValues = new List<int> { 1 },
                ParMoves = 2,
                ParCards = 1
            };

            AssignOppositeCardsThemes(level);
            return level;
        }

        private static void AssignOppositeCardsThemes(LevelDefinition level)
        {
            int theme = level.CreatureTheme;
            level.LeftVisualThemes.Clear();
            level.RightVisualThemes.Clear();
            level.HandVisualThemes.Clear();

            foreach (CardKind kind in level.LeftCards)
            {
                level.LeftVisualThemes.Add(
                    kind is CardKind.DayCreature or CardKind.NightCreature ? theme : -1);
            }

            foreach (CardKind kind in level.RightCards)
            {
                level.RightVisualThemes.Add(
                    kind is CardKind.DayCreature or CardKind.NightCreature ? theme : -1);
            }

            level.HandVisualThemes.Add(theme);
        }

        private static void AssignMatchingPairThemes(LevelDefinition level)
        {
            level.LeftVisualThemes.Clear();
            level.RightVisualThemes.Clear();

            foreach (CardKind kind in level.LeftCards)
            {
                level.LeftVisualThemes.Add(
                    kind is CardKind.DayCreature or CardKind.NightCreature ? level.CreatureTheme : -1);
            }

            foreach (CardKind kind in level.RightCards)
            {
                level.RightVisualThemes.Add(
                    kind is CardKind.DayCreature or CardKind.NightCreature ? level.CreatureTheme : -1);
            }
        }

        private static List<int> ValuesForCreatures(CardKind[] cards, int value)
        {
            var values = new List<int>();
            foreach (CardKind kind in cards)
            {
                values.Add(kind is CardKind.DayCreature or CardKind.NightCreature ? value : 1);
            }

            return values;
        }
    }
}
