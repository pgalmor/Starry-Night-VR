using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.Oculus;

public class HandPresence : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;
    public GameObject HandPrefab;
    public GameObject[] StarPrefab;
    public GameObject WindPrefab;
    public bool isRight = true;

    private InputDevice targetDevice;
    private GameObject spawnedHand;
    private Animator handAnimator;
    private LineRenderer line;
    private Vector3 initLinePos, finalLinePos;
    private bool hasShot = false;
    private bool startWind = false;
    private Vector3 pointerPos;
    private Camera mainCam;

    GameObject lineGO;
    LineRenderer LR;
    [SerializeField]
    Material lineMat;
    int currentIndex;
    [SerializeField]
    Camera cam;
    GameObject lastLinePoint;
    [SerializeField]
    GameObject particlePrefab;
    GameObject particleOrb;
    GameObject particle;
    GameObject beam;
    ParticleSystem particlePS;
    ParticleSystem beamPS;
    [SerializeField]
    GameObject explosionParticlePrefab;
    /*GameObject explosion;
    GameObject explosionEmbers;
    GameObject explosionShock;
    ParticleSystem embersPS;*/

    // Start is called before the first frame update
    void Start()
    {
        TryInitialize();
        //lineGO = new GameObject();
        currentIndex = 0;
        if (mainCam == null)
        {
            mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        }
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

            /*Transform particleTransform = transform;
            particleTransform.position += new Vector3(-0.0212f, -0.0136f, 0.0481f);
            particleOrb = Instantiate(particlePrefab, transform);*/
            particleOrb = spawnedHand.transform.Find("StarOrb").gameObject;
            particle = particleOrb.transform.Find("Particles").gameObject;
            beam = particleOrb.transform.Find("StarBeam").gameObject;
            particlePS = particle.GetComponent<ParticleSystem>();
            beamPS = beam.GetComponent<ParticleSystem>();
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
                if (isRight)
                {
                    if (triggerValue >= 0.9999 && hasShot == false)
                    {
                        ShootStar();
                        
                        hasShot = true;
                    }
                    else if (triggerValue < 0.9999 && hasShot == true)
                    {
                        hasShot = false;
                    }
                }
                else
                {
                    if (triggerValue >= 0.9999 && startWind == false)
                    {
                        startWind = true;
                        lineGO = new GameObject();
                        Vector3 direction = finalLinePos - initLinePos;
                        pointerPos = transform.position + direction.normalized * 550;

                        LR = lineGO.AddComponent<LineRenderer>();
                        LR.startWidth = 0.2f;
                        LR.material = lineMat;

                    }
                    else if (triggerValue < 0.9999 && startWind == true)
                    {
                        startWind = false;
                        Destroy(lastLinePoint.gameObject);
                        currentIndex = 0;
                    }
                }

                if (triggerValue > 0.05)
                {
                    particleOrb.SetActive(true);
                }
                else { particleOrb.SetActive(false); }

                if(triggerValue < 0.7670194)
                {
                    beam.transform.localScale = new Vector3(triggerValue / 10, triggerValue / 10, triggerValue / 10);
                    particle.transform.localScale = new Vector3(triggerValue / 10, triggerValue / 10, triggerValue / 10);

                    particlePS.emissionRate = (triggerValue * 50) / 0.7670194f;
                }
                else 
                { 
                    beam.transform.localScale = new Vector3(0.07670194f, 0.07670194f, 0.07670194f);
                    particle.transform.localScale = new Vector3(0.07670194f, 0.07670194f, 0.07670194f);

                    particlePS.emissionRate = 50;
                }
            }

            if (startWind)
            {
                Vector3 direction = finalLinePos - initLinePos;
                Vector3 tmpPointerPos = transform.position + direction.normalized * 550;
                Vector3 dist = pointerPos - tmpPointerPos;
                float dst_sqrMag = dist.sqrMagnitude;
                if(dst_sqrMag > 160f)
                {
                    LR.SetPosition(currentIndex, tmpPointerPos);

                    if(lastLinePoint != null)
                    {
                        Vector3 linePoint = LR.GetPosition(currentIndex);
                        lastLinePoint.transform.LookAt(linePoint);
                    }

                    lastLinePoint = Instantiate(WindPrefab, LR.GetPosition(currentIndex), Quaternion.identity, lineGO.transform);
                    lastLinePoint.transform.localScale = new Vector3(10, 10, 10);
                    //lastLinePoint.transform.LookAt(new Vector3(lastLinePoint.transform.position.x, lastLinePoint.transform.position.y, mainCam.transform.position.z));

                    pointerPos = tmpPointerPos;
                    currentIndex++;
                    LR.positionCount = currentIndex + 1;
                    LR.SetPosition(currentIndex, tmpPointerPos);
                }
            }
        }
    }

    void ShootStar()
    {
        Vector3 direction = finalLinePos - initLinePos;
        //Physics.Raycast(transform.position, direction, 20);
        Vector3 particleSpawnPos = transform.position + direction.normalized * 450;
        Vector3 newSpawnPos = transform.position + direction.normalized * 500;
        Quaternion quat = new Quaternion(1, 0, 0, 0);

        int rand = Random.Range(0, StarPrefab.Length);
        float randScale = Random.Range(75, 200);

        GameObject explosionParticle = Instantiate(explosionParticlePrefab, particleSpawnPos, quat);
        explosionParticle.transform.localScale = new Vector3((randScale * 50) / 240, (randScale * 50) / 240, (randScale * 50) / 240);
        GameObject explosionShock = explosionParticle.transform.Find("Shockwave").gameObject;
        explosionShock.transform.localScale = new Vector3((randScale * 50) / 240, (randScale * 50) / 240, (randScale * 50) / 240);
        ParticleSystem explosionEmbers = explosionParticle.transform.Find("Embers").gameObject.GetComponent<ParticleSystem>();
        var velocityOverLifetime = explosionEmbers.velocityOverLifetime;
        velocityOverLifetime.speedModifier = randScale / 6;

        GameObject star = Instantiate(StarPrefab[rand], newSpawnPos, quat);
        star.transform.localScale = new Vector3(randScale, randScale, randScale);
        Debug.Log("Star Shot!");
    }

    void ShootWind()
    {

    }
}
