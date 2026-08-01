#!/usr/bin/env bash
# Drop PNG/JPG files into creature-images/ then run:
#   bash scripts/add-creature-images.sh

set -euo pipefail

REPO="${SYMBOL_ALGEBRA_DIR:-/c/Users/rober/SymbolAlgebra}"
SRC="$REPO/creature-images"
DEST="$REPO/Assets/DragonBoxAlgebra/Resources/CreatureSprites"

cd "$REPO"
mkdir -p "$SRC" "$DEST"

git fetch origin
git checkout cursor/level-curriculum-50-3fe3
bash scripts/sync-dropins.sh import --here

shopt -s nullglob
count=0
for f in "$SRC"/*.{png,jpg,jpeg,PNG,JPG,JPEG}; do
  cp -f "$f" "$DEST/"
  echo "  + $(basename "$f")"
  count=$((count + 1))
done

echo ""
echo "Copied $count image(s) to CreatureSprites."
echo "Unity:"
echo "  1. Open Assets/DragonBoxAlgebra/Scenes/DragonBox.unity"
echo "  2. Hierarchy should show AlgebraBootstrap, Main Camera, EventSystem"
echo "  3. Each new image → Sprite (2D and UI) → Apply"
echo "  4. Press Play (teal board + sea-creature cards)"
