#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnderwaterOptimization
{
    public static class UnderwaterColliders
    {
        private const float MeshColliderThreshold = 12f; // Use a MeshCollider when a side exceeds this size

        [MenuItem("Tools/Underwater/6. Add Colliders (props)", false, 6)]
        public static void AddCollidersMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Underwater", "Exit Play mode before running this.", "OK");
                return;
            }

            string report = AddAll();
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

        }

        public static string AddAll()
        {
            int boxes = 0;
            int meshes = 0;
            int skipped = 0;
            StringBuilder sb = new StringBuilder("[Underwater] Colliders added: ");

            foreach (MeshRenderer r in Object.FindObjectsOfType<MeshRenderer>())
            {
                if (r is ParticleSystemRenderer)
                    continue;

                GameObject go = r.gameObject;

                if (go.name == "WaterBlock_50m" || go.name.StartsWith("Underwater_"))
                {
                    skipped++;
                    continue;
                }

                if (go.GetComponentInParent<Collider>(true) != null)
                {
                    skipped++;
                    continue;
                }

                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null)
                {
                    skipped++;
                    continue;
                }

                Bounds bounds = filter.sharedMesh.bounds;
                float maxSize = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));

                if (maxSize > MeshColliderThreshold)
                {
                    MeshCollider meshCollider = Undo.AddComponent<MeshCollider>(go);
                    meshCollider.sharedMesh = filter.sharedMesh;
                    meshCollider.convex = false;
                    meshes++;
                }
                else
                {
                    BoxCollider box = Undo.AddComponent<BoxCollider>(go);
                    box.center = bounds.center;
                    box.size = bounds.size;
                    boxes++;
                }
            }

            sb.Append("BoxCollider ").Append(boxes).Append(" boxes, MeshCollider ").Append(meshes)
              .Append(" meshes, skipped ").Append(skipped).Append(".");
            sb.Append("(large parts use MeshCollider, small parts BoxCollider; existing colliders are skipped)");
            return sb.ToString();
        }
    }
}
#endif