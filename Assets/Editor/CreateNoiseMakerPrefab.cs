using UnityEngine;
using UnityEditor;
using EchoThief.Player;

namespace EchoThief.Editor
{
    public static class CreateNoiseMakerPrefab
    {
        [MenuItem("EchoThief/Create NoiseMaker Prefab")]
        public static void Create()
        {
            // --- Build the GameObject ---
            var go = new GameObject("NoiseMaker");

            // Sphere mesh
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

            var mr = go.AddComponent<MeshRenderer>();
            // Try to find Dark_Env material; fall back to default
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Dark_Env.mat");
            mr.sharedMaterial = mat != null ? mat : new Material(Shader.Find("Universal Render Pipeline/Unlit"));

            // Scale down to feel like a small throwable
            go.transform.localScale = Vector3.one * 0.3f;

            // Collider
            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.5f; // matches sphere mesh unit radius

            // Rigidbody
            var rb = go.AddComponent<Rigidbody>();
            rb.mass             = 0.5f;
            rb.linearDamping    = 0.1f;
            rb.angularDamping   = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // NoiseMaker script
            go.AddComponent<NoiseMaker>();

            // Layer — create "NoiseMaker" layer if it doesn't exist
            int layer = LayerMask.NameToLayer("NoiseMaker");
            if (layer == -1)
            {
                Debug.LogWarning("[EchoThief] 'NoiseMaker' layer not found. " +
                    "Add it in Edit → Project Settings → Tags and Layers, then re-run this tool.");
            }
            else
            {
                go.layer = layer;
            }

            // --- Save as prefab ---
            const string dir    = "Assets/Prefabs";
            const string path   = dir + "/NoiseMaker.prefab";

            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            if (prefab != null)
            {
                Debug.Log($"[EchoThief] NoiseMaker prefab created at {path}");
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Debug.LogError("[EchoThief] Failed to save NoiseMaker prefab.");
            }
        }
    }
}