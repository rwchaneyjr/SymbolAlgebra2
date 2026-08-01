#!/usr/bin/env bash
# Restore your saved SymbolAlgebra copy (Ch1 chapter levels — the build you had before curriculum).
#
# Default: cursor/ch1-merge-intro-1c5d  (Ch1 Matching Pairs, Butterfly & Bat)
# Older save: RESTORE_BRANCH=cursor/my-save-copy-1c5d bash scripts/restore-saved-copy.sh
#
# Usage:
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/restore-saved-copy.sh

set -euo pipefail

BRANCH="${RESTORE_BRANCH:-cursor/ch1-merge-intro-1c5d}"
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

echo "==> Restoring SAVED COPY: origin/$BRANCH"
echo "    Folder: $REPO"
echo ""

echo "==> Removing files that block git checkout"
rm -f Assets/DragonBoxAlgebra/Scripts/Gameplay/ThemeAssignment.cs.meta

echo ""
git fetch origin "$BRANCH"
git checkout -B "$BRANCH" "origin/$BRANCH"
git reset --hard "origin/$BRANCH"

echo ""
echo "Done. You are on: $(git branch --show-current)"
echo "  Level 1 title in Play: Ch1 • Matching Pairs 1 • Butterfly & Bat"
echo ""
echo "Restart Unity and press Play."
echo ""
echo "Other saved branches:"
echo "  cursor/ch1-merge-intro-1c5d     (Jul 7 — Ch1 chapter, default)"
echo "  cursor/my-save-copy-1c5d       (Jul 5 — older save)"
echo "  cursor/snapshot-ch1-merge-jul-2026-1c5d  (same as ch1-merge)"
