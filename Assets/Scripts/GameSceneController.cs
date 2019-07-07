using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour {
    private int enemyDeathCount;
    public ShakeCamera shakeCamera;
    public Character mainCharacter;
    public EnemyPool enemyPool;
    public BulletPool bulletPool;

    public WaveScriptable stageWaves;

    public GameObject sanityItemPrefab;
    public Sprite sanityFull, sanityEmpty;
    public Transform sanityParent;

    public Animator countdownAnimator;

    public WaveScriptable tutorialWaveScriptable;
    public WaveScriptable stage1WaveScriptable;

    public GameObject overCanvasTrue;
    public GameObject overCanvasDied;

    [Header("Debug")]
    public WaveScriptable[] waveScriptables;
    public Text waveHint;

    public bool gameRunning { get; set; }

    public static GameSceneController instance;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogWarning("出现了两个 GameSceneController");
        }
        gameRunning = false;
    }

    public Image fadeInImage;
    public Text fadeInHint;
    private int stageTotalEnemyCount = 0;

    void Start() {
        // 判断使用哪一个关卡
        if (GameManager.Instance.shouldGotoTutorial) {
            stageWaves = tutorialWaveScriptable;
        } else {
            stageWaves = stage1WaveScriptable;
        }
        // 计算总怪数
        foreach (var wave in stageWaves.waves) {
            stageTotalEnemyCount += wave.waveEles.Length;
        }

        FadeInEffect(()=>{
            RealStart();
        });
    }

    public void FadeInEffect(Action action) {
        // 渐入
        fadeInImage.transform.parent.gameObject.SetActive(true);
        Delay(()=>{
            if (action != null)
                action();
            fadeInImage.transform.parent.gameObject.SetActive(false);
        }, 1f);
    }

    void RealStart() {
        // 倒计时
        countdownAnimator.gameObject.SetActive(true);
        countdownAnimator.Play("Countdown", 0, 0f);
        Delay(()=>{
            countdownAnimator.gameObject.SetActive(false);
            StartGame();
        }, 3f);
    }

    float gameStartTime = 0f;
    float elapsedTime = 0f;
    void Update() {
        if (gameRunning) {
            elapsedTime = Time.time - gameStartTime;
            // 检测是否该生成下一波怪
            CheckSpawn();
        }
        // if (Input.GetKeyDown(KeyCode.F) && !gameRunning) {
        //     gameRunning = true;
        //     gameStartTime = Time.time;
        //     StartGame();
        // }

        // For debug
        if (Input.GetKeyDown("1")) {
            stageWaves = waveScriptables[0];
        }
        if (Input.GetKeyDown("2")) {
            stageWaves = waveScriptables[1];
        }
        if (Input.GetKeyDown("3")) {
            stageWaves = waveScriptables[2];
        }
        if (Input.GetKeyDown("4")) {
            stageWaves = waveScriptables[3];
        }
        waveHint.text = stageWaves.name;
    }

    void CheckSpawn() {
        if (currentWaveIndex + 1 >= stageWaves.waves.Length) {
            // 游戏结束
            gameRunning = false;
            return;
        }
        var nextWave = stageWaves.waves[currentWaveIndex + 1];
        if (nextWave.time <= elapsedTime) {
            Debug.Log(nextWave.time);
            Debug.Log(currentWaveIndex);
            // 刷这一波
            SpawnWave(nextWave);
            currentWaveIndex = currentWaveIndex + 1;
            Debug.Log(currentWaveIndex);
        }
    }

    private int currentWaveIndex = -1;

    void SpawnWave(WaveScriptable.Wave wave) {
        foreach (var waveEle in wave.waveEles) {
            GameObject enemy = null;
            Debug.Log(waveEle.enemyType);
            enemy = enemyPool.getOneInstance(waveEle.enemyType, waveEle.isRange);
            enemy.transform.position = stageWaves.positions[waveEle.positionIndex];
            // 设置 Enemy 的 character
            enemy.GetComponent<Enemy>().setCharacter(mainCharacter.gameObject);
        }
    }

    public void StartGame() {
        // Game Init
        Debug.Log("GAME START!");
        InitGame();
        gameRunning = true;
        gameStartTime = Time.time;

        // 根据关卡选择BGM
        if (GameManager.Instance.shouldGotoTutorial) {
            AudioInterface.Instance.playBGM(AudioInterface.Instance.Stage0BGM);
        } else {
            AudioInterface.Instance.playBGM(AudioInterface.Instance.Stage1BGM);
        }
    }

    private Image[] sanityItems;

    public void InitGame() {
        // 初始化 San 值
        sanityItems = new Image[9];
        for (int i = 0; i < 9; ++i) {
            sanityItems[i] = Instantiate<GameObject>(sanityItemPrefab, sanityParent).GetComponent<Image>();
        }
        ChangeSanity(mainCharacter.sanity);
        // 初始化敌人死亡计数
        enemyDeathCount = 0;
    }

    public void ChangeSanity(int sanity) {
        for (int i = 0; i < sanity; ++i) {
            sanityItems[i].sprite = sanityFull;
        }
        for (int i = sanity; i < 9; ++i) {
            sanityItems[i].sprite = sanityEmpty;
        }
    }

    void Delay(Action action, float time) {
        StartCoroutine(_Delay(action, time));
    }

    IEnumerator _Delay(Action action, float time) {
        yield return new WaitForSeconds(time);
        action();
    }

    public void OnEnemyDeath(Enemy enemy) {
        ++enemyDeathCount;
        if (currentWaveIndex == stageWaves.waves.Length - 1) {
            // 如果已经在最后一波，判断是否所有怪清完
            Debug.Log("EnemyDeathCount " + enemyDeathCount + ", stageTotalEnemyCount " + stageTotalEnemyCount);
            if (enemyDeathCount == stageTotalEnemyCount) {
                Win();
            }
        }
    }

    public void OnPlayerDeath(Character character) {
        Fail();
    }

    public void Win() {
        if (GameManager.Instance.shouldGotoTutorial) {
            GameManager.Instance.shouldGotoTutorial = false;
            fadeInHint.gameObject.SetActive(true);
            FadeInEffect(null);
            Delay(()=>{
                SceneManager.LoadScene("GameScene");
            }, 1f);
        } else {
            FadeInEffect(null);
            overCanvasTrue.SetActive(true);
        }
        gameRunning = false;
    }

    public void Fail() {
        FadeInEffect(null);
        overCanvasDied.SetActive(true);
        gameRunning = false;
    }

    #region CLICK_HANDLE

    public void BackToTitle() {
        GameManager.Instance.shouldGotoTutorial = true;
        SceneManager.LoadScene("TitleScene");
    }

    public void Restart() {
        SceneManager.LoadScene("GameScene");
    }

    #endregion
}
