using UnityEngine;

namespace UnderwaterOptimization
{
    public class RescueBox : MonoBehaviour
    {
        public Transform door;
        public PowerGridPanel panel;
        public float useRange = 4.5f;
        public KeyCode useKey = KeyCode.E;

        private UnderwaterFlow flow;
        private Transform playerRoot;
        private bool opened;
        private Vector3 doorClosedPosition;
        private Vector3 doorOpenPosition;

        private void Start()
        {
            flow = FindObjectOfType<UnderwaterFlow>();

            playerRoot = UnderwaterInput.PlayerRoot;

            if (panel != null)
                panel.gameObject.SetActive(false);

            if (door != null)
            {
                doorClosedPosition = door.position;
                doorOpenPosition = doorClosedPosition + new Vector3(0f, 0f, -2.4f);
            }
        }

        private void Update()
        {
            if (opened || playerRoot == null)
                return;

            float distance = Vector3.Distance(playerRoot.position, transform.position);
            if (distance > useRange)
                return;

            if (!KeyPickup.HasKey)
            {
                if (flow != null)
                    flow.SetHint("The rescue box is locked. Find the key.");
                return;
            }

            if (flow != null)
                flow.SetHint("Open the rescue box");

            if (UnderwaterInput.InteractDown)
                Open();
        }

        private void Open()
        {
            opened = true;

            if (panel != null)
                panel.gameObject.SetActive(true);

            // Remove the door so it never blocks the view or the ray
            if (door != null)
                door.gameObject.SetActive(false);

            if (flow != null)
            {
                flow.SetHint("Wiring panel inside. Drag each wire onto the matching terminal.");
                flow.SetObjective("Objective 5/7: Reconnect the three wires");
            }

        }
    }
}