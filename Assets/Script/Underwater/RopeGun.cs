using UnityEngine;

namespace UnderwaterOptimization
{
    public class RopeGun : MonoBehaviour
    {
        [Header("Feel")]
        public float pickupRange = 2.5f;
        public float maxRange = 45f;
        public float zipDuration = 1.1f;
        public float zipArc = 0.7f;
        public float heldScale = 0.32f;

        public KeyCode pickupKey = KeyCode.E;

        private UnderwaterFlow flow;
        private Transform playerRoot;
        private Camera playerCamera;
        private UnityEngine.CharacterController controller;
        private DebugPlayerMotor motor;
        private Rigidbody body;
        private LineRenderer line;
        private TextMesh label;
        private Transform aimDot;
        private Material aimMaterial;
        private LineRenderer laser;
        private Material laserMaterial;

        private bool held;
        private bool zipping;
        private Vector3 zipFrom;
        private Vector3 zipEnd;
        private Vector3 anchorPoint;
        private float zipT;
        private float messageUntil;
        private bool wasInRange;

        private void Start()
        {
            flow = FindObjectOfType<UnderwaterFlow>();
            body = GetComponent<Rigidbody>();

            playerRoot = UnderwaterInput.PlayerRoot;
            playerCamera = UnderwaterInput.PlayerCamera;

            BuildLine();
            BuildLabel();
            BuildAimDot();
            BuildLaser();
        }

        private void BuildLabel()
        {
            GameObject go = new GameObject("RopeGunLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.55f, 0f);

            label = go.AddComponent<TextMesh>();
            label.text = "Rope gun";
            label.fontSize = 48;
            label.characterSize = 0.055f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(0.95f, 0.9f, 0.75f, 1f);

            Font font = UnderwaterFont.Get();
            if (font != null)
            {
                label.font = font;
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null && font.material != null)
                    renderer.sharedMaterial = font.material;
            }
        }

