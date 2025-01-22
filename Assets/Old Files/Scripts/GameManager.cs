using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] gameIntractables;

    private float rotationAngle = 90f;
    private Vector3 rotationAxis = new Vector3(0, 0, 1);

    CheckConnections _checkConnections;

    GameObject battery;
    GameObject bulb;

    bool Is_Level_Completed = false;

    [SerializeField]UIManager _UIManager;

    void Start()
    {
        GameObject[] wires = GameObject.FindGameObjectsWithTag("Wire");
        battery = GameObject.FindWithTag("Battery");
        bulb = GameObject.FindWithTag("Bulb");

        if (bulb.GetComponent<CheckConnections>().isLevelCompleted)
        {
            Is_Level_Completed = true;
        }
        else
        {
            Is_Level_Completed = false;
        }

        List<GameObject> collectedObjects = new List<GameObject>(wires);

        if (battery != null) collectedObjects.Add(battery);
        if (bulb != null) collectedObjects.Add(bulb);

        gameIntractables = collectedObjects.ToArray();

        Debug.Log(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (!Is_Level_Completed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                if (hit.collider != null)
                {
                    if (hit.transform.CompareTag("Wire") || hit.transform.CompareTag("Bulb") || hit.transform.CompareTag("Battery"))
                    {
                        _checkConnections = hit.transform.GetComponent<CheckConnections>();

                        if (!_checkConnections.isRotating)
                        {
                            StartCoroutine(_checkConnections.RotateObject(rotationAxis, rotationAngle));
                        }

                    }
                }
            }
        }

        if (bulb.GetComponent<CheckConnections>().isLevelCompleted)
        {
            Is_Level_Completed = true;

            _UIManager.LevelCompleted();
        }
    }
}