Sea creature images — REPLACE old art with your PNG/JPG files
=============================================================

Folder (put your images HERE):
  Assets/DragonBoxAlgebra/Resources/CreatureSprites/

DELETE old images from these folders if you have them:
  Assets/DragonBoxAlgebra/Resources/Sprites/
  Assets/DragonBoxAlgebra/Resources/CardSprites/
  Assets/DragonBoxAlgebra/Resources/Cards/

Your names work as-is:
  lightFish      + darkFish
  lightTurtle    + darkTurtle
  lightClam      + darkClam
  lightDolphin   + darkDolphin
  lightEel       + darkEel        (fix lightEelpng → lightEel)
  lightLobster   + darkLobster
  lightSeaHorse  + darkSeaHorse
  lightStarfish  + darkStarfish

Unity import (IMPORTANT — images won't show without this):
  1. Click the image in Project window
  2. Inspector → Texture Type → "Sprite (2D and UI)"
     (or "Default" also works now)
  3. Click Apply
  4. Press Play

If images still don't show:
  - Confirm files are in CreatureSprites (not just Downloads)
  - Run: git pull + bash scripts/sync-dropins.sh import --here
  - Edit → Clear All PlayerPrefs, then Play again
