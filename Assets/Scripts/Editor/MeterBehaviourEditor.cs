using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GaugeManager))]
public class MeterBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            var meter = (GaugeManager)target;
            if (GUILayout.Button("Shake"))
            {
                meter.ShakeForDebug();
            }
        }

    }

}
