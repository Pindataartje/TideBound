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

        // Global spawn test button.
        if (GUILayout.Button("Spawn Max (Edit Mode) for All Groups"))
        {
            Debug.Log("Global Spawn Max button clicked in Editor.");
            spawner.SpawnTest();
            EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Per-Group Controls", EditorStyles.boldLabel);
        foreach (SpawnGroup group in spawner.spawnGroups)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Spawn " + group.groupName))
            {
                spawner.SpawnGroupTest(group);
                EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            }
            if (GUILayout.Button("Clear " + group.groupName))
            {
                spawner.ClearGroup(group.groupName, group);
                EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
