#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DragonBoxAlgebra.Editor
{
    public class CreatureSpriteDropWindow : EditorWindow
    {
        private const string CreatureSpritesPath = "Assets/DragonBoxAlgebra/Resources/CreatureSprites";
        private Vector2 _scroll;
        private string _status = "Drag PNG or JPG files here from your desktop or file explorer.";

        [MenuItem("DragonBox Algebra/Import Creature Images (Drag and Drop)", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<CreatureSpriteDropWindow>("Creature Images");
            window.minSize = new Vector2(420f, 280f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sea creature card art", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Drag image files into the box below. They are copied to Resources/CreatureSprites " +
                "and set to Sprite (2D and UI) so they show on cards when you press Play.\n\n" +
                "Names: lightFish, darkFish, lightTurtle, darkTurtle, lightClam, darkClam, etc.",
                MessageType.Info);

            var dropRect = GUILayoutUtility.GetRect(0f, 120f, GUILayout.ExpandWidth(true));
            GUI.Box(dropRect, _status, EditorStyles.helpBox);
            HandleDragAndDrop(dropRect);

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Open CreatureSprites Folder"))
            {
                EnsureFolder();
                EditorUtility.RevealInFinder(Path.GetFullPath(CreatureSpritesPath));
            }

            if (GUILayout.Button("Setup Scene (if Hierarchy is empty)"))
            {
                DragonBoxEditorMenu.SetupScene();
            }

            EditorGUILayout.Space(8f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Expected pairs", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("lightFish + darkFish", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("lightTurtle + darkTurtle", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("lightClam + darkClam", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("lightDolphin + darkDolphin", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("lightEel + darkEel", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("lightLobster + darkLobster", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("lightSeaHorse + darkSeaHorse", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("lightStarfish + darkStarfish", EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }

        private void HandleDragAndDrop(Rect dropRect)
        {
            var evt = Event.current;
            if (!dropRect.Contains(evt.mousePosition))
            {
                return;
            }

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        ImportDroppedAssets();
                    }

                    evt.Use();
                    break;
            }
        }

        private void ImportDroppedAssets()
        {
            EnsureFolder();
            int imported = 0;

            foreach (string path in DragAndDrop.paths)
            {
                if (!IsImagePath(path))
                {
                    continue;
                }

                string fileName = Path.GetFileName(path);
                fileName = NormalizeCreatureFileName(fileName);
                string destAssetPath = $"{CreatureSpritesPath}/{fileName}";
                string destFullPath = Path.GetFullPath(destAssetPath);

                File.Copy(path, destFullPath, overwrite: true);
                AssetDatabase.ImportAsset(destAssetPath, ImportAssetOptions.ForceUpdate);
                ConfigureAsSprite(destAssetPath);
                imported++;
            }

            foreach (Object obj in DragAndDrop.objectReferences)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath) || !IsImagePath(assetPath))
                {
                    continue;
                }

                string fileName = Path.GetFileName(assetPath);
                string destAssetPath = $"{CreatureSpritesPath}/{fileName}";
                if (assetPath != destAssetPath)
                {
                    AssetDatabase.CopyAsset(assetPath, destAssetPath);
                }

                ConfigureAsSprite(destAssetPath);
                imported++;
            }

            AssetDatabase.Refresh();
            _status = imported > 0
                ? $"Imported {imported} image(s). Open DragonBox.unity and press Play."
                : "No PNG/JPG files detected in the drop.";
            Repaint();
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/DragonBoxAlgebra/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/DragonBoxAlgebra", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(CreatureSpritesPath))
            {
                AssetDatabase.CreateFolder("Assets/DragonBoxAlgebra/Resources", "CreatureSprites");
            }
        }

        private static bool IsImagePath(string path)
        {
            string ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg";
        }

        private static string NormalizeCreatureFileName(string fileName)
        {
            string lower = fileName.ToLowerInvariant();
            if (lower == "lighteelpng.png")
            {
                return "lightEel.png";
            }

            if (lower == "darkeel.png" || lower == "darkeeel.png")
            {
                return "darkEel.png";
            }

            return fileName;
        }

        private static void ConfigureAsSprite(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
#endif
