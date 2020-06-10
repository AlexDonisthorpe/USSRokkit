using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    //Variables
    [SerializeField]  float levelLoadDelay = 2f;

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 300f;

    [SerializeField] AudioClip engineSFX;
    [SerializeField] AudioClip collisionSFX;
    [SerializeField] AudioClip levelClearSFX;

    [SerializeField] ParticleSystem engineVFX;
    [SerializeField] ParticleSystem collisionVFX;
    [SerializeField] ParticleSystem levelClearVFX;

    enum State { Alive, Dying, Transcending };
    State state = State.Alive;

    bool collissionsAreEnabled = true;

    // References
    Rigidbody rigidBody;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            Rotate();
        }
        RespondToDebugKeys();
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKey(KeyCode.L))
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            Debug.Log(currentSceneIndex);
            if (currentSceneIndex == 6)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                SceneManager.LoadScene(currentSceneIndex+1);
            }
        } else if (Input.GetKey(KeyCode.C)){
            collissionsAreEnabled = !collissionsAreEnabled;
        }
    }

    private void Rotate()
    {
        rigidBody.freezeRotation = true; // take manual control of rotation
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; // Resume physics control
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            engineVFX.Stop();
        }
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
        if(state != State.Alive || !collissionsAreEnabled)
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
        state = State.Transcending;
        audioSource.PlayOneShot(levelClearSFX);
        levelClearVFX.Play();
        Invoke("LoadNextScene", levelLoadDelay); // parameterise time
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        engineVFX.Stop();
        collisionVFX.Play();
        audioSource.PlayOneShot(collisionSFX);
        Invoke("LoadPreviousScene", levelLoadDelay);
    }

    private void LoadPreviousScene()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(1); // allow for more than two levels
    }
}

