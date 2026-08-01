#!/usr/bin/env bash
# Pull latest game code + sync scripts + copy creature images.
#
# Usage (Git Bash):
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/update.sh
#
# Custom folder:
#   SYMBOL_ALGEBRA_DIR="/c/Users/rober/SymbolAlgebra" bash scripts/update.sh

set -euo pipefail

BRANCH="${UPDATE_BRANCH:-cursor/level-curriculum-50-3fe3}"
REPO="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"
SRC="$REPO/creature-images"
DEST="$REPO/Assets/DragonBoxAlgebra/Resources/CreatureSprites"

normalize_windows_path() {
  local p="$1"
  if [[ "$p" =~ ^[A-Za-z]: ]]; then
    local drive="${p:0:1}"
    drive=$(printf '%s' "$drive" | tr '[:upper:]' '[:lower:]')
    p="${p:2}"
    p="${p//\\//}"
    p="/${drive}${p}"
  fi
  printf '%s' "$p"
}

REPO="$(normalize_windows_path "$REPO")"

if [[ ! -d "$REPO/.git" ]]; then
  echo "Not a git repo: $REPO" >&2
  exit 1
fi

cd "$REPO"
mkdir -p "$SRC" "$DEST"

echo "==> Updating SymbolAlgebra from origin/$BRANCH"
echo "    Folder: $REPO"
echo ""

echo "==> Removing files that block git checkout"
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ThemeAssignment.cs.meta
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs.meta
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/BoardFoldRules.cs
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/BoardFoldRules.cs.meta

git fetch origin "$BRANCH"
git checkout -f "$BRANCH" 2>/dev/null || git checkout -B "$BRANCH" "origin/$BRANCH"
git reset --hard "origin/$BRANCH"

echo ""
echo "==> Restoring game scripts, editor tools, and scene from git"
git checkout "origin/$BRANCH" -- Assets/DragonBoxAlgebra/Scripts/
git checkout "origin/$BRANCH" -- Assets/DragonBoxAlgebra/Editor/ 2>/dev/null || true
git checkout "origin/$BRANCH" -- Assets/DragonBoxAlgebra/Scenes/

echo ""
echo "==> Syncing drop-in scripts"
bash scripts/sync-dropins.sh import --here

echo ""
echo "==> Removing old scripts that block custom images (Butterfly/Bat code)"
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs.meta
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/BoardFoldRules.cs
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/BoardFoldRules.cs.meta
rm -f dropins/ChapterLevelGenerator.cs 2>/dev/null || true

echo ""
echo "==> Copying creature images (if any in creature-images/)"
shopt -s nullglob
count=0
for f in "$SRC"/*.{png,jpg,jpeg,PNG,JPG,JPEG}; do
  cp -f "$f" "$DEST/"
  echo "  + $(basename "$f")"
  count=$((count + 1))
done

if [[ "$count" -eq 0 ]]; then
  echo "  (none — drop PNGs into creature-images/ and run again)"
else
  echo "  Copied $count image(s) to CreatureSprites."
fi

echo ""
echo "========== IMAGE DEBUG — next steps in Unity =========="
echo "  1. Restart Unity (so it recompiles scripts)"
echo "  2. Press Play"
echo "  3. Look for YELLOW text under the level title, OR open Console and filter: DragonBox"
echo ""
echo "  GOOD: Level title 'Pair on Left 1 • Fish' and sprites 16/16"
echo "  BAD:  Level title 'Ch1 • Butterfly & Bat' → run update.sh again"
echo "  BAD:  sprites 0/16 → images not in Resources/CreatureSprites or not Sprite type"
echo "======================================================="
