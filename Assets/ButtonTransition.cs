using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
public class ButtonTransition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
 
  public void OnPointerDown(PointerEventData eventData){
    this.GetComponentInChildren<Text>().enabled = false;
  }
  
  public void OnPointerUp(PointerEventData eventData){
    this.GetComponentInChildren<Text>().enabled = true;
  }
}