using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class FlexibleUIEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        FlexibleUI ui = target as FlexibleUI;

        if (GUILayout.Button("Apply")) {
            ui.Update();
        }

    }

}