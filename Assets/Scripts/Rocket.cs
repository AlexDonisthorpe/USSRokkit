using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    //Variables
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 300f;

    [SerializeField] AudioClip engineSFX;
    [SerializeField] AudioClip collisionSFX;
    [SerializeField] AudioClip levelClearSFX;

    [SerializeField] ParticleSystem engineVFX;
    [SerializeField] ParticleSystem collisionVFX;
    [SerializeField] ParticleSystem levelClearVFX;

    bool isTransitioning = false;
    int currentSceneIndex;

    bool collisionsDisabled = false;

    // References
    Rigidbody rigidBody;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTransitioning)
        {
            RespondToThrustInput();
            Rotate();
        }
        if (Debug.isDebugBuild) { 
        RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKey(KeyCode.L))
        {
           LoadNextScene();
        } else if (Input.GetKey(KeyCode.C)){
            collisionsDisabled = !collisionsDisabled;
        }
    }

    private void Rotate()
    {
        rigidBody.angularVelocity = Vector3.zero; // remove rotation due to physics
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
        }

    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            StopApplyingThrust();
        }
    }

    private void StopApplyingThrust()
    {
        audioSource.Stop();
        engineVFX.Stop();
    }

    private void ApplyThrust()
    {
        float thrustThisFrame = mainThrust * Time.deltaTime;
        rigidBody.AddRelativeForce(Vector3.up * thrustThisFrame);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(engineSFX);
        }
        engineVFX.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(isTransitioning || collisionsDisabled)
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        isTransitioning = true;
        audioSource.PlayOneShot(levelClearSFX);
        levelClearVFX.Play();
        Invoke("LoadNextScene", levelLoadDelay); // parameterise time
    }

    private void StartDeathSequence()
    {
        isTransitioning = true;
        audioSource.Stop();
        engineVFX.Stop();
        collisionVFX.Play();
        audioSource.PlayOneShot(collisionSFX);
        Invoke("LoadCurrentScene", levelLoadDelay);
    }

    private void LoadFirstScene()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {
        if (currentSceneIndex == SceneManager.sceneCountInBuildSettings-1)
        {
            LoadFirstScene();
        }
        else
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }

    private void LoadCurrentScene()
    {
        SceneManager.LoadScene(currentSceneIndex);
    }
}

