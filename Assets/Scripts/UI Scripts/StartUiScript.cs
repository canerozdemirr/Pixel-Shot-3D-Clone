using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartUiScript : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.cubeNumberSlider.gameObject.SetActive(true);
        GameManager.Instance.ballCountUi.SetActive(true);
        gameObject.SetActive(false);
    }
}
