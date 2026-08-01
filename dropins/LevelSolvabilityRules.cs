using System.Collections.Generic;
using DragonBoxAlgebra.Core;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelSolvabilityRules
    {
        public const int ExtraPuzzleFromIndex = 12;
        public const int ExtraPuzzleToIndex = 22;

        public static bool ShouldConfigureBoxSide(int handCount) => handCount >= 2;

        public static bool IsExtraPuzzleLevel(int levelIndex) =>
            levelIndex >= ExtraPuzzleFromIndex && levelIndex < ExtraPuzzleToIndex;

        public static void ConfigureSolvableLevel(LevelDefinition level, int handCount, bool diceLevel, int value,
            int extraTileCount = 0, bool extraTilesOnOtherSide = true)
        {
            if (handCount < 2)
            {
                return;
            }

            if (extraTileCount == 0)
            {
                ConfigureStandardSolvableLevel(level, handCount, diceLevel, value);
                return;
            }

            int leftBesideBox = handCount;
            int rightCount = 0;
            if (extraTileCount > 0)
            {
                if (extraTilesOnOtherSide)
                {
                    rightCount = extraTileCount;
                }
                else if (handCount + extraTileCount <= 3)
                {
                    leftBesideBox = handCount + extraTileCount;
                }
                else
                {
                    rightCount = extraTileCount;
                }
            }

            if (diceLevel)
            {
                ApplyDiceBoard(level, leftBesideBox, rightCount, value);
                return;
            }

            ApplyCreatureBoard(level, leftBesideBox, rightCount, value, handCount);
        }

        public static void ConfigureStandardSolvableLevel(LevelDefinition level, int handCount, bool diceLevel, int value)
        {
            level.RightCards.Clear();
            level.RightValues.Clear();
            level.RightVisualThemes.Clear();

            if (diceLevel)
            {
                CardKind solverKind = level.HandCards[0];
                CardKind diceObstacle = solverKind == CardKind.NegativeConstant
                    ? CardKind.PositiveConstant
                    : CardKind.NegativeConstant;

                level.LeftCards = new List<CardKind> { CardKind.Box, diceObstacle };
                level.LeftValues = new List<int> { 1, value };
                level.LeftVisualThemes = new List<int> { -1, -1 };
                return;
            }

            ApplyCreatureBoard(level, leftBesideBox: handCount, rightCount: 0, value, handCount);
        }

        public static void ConfigureMirrorSolvableLevel(LevelDefinition level, int handCount, bool diceLevel, int value)
        {
            level.LeftCards.Clear();
            level.LeftValues.Clear();
            level.LeftVisualThemes.Clear();

            if (diceLevel)
            {
                CardKind solverKind = level.HandCards[0];
                CardKind diceObstacle = solverKind == CardKind.NegativeConstant
                    ? CardKind.PositiveConstant
                    : CardKind.NegativeConstant;

                level.RightCards = new List<CardKind> { CardKind.Box, diceObstacle };
                level.RightValues = new List<int> { 1, value };
                level.RightVisualThemes = new List<int> { -1, -1 };
                return;
            }

            ApplyCreatureBoardBoxOnRight(level, rightBesideBox: handCount, leftCount: 0, value, handCount);
        }

        private static void ApplyCreatureBoard(LevelDefinition level, int leftBesideBox, int rightCount, int value,
            int handCount)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind obstacleKind = solverKind == CardKind.NightCreature
                ? CardKind.DayCreature
                : CardKind.NightCreature;

            List<int> handThemes = CoordinatedCreatureThemes.BuildRedSideThemes(handCount, level.CreatureTheme);
            List<int> leftThemes = leftBesideBox <= handCount
                ? handThemes
                : CoordinatedCreatureThemes.BuildRedSideThemes(leftBesideBox, level.CreatureTheme);

            var usedThemes = new HashSet<int>(leftThemes);
            List<int> rightThemes = rightCount > 0
                ? CoordinatedCreatureThemes.BuildOtherSideThemes(rightCount, usedThemes, level.CreatureTheme)
                : new List<int>();

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftVisualThemes = new List<int> { -1 };

            for (int i = 0; i < leftBesideBox; i++)
            {
                leftCards.Add(obstacleKind);
                leftValues.Add(value);
                leftVisualThemes.Add(leftThemes[i]);
            }

            var rightCards = new List<CardKind>();
            var rightValues = new List<int>();
            var rightVisualThemes = new List<int>();

            for (int i = 0; i < rightCount; i++)
            {
                rightCards.Add(obstacleKind);
                rightValues.Add(value);
                rightVisualThemes.Add(rightThemes[i]);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;

            CoordinatedCreatureThemes.ApplyRedSideAndHand(level, handThemes);
        }

        private static void ApplyCreatureBoardBoxOnRight(LevelDefinition level, int rightBesideBox, int leftCount, int value,
            int handCount)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind obstacleKind = solverKind == CardKind.NightCreature
                ? CardKind.DayCreature
                : CardKind.NightCreature;

            List<int> handThemes = CoordinatedCreatureThemes.BuildRedSideThemes(handCount, level.CreatureTheme);
            List<int> rightThemes = rightBesideBox <= handCount
                ? handThemes
                : CoordinatedCreatureThemes.BuildRedSideThemes(rightBesideBox, level.CreatureTheme);

            var usedThemes = new HashSet<int>(rightThemes);
            List<int> leftThemes = leftCount > 0
                ? CoordinatedCreatureThemes.BuildOtherSideThemes(leftCount, usedThemes, level.CreatureTheme)
                : new List<int>();

            var rightCards = new List<CardKind> { CardKind.Box };
            var rightValues = new List<int> { 1 };
            var rightVisualThemes = new List<int> { -1 };

            for (int i = 0; i < rightBesideBox; i++)
            {
                rightCards.Add(obstacleKind);
                rightValues.Add(value);
                rightVisualThemes.Add(rightThemes[i]);
            }

            var leftCards = new List<CardKind>();
            var leftValues = new List<int>();
            var leftVisualThemes = new List<int>();

            for (int i = 0; i < leftCount; i++)
            {
                leftCards.Add(obstacleKind);
                leftValues.Add(value);
                leftVisualThemes.Add(leftThemes[i]);
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;

            CoordinatedCreatureThemes.ApplyRedSideAndHand(level, handThemes);
        }

        private static void ApplyDiceBoard(LevelDefinition level, int leftBesideBox, int rightCount, int value)
        {
            CardKind solverKind = level.HandCards[0];
            CardKind obstacleKind = solverKind == CardKind.NegativeConstant
                ? CardKind.PositiveConstant
                : CardKind.NegativeConstant;
            CardKind oppositeObstacle = obstacleKind == CardKind.PositiveConstant
                ? CardKind.NegativeConstant
                : CardKind.PositiveConstant;

            var leftCards = new List<CardKind> { CardKind.Box };
            var leftValues = new List<int> { 1 };
            var leftVisualThemes = new List<int> { -1 };

            for (int i = 0; i < leftBesideBox; i++)
            {
                leftCards.Add(obstacleKind);
                leftValues.Add(value);
                leftVisualThemes.Add(-1);
            }

            var rightCards = new List<CardKind>();
            var rightValues = new List<int>();
            var rightVisualThemes = new List<int>();

            if (rightCount >= 2)
            {
                int pairTheme = level.CreatureTheme;
                rightCards.Add(CardKind.DayCreature);
                rightValues.Add(value);
                rightVisualThemes.Add(pairTheme);
                rightCards.Add(CardKind.NightCreature);
                rightValues.Add(value);
                rightVisualThemes.Add(pairTheme);
            }
            else
            {
                for (int i = 0; i < rightCount; i++)
                {
                    CardKind kind = i % 2 == 0 ? obstacleKind : oppositeObstacle;
                    rightCards.Add(kind);
                    rightValues.Add(value);
                    rightVisualThemes.Add(-1);
                }
            }

            level.LeftCards = leftCards;
            level.LeftValues = leftValues;
            level.LeftVisualThemes = leftVisualThemes;
            level.RightCards = rightCards;
            level.RightValues = rightValues;
            level.RightVisualThemes = rightVisualThemes;
        }
    }
}
