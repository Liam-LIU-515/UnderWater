using UnityEngine;

namespace UnderwaterOptimization
{
    public class FlashlightQuest : MonoBehaviour
    {
        public UnderwaterFlow flow;
        public GameObject keyObject;

        private Light playerLight;
        private UnderwaterFlashlight toggle;
        private bool hasBroken;
        private bool hasBattery;

        public bool HasLight
        {
            get { return playerLight != null && playerLight.enabled; }
        }

        public GameObject batteryObject;

        private void Start()
        {
            if (keyObject != null)
                keyObject.SetActive(false);

            // The battery does not exist at first: it appears once the torch is picked up
            if (batteryObject == null)
            {
                QuestItem[] items = FindObjectsOfType<QuestItem>(true);
                foreach (QuestItem item in items)
                {
                    if (item.type == QuestItemType.Battery)
                    {
                        batteryObject = item.gameObject;
                        break;
                    }
                }
            }

            if (batteryObject != null)
                batteryObject.SetActive(false);

            toggle = Object.FindObjectOfType<UnderwaterFlashlight>();
            if (toggle != null)
                toggle.enabled = false;

            playerLight = FindPlayerLight();
            if (playerLight != null)
                playerLight.enabled = false;
        }

        private Light FindPlayerLight()
        {
            GameObject existing = GameObject.Find("Underwater_Flashlight");
            if (existing != null)
            {
                Light light = existing.GetComponentInChildren<Light>();
                if (light != null)
                    return light;
            }

            // No torch exists in VR, so create one on the camera
            Camera cam = Camera.main;
            if (cam == null)
                return null;

            GameObject go = new GameObject("QuestFlashlight");
            go.transform.SetParent(cam.transform, false);
            go.transform.localPosition = new Vector3(0.15f, -0.1f, 0.2f);

            Light created = go.AddComponent<Light>();
            created.type = LightType.Spot;
            created.color = new Color(1f, 0.94f, 0.82f);
            created.intensity = 260f;
            created.range = 30f;
            created.spotAngle = 62f;
            created.shadows = LightShadows.Soft;
            return created;
        }

        public void Pick(QuestItemType type)
        {
            if (type == QuestItemType.BrokenFlashlight)
                hasBroken = true;
            else
                hasBattery = true;

            if (hasBroken && hasBattery)
            {
                Assemble();
                return;
            }

            // The battery appears only after the torch is picked up
            if (type == QuestItemType.BrokenFlashlight && batteryObject != null)
                batteryObject.SetActive(true);

            if (flow == null)
                return;

            flow.SetHint(type == QuestItemType.BrokenFlashlight
                ? "Broken torch picked up. Still missing a battery."
                : "Battery picked up. Fit it into the torch.");

            if (flow != null && type == QuestItemType.BrokenFlashlight)
                flow.SetObjective("Objective 2/7: Find the battery");
        }

        private void Assemble()
        {
            if (toggle != null)
                toggle.enabled = true;

            if (playerLight != null)
            {
                playerLight.enabled = true;
                // Keep it soft: lower intensity, wider cone
                playerLight.intensity = 300f;
                playerLight.range = 26f;
                playerLight.spotAngle = 62f;
            }

            if (keyObject != null)
                keyObject.SetActive(true);

            if (flow != null)
                flow.SetHint("The torch is on. Now the key in the corner is visible.");
                flow.SetObjective("Objective 3/7: With light, find the key");
                flow.OnFlashlightAssembled();

        }
    }
}