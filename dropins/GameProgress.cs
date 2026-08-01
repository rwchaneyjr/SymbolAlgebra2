using UnityEngine;

namespace DragonBoxAlgebra.Gameplay
{
    public static class GameProgress
    {
        private const string LevelIndexKey = "SymbolAlgebra_CurrentLevel";

        public static int SavedLevelIndex => PlayerPrefs.GetInt(LevelIndexKey, 0);

        public static void SaveLevelIndex(int levelIndex)
        {
            PlayerPrefs.SetInt(LevelIndexKey, Mathf.Max(0, levelIndex));
            PlayerPrefs.Save();
        }

        public static void ResetToFirstLevel()
        {
            SaveLevelIndex(0);
        }
    }
}
