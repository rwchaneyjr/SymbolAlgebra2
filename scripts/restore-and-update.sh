#!/usr/bin/env bash
# Revert to Jul 9 copy (before today's changes) and apply that branch cleanly.
#
# Usage (Git Bash):
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/restore-and-update.sh
#
# Different commit:
#   RESTORE_COMMIT=c24de9d bash scripts/restore-and-update.sh

set -euo pipefail

BRANCH="${RESTORE_BRANCH:-cursor/level-curriculum-50-3fe3}"
COMMIT="${RESTORE_COMMIT:-b08a1b7}"
REPO="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"

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

echo "==> Revert to copy before today: commit $COMMIT"
echo "    Branch: $BRANCH"
echo "    Folder: $REPO"
echo ""

echo "==> Removing files that block git checkout"
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ThemeAssignment.cs.meta
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ChapterLevelGenerator.cs.meta
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/BoardFoldRules.cs
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/BoardFoldRules.cs.meta

echo ""
git fetch origin "$BRANCH"
git checkout -B "$BRANCH" "$COMMIT"
git reset --hard "$COMMIT"

echo ""
bash scripts/update.sh

echo ""
echo "Reverted to Jul 9 build ($COMMIT). Restart Unity and press Play."
