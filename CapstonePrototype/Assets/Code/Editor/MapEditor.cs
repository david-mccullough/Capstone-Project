using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        //EditorGUILayout.LabelField("Some help", "Some other text");

        MapGenerator map = target as MapGenerator;

        if (GUILayout.Button("Generate Map")) {
            map.GenerateMap(GameController.factions, 1);
        }
        
    }

}
