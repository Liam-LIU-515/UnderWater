using System.Collections.Generic;
using UnityEngine;

namespace UnderwaterOptimization
{
    public class PowerGridPanel : MonoBehaviour
    {
        public List<PowerTerminal> terminals = new List<PowerTerminal>();
        public float workRange = 5f;
        public float pickAngle = 35f;

        private UnderwaterFlow flow;
        private PowerTerminal selected;
        private int connected;
        private LineRenderer preview;
        private bool nearPanel;

        public bool IsPowered
        {
            get { return terminals.Count > 0 && connected >= terminals.Count / 2; }
        }

        private void Start()
        {
            flow = FindObjectOfType<UnderwaterFlow>();

            GameObject go = new GameObject("GridPreviewWire");
            preview = go.AddComponent<LineRenderer>();
            preview.positionCount = 2;
            preview.useWorldSpace = true;
            preview.widthMultiplier = 0.05f;
            preview.enabled = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                Material material = new Material(shader);
                material.SetColor("_BaseColor", Color.white);
                preview.material = material;
            }

            // Fallback: collect the terminals automatically if the list is empty
            if (terminals.Count == 0)
            {
                PowerTerminal[] found = FindObjectsOfType<PowerTerminal>(true);
                terminals.AddRange(found);
            }

        }

        private void Update()
        {
            if (IsPowered)
                return;

            Vector3 origin = UnderwaterInput.RayOrigin;
            Vector3 direction = UnderwaterInput.RayDirection;

            nearPanel = Vector3.Distance(origin, transform.position) <= workRange;

            if (!nearPanel)
            {
                if (selected != null)
                    Cancel("Left the panel. Wire released.");

                nearPanel = false;
                return;
            }

            // Empty handed: grab the nearest wire
            if (selected == null && UnderwaterInput.FireDown)
            {
                selected = FindNearest(direction, origin, true);

                if (selected != null)
                {
                    selected.SetDragging(true);

                    if (preview.material != null)
                        preview.material.SetColor("_BaseColor", PowerTerminal.ColorOf(selected.colorIndex));

                }
            }

            // Holding a wire: it follows the crosshair, release to connect
            if (selected != null)
            {
                Vector3 tip = origin + direction * 4f;
                preview.enabled = true;
                preview.SetPosition(0, selected.transform.position);
                preview.SetPosition(1, tip);

                if (UnderwaterInput.FireUp)
                    Release();
            }
            else
            {
                preview.enabled = false;
            }
        }

        // The hint runs in LateUpdate so it overrides the phone and gun hints
        private void LateUpdate()
        {
            if (flow == null || IsPowered || !nearPanel)
                return;

            if (selected != null)
            {
                PowerTerminal partner = FindPartner(selected);
                flow.SetHint(partner != null
                    ? "Dragging the " + PowerTerminal.NameOf(selected.colorIndex) + " wire. Release to connect it."
                    : "Release the wire?");
            }
            else
            {
                flow.SetHint("Grab a wire (" + connected + "/" + (terminals.Count / 2) + " connected)");
            }
        }

        private PowerTerminal FindNearest(Vector3 direction, Vector3 origin, bool requireUnwired)
        {
            PowerTerminal best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < terminals.Count; i++)
            {
                PowerTerminal terminal = terminals[i];

                if (terminal == null)
                    continue;

                if (requireUnwired && terminal.wired)
                    continue;

                Vector3 to = terminal.transform.position - origin;
                float distance = to.magnitude;

                if (distance > workRange)
                    continue;

                float angle = Vector3.Angle(direction, to);

                // Prefer angle, then distance
                float score = angle * 2f + distance;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = terminal;
                }
            }

            return best;
        }

        private PowerTerminal FindPartner(PowerTerminal terminal)
        {
            for (int i = 0; i < terminals.Count; i++)
            {
                PowerTerminal other = terminals[i];

                if (other == null || other == terminal)
                    continue;

                if (!other.wired && other.colorIndex == terminal.colorIndex)
                    return other;
            }

            return null;
        }

        private void Release()
        {
            PowerTerminal partner = FindPartner(selected);

            if (partner == null)
            {
                Cancel("Not connected. That wire is already used.");
                return;
            }

            Connect(selected, partner);
            selected = null;
        }

        private void Connect(PowerTerminal a, PowerTerminal b)
        {
            a.SetWired(true);
            b.SetWired(true);
            a.SetDragging(false);
            b.SetDragging(false);
            connected++;

            GameObject wire = new GameObject("Wire");
            LineRenderer line = wire.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, a.transform.position);
            line.SetPosition(1, b.transform.position);
            line.widthMultiplier = 0.05f;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                Material material = new Material(shader);
                material.SetColor("_BaseColor", PowerTerminal.ColorOf(a.colorIndex));
                line.material = material;
            }

            preview.enabled = false;

            if (IsPowered)
            {
                if (flow != null)
                {
                    flow.SetHint("Power restored. Water is coming in, make the call.");
                    flow.OnGridPowered();
                }
            }
            else if (flow != null)
            {
                flow.SetHint("Connected " + connected + "/" + (terminals.Count / 2) + " pairs");
            }
        }

        private void Cancel(string message)
        {
            if (selected != null)
                selected.SetDragging(false);

            selected = null;

            if (preview != null)
                preview.enabled = false;

            if (flow != null)
                flow.SetHint(message);
        }
    }
}