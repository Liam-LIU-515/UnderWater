using UnityEngine;

namespace UnderwaterOptimization
{
    public class SosPhone : MonoBehaviour
    {
        public UnderwaterFlow flow;
        public float useRange = 4f;
        public KeyCode useKey = KeyCode.E;

        private bool used;
        public PowerGridPanel powerGrid;
        private TextMesh label;
        private Transform playerRoot;
        private Camera playerCamera;

        private void Start()
        {
            if (flow == null)
                flow = FindObjectOfType<UnderwaterFlow>();

            if (powerGrid == null)
            {
                // The panel is hidden until the box opens, so search inactive objects too
                PowerGridPanel[] panels = FindObjectsOfType<PowerGridPanel>(true);
                if (panels.Length > 0)
                    powerGrid = panels[0];
            }

            playerRoot = UnderwaterInput.PlayerRoot;
            playerCamera = UnderwaterInput.PlayerCamera;

            BuildLabel();
        }

        private void BuildLabel()
        {
            GameObject go = new GameObject("PhoneLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            label = go.AddComponent<TextMesh>();
            label.text = "Emergency phone";
            label.fontSize = 48;
            label.characterSize = 0.05f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(1f, 0.85f, 0.6f, 1f);

            Font font = UnderwaterFont.Get();
            if (font != null)
            {
                label.font = font;
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null && font.material != null)
                    renderer.sharedMaterial = font.material;
            }
        }

        private void Update()
        {
            if (used || playerRoot == null)
                return;

            float distance = Vector3.Distance(playerRoot.position, transform.position);
            bool near = distance <= useRange;
            // The phone only works with real power; a missing panel does not count
            bool powered = powerGrid != null && powerGrid.IsPowered;

            if (near)
            {
                if (!powered)
                {
                    if (flow != null)
                        flow.SetHint("No power. Reconnect the wiring panel on the wall first.");
                }
                else
                {
                    if (flow != null)
                        flow.SetHint("Make the distress call");

                    if (UnderwaterInput.InteractDown)
                        Call();
                }
            }

            if (label != null)
        {
            string want = "Emergency phone";
            if (label.text != want) label.text = want;
        }

        if (label != null && playerCamera != null)
            {
                label.transform.position = transform.position + Vector3.up * 0.6f;
                label.transform.rotation = playerCamera.transform.rotation;
            }
        }

        private void Call()
        {
            used = true;

            if (label != null)
                label.gameObject.SetActive(false);

            if (flow != null)
                flow.PlayCallDialogue();
                flow.SetObjective("Objective 7/7: Water is rising. Rope gun to the ceiling anchor");

        }
    }
}
