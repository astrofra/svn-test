using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(ConeGenerator))]
[CustomEditor(typeof(ConeGenerator), true)]
public class ConeGeneratorEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ConeGenerator gen = (ConeGenerator)target;
        if (GUILayout.Button("Generate Cone"))
        {
            //gen.CreateCone();
            gen.CreateConeSplited();
        }
    }
}
