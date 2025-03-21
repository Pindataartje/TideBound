using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(TerrainSpawner))]
public class TerrainSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainSpawner spawner = (TerrainSpawner)target;
        if (GUILayout.Button("Spawn Max (Edit Mode)"))
        {
            Debug.Log("Spawn Max button clicked in Editor.");
            spawner.SpawnTest();
            EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Clear Spawn Groups", EditorStyles.boldLabel);
        foreach (SpawnGroup group in spawner.spawnGroups)
        {
            if (GUILayout.Button("Clear " + group.groupName))
            {
                spawner.ClearGroup(group.groupName, group);
                EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            }
        }
    }
}
