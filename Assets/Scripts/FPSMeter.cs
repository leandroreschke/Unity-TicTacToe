using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSMeter : MonoBehaviour
{
    int frameCount = 0;
    double dt = 0.0f;
    double fps = 0.0f;
    double updateRate = 4.0f;  // 4 updates per sec.
    
    Text fpsText;
    void Start(){
        fpsText = this.GetComponent<Text>();
    }
    void Update()
    {
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0/updateRate)
        {
            fps = frameCount / dt ;
            frameCount = 0;
            dt -= 1.0/updateRate;
        }
        if(fps > 60){
          fpsText.color = Color.green;
        }else if(fps > 30){
          fpsText.color = Color.yellow;
        }else {
          fpsText.color = Color.red;
        }
        fpsText.text = fps.ToString("F2");

    }
}
