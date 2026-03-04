using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(InformationPanelView))]
public class InformationPanelViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var view = (InformationPanelView)target;
        if (GUILayout.Button("Apply Connections"))
        {
            view.ApplyConnections();
            EditorUtility.SetDirty(view);
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
            }
        }
    }
}
