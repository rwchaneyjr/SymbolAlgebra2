#!/usr/bin/env bash
# Sync SymbolAlgebra drop-in C# scripts between dropins/ and a Unity project.
#
# Export (repo -> dropins/ flat folder):
#   bash scripts/sync-dropins.sh export
#
# Import (dropins/ -> your Unity project):
#   bash scripts/sync-dropins.sh import
#   SYMBOL_ALGEBRA_DIR="/c/Users/rober/SymbolAlgebra" bash scripts/sync-dropins.sh import
#
# Import from inside this repo (updates Assets/DragonBoxAlgebra/Scripts):
#   bash scripts/sync-dropins.sh import --here

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DROPINS_DIR="${DROPINS_DIR:-$REPO_ROOT/dropins}"
ASSETS_SCRIPTS="${ASSETS_SCRIPTS:-$REPO_ROOT/Assets/DragonBoxAlgebra/Scripts}"
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

target_subdir_for() {
  case "$1" in
    AlgebraBootstrap.cs|AlgebraSceneSetup.cs|SceneCameraSetup.cs) printf '%s' "" ;;
    Audio/*) printf '%s' "Audio" ;;
    Core/*) printf '%s' "Core" ;;
    Gameplay/*) printf '%s' "Gameplay" ;;
    UI/*) printf '%s' "UI" ;;
    *) return 1 ;;
  esac
}

export_dropins() {
  echo "==> Exporting Assets -> dropins/"
  echo "    From: $ASSETS_SCRIPTS"
  echo "    To:   $DROPINS_DIR"
  echo ""

  mkdir -p "$DROPINS_DIR"

  local count=0
  while IFS= read -r -d '' src; do
    local base
    base="$(basename "$src")"
    cp "$src" "$DROPINS_DIR/$base"
    count=$((count + 1))
    echo "    $base"
  done < <(find "$ASSETS_SCRIPTS" -name "*.cs" -type f -print0 | sort -z)

  echo ""
  echo "Exported $count files to dropins/"
}

import_dropins() {
  local target_root="$1"
  local scripts_dir="$target_root/Assets/DragonBoxAlgebra/Scripts"

  if [[ ! -d "$DROPINS_DIR" ]]; then
    echo "dropins/ folder not found: $DROPINS_DIR" >&2
    exit 1
  fi

  echo "==> Importing dropins/ -> Unity project"
  echo "    From: dropins/"
  echo "    To:   $scripts_dir"
  echo ""

  mkdir -p "$scripts_dir/Core" "$scripts_dir/Gameplay" "$scripts_dir/UI" "$scripts_dir/Audio"

  local count=0
  for src in "$DROPINS_DIR"/*.cs; do
    [[ -f "$src" ]] || continue
    local base dest_subdir dest
    base="$(basename "$src")"

    case "$base" in
      AlgebraBootstrap.cs|AlgebraSceneSetup.cs|SceneCameraSetup.cs) dest_subdir="" ;;
      AudioManager.cs) dest_subdir="Audio" ;;
      AlgebraBoard.cs|BoardCard.cs|BoardSide.cs|CardKind.cs|CombineRules.cs|WinChecker.cs)
        dest_subdir="Core" ;;
      AlgebraGameController.cs|BalancePending.cs|CardFlipRules.cs|GameSnapshot.cs|GameProgress.cs|HandRules.cs|\
      BoardVisualRules.cs|CoordinatedCreatureThemes.cs|HandCompositionRules.cs|HandVisualRules.cs|\
      LevelDefinition.cs|LevelGenerator.cs|LevelLibrary.cs|LevelSolvabilityRules.cs|MoveTracker.cs|\
      PendingCancelMarker.cs|ThemeAssignment.cs)
        dest_subdir="Gameplay" ;;
      BoardSideLayout.cs|BoardView.cs|AlgebraUI.cs|AsteriskCancelWidget.cs|BalanceHoleWidget.cs|\
      BoardDropZone.cs|CardSpriteLoader.cs|CardVisuals.cs|CardWidget.cs|CreatureArt.cs|\
      CreatureReaction.cs|CreatureSpriteDebug.cs|EmojiFont.cs|HandView.cs|LevelCompleteView.cs|SpriteFactory.cs|VortexEffect.cs)
        dest_subdir="UI" ;;
      *) dest_subdir="UI" ;;
    esac

    if [[ -z "$dest_subdir" ]]; then
      dest="$scripts_dir/$base"
    else
      dest="$scripts_dir/$dest_subdir/$base"
    fi

    cp "$src" "$dest"
    count=$((count + 1))
    if [[ -z "$dest_subdir" ]]; then
      echo "    $base -> Scripts/"
    else
      echo "    $base -> $dest_subdir/"
    fi
  done

  echo ""
  echo "Imported $count files."
  echo "Open Unity, wait for compile, then press Play."
  echo "Scene: Assets/DragonBoxAlgebra/Scenes/DragonBox.unity"
}

usage() {
  cat <<'EOF'
Usage:
  bash scripts/sync-dropins.sh export
  bash scripts/sync-dropins.sh import [--here]
  SYMBOL_ALGEBRA_DIR="/c/Users/rober/SymbolAlgebra" bash scripts/sync-dropins.sh import

export  Copy all C# from Assets/DragonBoxAlgebra/Scripts into dropins/
import  Copy dropins/*.cs into a Unity project's Scripts subfolders
--here  Import into this repo's Assets (default target is SYMBOL_ALGEBRA_DIR)
EOF
}

main() {
  local cmd="${1:-import}"
  shift || true

  case "$cmd" in
    export)
      export_dropins
      ;;
    import)
      local target
      if [[ "${1:-}" == "--here" ]]; then
        target="$REPO_ROOT"
      else
        target="$(normalize_windows_path "$SYMBOL_ALGEBRA_DIR")"
      fi

      if [[ ! -d "$target/Assets/DragonBoxAlgebra" && "$target" != "$REPO_ROOT" ]]; then
        echo "Unity project not found: $target" >&2
        echo "Set SYMBOL_ALGEBRA_DIR or use: bash scripts/sync-dropins.sh import --here" >&2
        exit 1
      fi

      import_dropins "$target"
      ;;
    -h|--help|help)
      usage
      ;;
    *)
      echo "Unknown command: $cmd" >&2
      usage >&2
      exit 1
      ;;
  esac
}

main "$@"
