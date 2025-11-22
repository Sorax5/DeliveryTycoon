using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapNoiseVisualizer : EditorWindow
{
    public MapDefinition map;

    private int seed = 1337;
    private Texture2D previewTexture;
    private int previewSize = 512; // taille maximum pour la génération (contrôlable)
    private const int previewSizeMin = 64;
    private const int previewSizeMax = 2048; // limite de sécurité pour éviter OOM
    private bool autoUpdate = false;
    private int lastSeed;
    private MapDefinition lastMapReference;
    private float previewZoom = 1.0f;

    // SerializedObject pour éditer proprement le ScriptableObject (support undo & apply)
    private SerializedObject mapSO;

    [MenuItem("Tools/Map Noise Visualizer")]
    public static void ShowWindow()
    {
        GetWindow<MapNoiseVisualizer>("Map Noise Visualizer");
    }

    private void OnDisable()
    {
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
            previewTexture = null;
        }
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        var newMap = (MapDefinition)EditorGUILayout.ObjectField("Map", map, typeof(MapDefinition), false);
        if (newMap != map)
        {
            map = newMap;
            // créer ou rafraîchir SerializedObject quand on change de map
            mapSO = map != null ? new SerializedObject(map) : null;
        }

        EditorGUILayout.BeginHorizontal();
        seed = EditorGUILayout.IntField("Seed", seed);
        autoUpdate = EditorGUILayout.ToggleLeft("Auto", autoUpdate, GUILayout.Width(48));
        EditorGUILayout.EndHorizontal();

        // Contrôles pour la taille et l'affichage de l'aperçu
        EditorGUILayout.BeginHorizontal();
        previewSize = EditorGUILayout.IntSlider("Résolution aperçu", previewSize, previewSizeMin, previewSizeMax);
        if (GUILayout.Button("Plein (limit)", GUILayout.Width(100)))
        {
            GeneratePreview(fullResolution: true);
        }
        EditorGUILayout.EndHorizontal();

        previewZoom = EditorGUILayout.Slider("Zoom affichage", previewZoom, 0.25f, 4f);

        if (map == null)
        {
            EditorGUILayout.HelpBox("Aucun MapDefinition sélectionné.", MessageType.Info);
            return;
        }

        if (map.NoiseDefinition == null)
        {
            EditorGUILayout.HelpBox("Le MapDefinition n'a pas de NoiseDefinition.", MessageType.Warning);
            return;
        }

        if (map.FloorRanges == null || map.FloorRanges.Count == 0)
        {
            EditorGUILayout.HelpBox("Aucune FloorRange dans le MapDefinition.", MessageType.Info);
            return;
        }

        GUILayout.Label("Floor Noise Ranges (édition)");
        DrawEditableFloorRanges();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Générer l'aperçu"))
        {
            GeneratePreview();
        }

        if (GUILayout.Button("Supprimer aperçu"))
        {
            ClearPreview();
        }
        EditorGUILayout.EndHorizontal();

        // Auto regenerate when seed or map changed and autoUpdate enabled
        if (autoUpdate && (seed != lastSeed || map != lastMapReference))
        {
            GeneratePreview();
        }

        // Display preview (scaled by previewZoom)
        if (previewTexture != null)
        {
            GUILayout.Space(6);
            float dispW = previewTexture.width * previewZoom;
            float dispH = previewTexture.height * previewZoom;

            // Clamp display size so it fits reasonably in the window
            float maxDisplay = Mathf.Max(800, EditorGUIUtility.currentViewWidth - 20);
            if (dispW > maxDisplay)
            {
                float scale = maxDisplay / dispW;
                dispW *= scale;
                dispH *= scale;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(previewTexture, GUILayout.Width(dispW), GUILayout.Height(dispH));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Information sous l'aperçu
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Tex: {previewTexture.width}x{previewTexture.height}  Zoom: {previewZoom:F2}", GUILayout.Width(220));
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
        {
            // Si on a changé la référence map via le ObjectField, s'assurer que SerializedObject est à jour
            if (map != null && mapSO == null) mapSO = new SerializedObject(map);
        }
    }

    private void DrawEditableFloorRanges()
    {
        // Utilisation de SerializedObject/SerializedProperty pour prise en charge de l'undo et de l'apply
        if (mapSO == null)
        {
            mapSO = new SerializedObject(map);
        }

        mapSO.Update();

        var floorsProp = mapSO.FindProperty("FloorRanges");
        if (floorsProp == null)
        {
            EditorGUILayout.HelpBox("Impossible de trouver la propriété FloorRanges (serialized).", MessageType.Error);
            return;
        }

        // Boutons d'édition global
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Ajouter FloorRange"))
        {
            Undo.RecordObject(map, "Ajouter FloorRange");
            var newRange = new FloorRange
            {
                NoiseRange = new Vector2(-1f, 1f)
            };
            map.FloorRanges.Add(newRange);
            EditorUtility.SetDirty(map);
            mapSO.Update(); // rafraîchir serialized
        }
        if (GUILayout.Button("Supprimer dernier"))
        {
            if (map.FloorRanges.Count > 0)
            {
                Undo.RecordObject(map, "Supprimer FloorRange");
                map.FloorRanges.RemoveAt(map.FloorRanges.Count - 1);
                EditorUtility.SetDirty(map);
                mapSO.Update();
            }
        }
        EditorGUILayout.EndHorizontal();

        // Itérer et afficher chaque élément modifiable sur une seule ligne
        for (int i = 0; i < floorsProp.arraySize; i++)
        {
            var elem = floorsProp.GetArrayElementAtIndex(i);
            var floorDefProp = elem.FindPropertyRelative("FloorDefinition");
            var noiseProp = elem.FindPropertyRelative("NoiseRange");

            EditorGUILayout.BeginHorizontal();

            // Pastille couleur provenant de l'objet FloorDefinition si présent
            var floorDefObj = floorDefProp.objectReferenceValue as FloorDefinition;
            Color fillColor = floorDefObj != null ? floorDefObj.debugColor : ColorFromString($"floor_{i}");
            Rect swatchRect = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18), GUILayout.Height(18));
            EditorGUI.DrawRect(swatchRect, fillColor);
            GUILayout.Space(4);

            // Champ pour la référence FloorDefinition
            EditorGUILayout.PropertyField(floorDefProp, GUIContent.none, GUILayout.Width(160));

            // NoiseRange : affichage Min/Max + MinMaxSlider
            Vector2 noiseRange = noiseProp.vector2Value;
            EditorGUILayout.LabelField("Min", GUILayout.Width(32));
            noiseRange.x = EditorGUILayout.DelayedFloatField(noiseRange.x, GUILayout.Width(60));
            EditorGUILayout.LabelField("Max", GUILayout.Width(36));
            noiseRange.y = EditorGUILayout.DelayedFloatField(noiseRange.y, GUILayout.Width(60));

            // Slider (utilise ref pour modifier directement les valeurs)
            EditorGUILayout.MinMaxSlider(ref noiseRange.x, ref noiseRange.y, -1.1f, 1f, GUILayout.Width(160));

            // Clamp et garantir min <= max
            noiseRange.x = Mathf.Clamp(noiseRange.x, -1.1f, noiseRange.y);
            noiseRange.y = Mathf.Clamp(noiseRange.y, noiseRange.x, 1f);

            noiseProp.vector2Value = noiseRange;

            // Boutons utilitaires pour déplacer/dupliquer/supprimer
            if (GUILayout.Button("↑", GUILayout.Width(24))) { MoveElement(floorsProp, i, -1); break; }
            if (GUILayout.Button("↓", GUILayout.Width(24))) { MoveElement(floorsProp, i, +1); break; }
            if (GUILayout.Button("Dup", GUILayout.Width(36))) { DuplicateElement(floorsProp, i); break; }
            if (GUILayout.Button("Del", GUILayout.Width(36))) { RemoveElement(floorsProp, i); break; }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        // Appliquer modifications sérialisées
        if (mapSO.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(map);
        }
    }

    private void MoveElement(SerializedProperty arrayProp, int index, int offset)
    {
        int newIndex = Mathf.Clamp(index + offset, 0, arrayProp.arraySize - 1);
        if (newIndex == index) return;

        Undo.RecordObject(map, "Déplacer FloorRange");
        var tmp = map.FloorRanges[index];
        map.FloorRanges.RemoveAt(index);
        map.FloorRanges.Insert(newIndex, tmp);
        EditorUtility.SetDirty(map);
        mapSO.Update();
    }

    private void DuplicateElement(SerializedProperty arrayProp, int index)
    {
        if (index < 0 || index >= map.FloorRanges.Count) return;
        Undo.RecordObject(map, "Dupliquer FloorRange");
        var src = map.FloorRanges[index];
        var copy = new FloorRange
        {
            FloorDefinition = src.FloorDefinition,
            NoiseRange = src.NoiseRange
        };
        map.FloorRanges.Insert(index + 1, copy);
        EditorUtility.SetDirty(map);
        mapSO.Update();
    }

    private void RemoveElement(SerializedProperty arrayProp, int index)
    {
        if (index < 0 || index >= map.FloorRanges.Count) return;
        Undo.RecordObject(map, "Supprimer FloorRange");
        map.FloorRanges.RemoveAt(index);
        EditorUtility.SetDirty(map);
        mapSO.Update();
    }

    private void GeneratePreview(bool fullResolution = false)
    {
        if (map == null || map.NoiseDefinition == null)
            return;

        // destroy previous texture to avoid leak
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
            previewTexture = null;
        }

        int texW, texH;

        if (fullResolution)
        {
            // Try to generate at map's full resolution but clamp to previewSizeMax for safety
            int clampW = Mathf.Clamp(map.Width, 1, previewSizeMax);
            int clampH = Mathf.Clamp(map.Height, 1, previewSizeMax);

            // if map is bigger than clamp, scale down preserving aspect
            if (map.Width > previewSizeMax || map.Height > previewSizeMax)
            {
                float aspect = map.Width / (float)map.Height;
                if (map.Width > map.Height)
                {
                    texW = previewSizeMax;
                    texH = Mathf.Max(1, Mathf.RoundToInt(previewSizeMax / aspect));
                }
                else
                {
                    texH = previewSizeMax;
                    texW = Mathf.Max(1, Mathf.RoundToInt(previewSizeMax * aspect));
                }
            }
            else
            {
                texW = clampW;
                texH = clampH;
            }
        }
        else
        {
            // Use user chosen previewSize as largest dimension (preserve aspect)
            int maxDim = Mathf.Clamp(previewSize, previewSizeMin, previewSizeMax);
            float aspectMap = map.Width / (float)map.Height;
            if (map.Width >= map.Height)
            {
                texW = maxDim;
                texH = Mathf.Max(1, Mathf.RoundToInt(maxDim / aspectMap));
            }
            else
            {
                texH = maxDim;
                texW = Mathf.Max(1, Mathf.RoundToInt(maxDim * aspectMap));
            }
        }

        previewTexture = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        previewTexture.filterMode = FilterMode.Point;
        previewTexture.wrapMode = TextureWrapMode.Clamp;

        // Create noise maker from definition and set seed
        var noiseMaker = map.NoiseDefinition.CreateNoiseMaker();
        noiseMaker.SetSeed(seed);

        // Scale from texture pixel to map coordinates
        float stepX = map.Width / (float)texW;
        float stepY = map.Height / (float)texH;

        for (int py = 0; py < texH; py++)
        {
            for (int px = 0; px < texW; px++)
            {
                // sample at cell center
                float sampleX = (px + 0.5f) * stepX;
                float sampleY = (py + 0.5f) * stepY;

                // Use noiseMaker.GetNoise with map coords
                float noise = noiseMaker.GetNoise(sampleX, sampleY); // returns in [-1,1]

                var floor = map.GetFloorRangeByNoise(noise);
                Color col;
                if (floor != null && floor.FloorDefinition != null)
                {
                    // use debugColor from FloorDefinition
                    col = floor.FloorDefinition.debugColor;
                }
                else
                {
                    col = Color.black;
                }

                previewTexture.SetPixel(px, texH - 1 - py, col);
            }
        }

        previewTexture.Apply();

        lastSeed = seed;
        lastMapReference = map;
        Repaint();
    }

    private void ClearPreview()
    {
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
            previewTexture = null;
            Repaint();
        }
    }

    // Deterministic color generator from a string (stable per asset name)
    private Color ColorFromString(string s)
    {
        if (string.IsNullOrEmpty(s)) return Color.white;
        unchecked
        {
            int hash = 23;
            foreach (char c in s) hash = hash * 31 + c;
            float hue = (hash & 0xFFFF) / (float)ushort.MaxValue;
            float saturation = 0.6f;
            float value = 0.85f;
            return Color.HSVToRGB(Mathf.Repeat(hue, 1f), saturation, value);
        }
    }
}