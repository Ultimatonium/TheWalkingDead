using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Text wonLostScreen;
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private GameObject playerPrefab;
    private PlayerController player;
    private bool gameEndet;
    [Header("Audio")]
    [SerializeField]
    private AudioClip wonClip;
    [SerializeField]
    private AudioClip lostClip;
    [SerializeField]
    private AudioClip[] backgroundClip;

    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("Error in Singelton");
            }
            return instance;
        }
    }

    [HideInInspector]
    public GameState state;

    private static GameController instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        gameEndet = false;
    }

    private void Start()
    {
        AddBackgroundSound();
        InitGame();
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.Won:
            case GameState.Lost:
                SetWonLostConditions(state);
                break;
            case GameState.Ongoing:
                if (player.state == PlayerState.Dead) state = GameState.Lost;
                break;
            default:
                break;
        }
        //if (Input.GetKey(KeyCode.R)) InitGame(); //dont work
    }

    private void InitGame()
    {
        DestroyWorld();
        WorldGenerator.GenerateWorld();
        WorldGenerator.GenerateNavMesh();
        CreateEnemy();
        wonLostScreen.enabled = false;
        SetPlayer();
        state = GameState.Ongoing;
        player.state = PlayerState.Alive;
        wonLostScreen.enabled = false;
    }

    private void AddBackgroundSound()
    {
        AudioSource backgroundSource = gameObject.AddComponent<AudioSource>();
        backgroundSource.loop = true;
        backgroundSource.clip = backgroundClip[UnityEngine.Random.Range(0, backgroundClip.Length)];
        backgroundSource.Play();
    }

    private void SetPlayer()
    {
        foreach (GameObject startRoom in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Start"))
        {
            GameObject playerObject = Instantiate(playerPrefab, startRoom.transform.position + Vector3.up, Quaternion.identity);
            player = playerObject.GetComponent<PlayerController>();
        }
    }

    private void DestroyWorld()
    {
        foreach (GameObject item in GameObject.FindObjectsOfType<GameObject>())
        {
            if (item.tag == "DontDestroy") continue;
            Destroy(item);
        }
    }

    private void DestroyAllEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
    }

    private void CreateEnemy()
    {
        foreach (GameObject spawnpoint in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Spawnpoint"))
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                Instantiate(enemyPrefab, spawnpoint.transform.position + new Vector3(0, 1, 0), Quaternion.identity);
            }
        }
    }

    private void SetWonLostConditions(GameState state)
    {
        if (gameEndet) return;
        switch (state)
        {
            case GameState.Won:
                wonLostScreen.text = "Victory";
                PlayOneShot(wonClip);
                DestroyAllEnemies();
                break;
            case GameState.Lost:
                wonLostScreen.text = "Lost";
                PlayOneShot(lostClip);
                break;
            default:
                Debug.LogError("Gamestate not catched: " + state);
                break;
        }
        wonLostScreen.enabled = true;
        gameEndet = true;
    }

    private void PlayOneShot(AudioClip clip)
    {
        AudioSource oneShot = gameObject.AddComponent<AudioSource>();
        oneShot.PlayOneShot(clip);
        Destroy(oneShot, clip.length);
    }
}
