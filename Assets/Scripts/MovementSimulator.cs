using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovementSimulator : MonoBehaviour
{
    private Scene parallelScene;
    private PhysicsScene parallelPhysicsScene;
    
    public Vector3 initialVelocity;
    public GameObject ball;
    public GameObject environment;
    public GameObject obstacles;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        Physics.autoSimulation = false;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 1000;

        var createSceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        parallelScene = SceneManager.CreateScene("ParallelScene", createSceneParameters);
        parallelPhysicsScene = parallelScene.GetPhysicsScene();
    }

    private void SimulatePhysics()
    {
        var simulationObject = Instantiate(LeanSpawnBetween.Instance.Prefab.gameObject);
        var simulationEnvironment = Instantiate(environment);
        var simulationObstacles = Instantiate(obstacles);

        SceneManager.MoveGameObjectToScene(simulationObject, parallelScene);
        SceneManager.MoveGameObjectToScene(simulationEnvironment, parallelScene);
        SceneManager.MoveGameObjectToScene(simulationObstacles, parallelScene);
    }
}