        private void BuildLine()
        {
            line = gameObject.GetComponent<LineRenderer>();
            if (line == null)
                line = gameObject.AddComponent<LineRenderer>();

            line.positionCount = 2;
            line.widthMultiplier = 0.045f;
            line.useWorldSpace = true;
            line.enabled = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.8f, 0.85f, 0.8f, 1f));
                line.material = mat;
            }
        }

        private void Update()
        {
            if (playerRoot == null || playerCamera == null)
                return;

            if (zipping)
            {
                UpdateZip();
                return;
            }

            if (!held)
            {
                UpdatePickup();
                UpdateLabel();
                return;
            }

            HoldInHand();
            UpdateAim();

            if (UnderwaterInput.FireDown)
                Fire();
        }

        private void UpdateAim()
        {
            if (aimDot == null)
                return;

            Ray ray = new Ray(UnderwaterInput.RayOrigin, UnderwaterInput.RayDirection);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, maxRange, ~0, QueryTriggerInteraction.Ignore))
            {
                aimDot.gameObject.SetActive(false);
                if (laser != null)
                    laser.enabled = false;
                if (flow != null)
                    flow.SetAimInfo("", false);
                return;
            }

            aimDot.gameObject.SetActive(true);
            aimDot.position = hit.point + hit.normal * 0.03f;

            // While aiming at a terminal, leave the UI to the wiring logic
            if (hit.collider.GetComponentInParent<PowerTerminal>() != null)
            {
                aimDot.gameObject.SetActive(false);
                if (laser != null)
                    laser.enabled = false;
                return;
            }

            float distance = Mathf.Max(1f, hit.distance);
            float scale = Mathf.Clamp(distance * 0.02f, 0.18f, 0.9f);
            aimDot.localScale = Vector3.one * scale;

            bool onAnchor = hit.collider.GetComponentInParent<RopeAnchor>() != null;
            Color color = onAnchor ? new Color(3f, 1.2f, 0.25f) : new Color(1.2f, 1.35f, 1.5f, 1f);

            if (aimMaterial != null)
                aimMaterial.SetColor("_BaseColor", color);

            if (laser != null)
            {
                laser.enabled = true;
                laser.SetPosition(0, transform.position + transform.forward * 0.28f);
                laser.SetPosition(1, hit.point);
                if (laserMaterial != null)
                    laserMaterial.SetColor("_BaseColor", color);
            }

            if (flow != null && Time.time > messageUntil)
                flow.SetHint(onAnchor ? "Anchor locked. Ready to fire." : "Aim at the orange anchor to hook it.");

            if (flow != null)
                flow.SetAimInfo(onAnchor
                    ? "✔ Anchor · " + hit.distance.ToString("F1") + " m"
                    : "Range " + hit.distance.ToString("F1") + " m", onAnchor);
        }

        private void BuildAimDot()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "AimDot";
            go.transform.SetParent(null);

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            Renderer renderer = go.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                aimMaterial = new Material(shader);
                aimMaterial.SetColor("_BaseColor", new Color(3f, 1.2f, 0.25f));
                if (renderer != null)
                    renderer.sharedMaterial = aimMaterial;
            }

            aimDot = go.transform;
            aimDot.gameObject.SetActive(false);
        }

        private void BuildLaser()
        {
            GameObject go = new GameObject("AimLaser");
            laser = go.AddComponent<LineRenderer>();
            laser.positionCount = 2;
            laser.widthMultiplier = 0.03f;
            laser.useWorldSpace = true;
            laser.enabled = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                laserMaterial = new Material(shader);
                laserMaterial.SetColor("_BaseColor", new Color(1.2f, 1.35f, 1.5f, 1f));
                laser.material = laserMaterial;
            }
        }

        private void UpdatePickup()
        {
            float distance = Vector3.Distance(playerRoot.position, transform.position);
            bool inRange = distance <= pickupRange;

            if (inRange && flow != null)
                flow.SetHint("Pick up the rope gun");

            if (inRange && UnderwaterInput.InteractDown)
            {
                held = true;
                if (body != null)
                {
                    body.isKinematic = true;
                    body.detectCollisions = false;
                }

                if (label != null)
                    label.gameObject.SetActive(false);

                // The held gun must not block the ray
                Collider[] colliders = GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                    colliders[i].enabled = false;

                transform.localScale = Vector3.one * heldScale;

                if (flow != null)
                    flow.SetHint("Fire the rope at the orange anchor above.");
                flow.SetObjective("Objective 7/7: Aim at the orange anchor and fire");
            }

            wasInRange = inRange;
        }

        private void UpdateLabel()
        {
            if (label == null || held)
                return;

            label.transform.position = transform.position + Vector3.up * 0.55f;

            if (playerCamera == null)
                return;

            // Always face the player
            label.transform.rotation = playerCamera.transform.rotation;
        }

        private void HoldInHand()
        {
            Transform cam = playerCamera.transform;

            // Small in-hand gun: scaled down, bottom right, clear of the crosshair
            transform.localScale = Vector3.one * heldScale;
            transform.position = cam.position + cam.forward * 0.55f + cam.right * 0.34f - cam.up * 0.32f;
            transform.rotation = cam.rotation * Quaternion.Euler(0f, -18f, 0f);
        }

        private void Fire()
        {
            Ray ray = new Ray(UnderwaterInput.RayOrigin, UnderwaterInput.RayDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRange, ~0, QueryTriggerInteraction.Ignore))
            {
                // Do not fire while aiming at a terminal
                if (hit.collider.GetComponentInParent<PowerTerminal>() != null)
                    return;

                RopeAnchor anchor = hit.collider.GetComponentInParent<RopeAnchor>();
                if (anchor != null)
                {
                    StartZip(anchor, hit.point);
                    return;
                }
            }

            if (flow != null)
                flow.SetHint("Missed. Aim at the orange anchor above.");
            messageUntil = Time.time + 1.5f;
        }

        private void StartZip(RopeAnchor anchor, Vector3 hitPoint)
        {
            zipping = true;
            zipT = 0f;
            anchorPoint = hitPoint;
            zipFrom = playerRoot.position;
            zipEnd = anchor.landingPoint != null ? anchor.landingPoint.position : hitPoint;

            if (controller != null)
                controller.enabled = false;
            if (motor != null)
                motor.enabled = false;

            if (line != null)
                line.enabled = true;

            if (flow != null)
                flow.SetHint("Rope attached!");
        }

        private void UpdateZip()
        {
            zipT += Time.deltaTime / Mathf.Max(0.05f, zipDuration);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(zipT));

            Vector3 position = Vector3.Lerp(zipFrom, zipEnd, t);
            position.y += Mathf.Sin(t * Mathf.PI) * zipArc;
            playerRoot.position = position;

            if (line != null)
            {
                line.SetPosition(0, transform.position);
                line.SetPosition(1, anchorPoint);
            }

            if (zipT < 1f)
                return;

            zipping = false;
            if (line != null)
                line.enabled = false;

            if (controller != null)
                controller.enabled = true;
            if (motor != null)
                motor.enabled = true;

            if (flow != null)
                flow.SubmitEscape();
        }
    }
}
