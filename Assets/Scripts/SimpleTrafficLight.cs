using System.Collections;
using System;
using UnityEngine;

public class SimpleTrafficLight : MonoBehaviour
{
    public float greenTime = 5f;
    public float yellowTime = 2f;
    public float redTime = 5f;
    public ColorState startWith;

    public enum ColorState { green, yellow, red };

    public ColorState currentState { get { return _currentState; } }

    private ColorState _currentState;

    void Start()
    {
        _currentState = (ColorState)(((int)startWith + 2) % 3);
        StartCoroutine("ColorTimer");
    }

    void TurnGreen()
    {
        _currentState = ColorState.green;

        this.transform.GetChild(0).GetComponent<Renderer>().
            material.color = Color.green;
        
    }

    void TurnYellow()
    {
        _currentState = ColorState.yellow;

        this.transform.GetChild(0).GetComponent<Renderer>().
            material.color = Color.yellow;
    }

    void TurnRed()
    {
        _currentState = ColorState.red;

        this.transform.GetChild(0).GetComponent<Renderer>().
            material.color = Color.red;
    }

    IEnumerator ColorTimer()
    {
        while(true) 
        {
            switch(_currentState)
            {
                case ColorState.green:
                    TurnYellow();
			        yield return new WaitForSeconds(yellowTime);
                    break;
                case ColorState.yellow:
                    TurnRed();
			        yield return new WaitForSeconds(redTime);
                    break;
                case ColorState.red:
                    TurnGreen();
			        yield return new WaitForSeconds(greenTime);
                    break;
            }
		}
    }
}
