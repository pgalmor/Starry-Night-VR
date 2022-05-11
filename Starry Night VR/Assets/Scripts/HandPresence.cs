using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;
    public GameObject HandPrefab;
    public GameObject[] StarPrefab;

    private InputDevice targetDevice;
    private GameObject spawnedHand;
    private Animator handAnimator;
    private LineRenderer line;
    private Vector3 initLinePos, finalLinePos;
    private bool hasShot = false;

    // Start is called before the first frame update
    void Start()
    {
        TryInitialize();
    }

    void TryInitialize()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            spawnedHand = Instantiate(HandPrefab, transform);
            handAnimator = spawnedHand.GetComponent<Animator>();
            line = spawnedHand.GetComponentInParent<LineRenderer>();
        }
    }

    void UpdateHandAnimation()
    {
        if(targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!targetDevice.isValid)
        {
            TryInitialize();
        }
        else
        {
            UpdateHandAnimation();
            initLinePos = line.GetPosition(0);
            finalLinePos = line.GetPosition(1);

            if(targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                Debug.Log("Trigger Value: " + triggerValue);
                if(triggerValue >= 0.9999 && hasShot == false)
                {
                    ShootStar();
                    hasShot = true;
                }
                else if(triggerValue < 0.9999 && hasShot == true)
                {
                    hasShot = false;
                }
            }
        }
    }

    void ShootStar()
    {
        Vector3 direction = finalLinePos - initLinePos;
        //Physics.Raycast(transform.position, direction, 20);
        Vector3 newSpawnPos = transform.position + direction.normalized * 5;
        Quaternion quat = new Quaternion(1, 0, 0, 0);

        int rand = Random.Range(0, StarPrefab.Length);
        Instantiate(StarPrefab[rand], newSpawnPos, quat);
        Debug.Log("Star Shot!");
    }
}
