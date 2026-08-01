using System.Collections.Generic;
using DragonBoxAlgebra.UI;

namespace DragonBoxAlgebra.Gameplay
{
    public static class ThemeAssignment
    {
        public const int ThemeCount = CreatureArt.ThemeCount;

        public static List<int> DistinctThemes(int count, int preferredFirst = -1)
        {
            var themes = new List<int>();
            var used = new HashSet<int>();

            if (count <= 0)
            {
                return themes;
            }

            if (preferredFirst >= 0 && preferredFirst < ThemeCount)
            {
                themes.Add(preferredFirst);
                used.Add(preferredFirst);
            }

            while (themes.Count < count)
            {
                int theme = PickUnused(used);
                themes.Add(theme);
                used.Add(theme);
            }

            return themes;
        }

        public static List<int> DistinctThemesExcluding(int count, ISet<int> exclude, int preferredFirst = -1)
        {
            var themes = new List<int>();
            var used = exclude != null ? new HashSet<int>(exclude) : new HashSet<int>();

            if (count <= 0)
            {
                return themes;
            }

            if (preferredFirst >= 0 && preferredFirst < ThemeCount && !used.Contains(preferredFirst))
            {
                themes.Add(preferredFirst);
                used.Add(preferredFirst);
            }

            while (themes.Count < count)
            {
                int theme = PickUnused(used);
                themes.Add(theme);
                used.Add(theme);
            }

            return themes;
        }

        public static int PickUnused(ISet<int> used)
        {
            for (int theme = 0; theme < ThemeCount; theme++)
            {
                if (!used.Contains(theme))
                {
                    return theme;
                }
            }

            return 0;
        }
    }
}
