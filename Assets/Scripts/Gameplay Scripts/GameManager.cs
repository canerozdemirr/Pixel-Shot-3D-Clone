using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Touch;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameUi, winUi, failUi, ballCountUi;
    public Slider cubeNumberSlider;

    public Button winButton, failButton;
    public static GameManager Instance { get; private set; }
    public List<GameObject> allObstacles;

    public int ballCount;
    public float cubePushPower;

    public bool isGameOver, isLevelSucceeded;

    public GameObject leanTouch;

    private TextMeshProUGUI _ballCountText;

    private void Awake()
    {
        allObstacles = GameObject.FindGameObjectsWithTag("Cube").ToList();
        Instance = this;
        gameUi = GameObject.FindGameObjectWithTag(Tags.GameUiTag);
        winUi = gameUi.transform.GetChild(1).gameObject;
        failUi = gameUi.transform.GetChild(2).gameObject;
        cubeNumberSlider = gameUi.transform.GetChild(3).GetComponent<Slider>();
        ballCountUi = gameUi.transform.GetChild(4).gameObject;
        winButton = winUi.GetComponent<Button>();
        failButton = failUi.GetComponent<Button>();
        winButton.onClick.AddListener(LoadNextScene);
        failButton.onClick.AddListener(ReloadTheScene);
        leanTouch = GameObject.FindGameObjectWithTag("LeanTouch");
        cubeNumberSlider.maxValue = allObstacles.Count;
        _ballCountText = ballCountUi.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _ballCountText.text = ballCount.ToString();
        StartCoroutine(GameOverCheck());
    }

    private void Update()
    {
        if (allObstacles.Count == 0 && !winUi.activeInHierarchy && !isLevelSucceeded && !isGameOver)
        {
            isLevelSucceeded = true;
            cubeNumberSlider.gameObject.SetActive(false);
            ballCountUi.SetActive(false);
            winUi.SetActive(true);
        }

        if (isGameOver && !failUi.activeInHierarchy && !isLevelSucceeded)
        {
            cubeNumberSlider.gameObject.SetActive(false);
            ballCountUi.SetActive(false);
            failUi.SetActive(true);
        }
        if (Input.GetMouseButtonUp(0) && LeanDragLine.Instance.canShoot)
        {
            if (ballCount > 0)
            {
                ballCount--;
                _ballCountText.text = ballCount.ToString();
            }
            if(ballCount == 0 || allObstacles.Count == 0) leanTouch.SetActive(false);
        }
    }
    
    private void LoadNextScene()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2) SceneManager.LoadScene(0);
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void ReloadTheScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator GameOverCheck()
    {
        yield return new WaitUntil(() => ballCount == 0);
        yield return new WaitForSeconds(2.5f);
        if (allObstacles.Count > 0) isGameOver = true;
    }
}
