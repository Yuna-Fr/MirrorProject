using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TIMER : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI lastTimeText;
    InputSystem inputs;
    Vector2 stickVector;
    float stickThreshold = 0.05f;
    bool isMoving;
    float timer;
    float lastTime;

    void Start()
    {
        inputs = new InputSystem();
		inputs.InGame.Move.Enable();
    }

    void Update()
    {
        stickVector = inputs.InGame.Move.ReadValue<Vector2>();

        isMoving = !(Mathf.Abs(stickVector.x) < stickThreshold && Mathf.Abs(stickVector.y) < stickThreshold);


        if (isMoving)
        {
            timer += Time.deltaTime;
            lastTime = timer;
        }
        else
        {
            timer = 0;
        }

        timerText.text = timer.ToString();
        lastTimeText.text = lastTime.ToString();
    }
}
