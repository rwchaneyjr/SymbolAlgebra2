#!/usr/bin/env bash
# Restore ALL DragonBox scripts from GitHub (fixes mixed drop-in / compile errors).
#
# Usage (Git Bash):
#   cd /c/Users/rober/SymbolAlgebra
#   bash scripts/restore-scripts.sh
#
# Custom branch:
#   RESTORE_BRANCH=main bash scripts/restore-scripts.sh

set -euo pipefail

RESTORE_BRANCH="${RESTORE_BRANCH:-cursor/level-curriculum-50-3fe3}"
SYMBOL_ALGEBRA_DIR="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"

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

ROOT="$(normalize_windows_path "$SYMBOL_ALGEBRA_DIR")"
SCRIPTS="$ROOT/Assets/DragonBoxAlgebra/Scripts"

if [[ ! -d "$ROOT/.git" ]]; then
  echo "Not a git repo: $ROOT" >&2
  exit 1
fi

cd "$ROOT"

echo "==> Restoring scripts from origin/$RESTORE_BRANCH"
echo "    Folder: $SCRIPTS"
echo ""

git fetch origin "$RESTORE_BRANCH"
git checkout "origin/$RESTORE_BRANCH" -- Assets/DragonBoxAlgebra/Scripts/
git checkout "origin/$RESTORE_BRANCH" -- Assets/DragonBoxAlgebra/Editor/ 2>/dev/null || true
git checkout "origin/$RESTORE_BRANCH" -- Assets/DragonBoxAlgebra/Scenes/

echo ""
echo "Restored. Open Unity, wait for compile, press Play."
echo "Do NOT run 'sync-dropins.sh import' unless dropins/ matches this branch."
