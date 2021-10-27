using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    Dropdown vsyncDrop;
    // Start is called before the first frame update
    void Start()
    {
      List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
      options.Add(new Dropdown.OptionData("OFF"));
      int max = (int)FindExp(30, Screen.currentResolution.refreshRate) + 1;
      for(int x = 0; x < max; x++){
        options.Add(new Dropdown.OptionData((30 * (Mathf.Pow(2, x))).ToString()));
      }
      vsyncDrop.AddOptions(options);
      vsyncDrop.value = QualitySettings.vSyncCount;
      int id = 0;
      foreach (Resolution res in Screen.resolutions)
      {
        id++;
      }
    }

    double FindExp(int power, int n) {
      int exp = 0;
      while(n != (30 * (Mathf.Pow(2, exp)))){
        exp++;
      }
      return exp;
    }

    public void VSync(Dropdown dropdown){
      QualitySettings.vSyncCount = dropdown.value == 0 ? 0 : dropdown.options.Count - dropdown.value;
      Debug.Log(QualitySettings.vSyncCount);
    }

    public void MaxFrame(Slider slider)
    {
      Debug.Log(Screen.currentResolution);
      if((int)slider.value > 300)
      {
        Application.targetFrameRate = -1;
      }else
      {
        Application.targetFrameRate = (int)slider.value;
      }
      foreach (var child in slider.GetComponentsInChildren<Text>())
      {
        if(child.transform.name == "CurrentValue")
        {
          updateText(child, (int)slider.value > 300 ? "MAX" : ((int)slider.value).ToString());
        }
      }
    }

    void updateText(Text text, string value)
    {
      text.text = value;
    }

    public void Quit()
    {
      Application.Quit();
    }
}
