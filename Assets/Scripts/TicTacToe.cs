using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

public class TicTacToe : MonoBehaviour
{

  PlayerControls controls; //Holds player controller from the new input system

  //just variables for the game algorithm
  public string currentPlayer = "X", winner = ""; 
  List<List<string>> board = new List<List<string>>();
  public int boardSize = 3;

  public float slotSpaceFactor = 0f; //holds the space factor between slots

  public Color player1Color, player2Color; //player colors

  public bool isPaused = false; //boolean for pause the game

  //some gameobject imports
  public GameObject mainCanvas, mainMenu, sceneTransPrefab, slotFramePrefab, slotPrefab, slotP1Prefab, slotP2Prefab, hitPrefab, playerCamera; 

  //array of audio clips to add variety to the sounds
  public AudioClip[] lasers, clicks;

  public AudioClip explosion;//explosion sound

  public float radius = 5.0F; //explosion radius
  public float power = 10.0F; //explosion power

  bool canReset = false; //limit the time between resets, prevent lag

  void Awake()
  {
    controls = new PlayerControls(); //initialize player control

    //add click sound to all buttons in the game
    Button[] buttons = (Button[])Resources.FindObjectsOfTypeAll<Button>();
    foreach (Button button in buttons)
    {
      button.GetComponent<Button>().onClick.AddListener(() => ButtonClick(.1f));
    }
  
  }

  // Start is called before the first frame update
  void Start()
  {
    //nice transition to the game
    StartCoroutine("FadeIn");
    SetupBoard(); //setup the game
  }

  //setup the board/game
  public void SetupBoard()
  {
    //cleanup game

    currentPlayer = winner.Equals("X") ? "O" : "X";

    winner = "";
    

    for (int qiy = 0; qiy < boardSize; qiy++)
    {
      List<string> rowTemp = new List<string>();
      for (int x = 0; x < boardSize; x++)
      {
        rowTemp.Add("");
        GameObject slotFrame = Instantiate(slotFramePrefab, new Vector3((qiy * slotSpaceFactor) - ((boardSize / 2) * slotSpaceFactor), x * slotSpaceFactor - ((boardSize / 2) * slotSpaceFactor), 0), slotFramePrefab.transform.rotation);
        slotFrame.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white); // just to create a new instance

        Quaternion slotRotation = slotPrefab.transform.rotation * Quaternion.Euler (180 * Random.Range(1, 3), 90 * Random.Range(0, 4), 180 * Random.Range(1, 3));
        GameObject slot = Instantiate(slotPrefab, slotFrame.transform.position, slotRotation);
        slot.name = "slot";
        slot.transform.SetParent(slotFrame.transform);
        slotFrame.name = qiy + "," + x;
      }
      board.Add(rowTemp);
    }

    canReset = true;
  }


  //method called upon player mouse click, handles the raycast and particle stuff
  void Action()
  {
    if(!isPaused && canReset){
      Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit))
      {
        if (hit.transform.gameObject.tag.Equals("Slot"))
        {
          string[] position = hit.transform.name.Split(",");
          GameObject slot = hit.transform.gameObject;
          GameObject hitParticle = Instantiate(hitPrefab, hit.point, hitPrefab.transform.rotation);
          RandomPlay(lasers, .2f);
          Color playerColor = Color.white;
          if (currentPlayer.Equals("X"))
          {
            playerColor = player1Color * Mathf.Pow(2, 3f);
          }
          else
          {
            playerColor = player2Color * Mathf.Pow(2, 3f);
          }

          hitParticle.GetComponent<VisualEffect>().SetVector4("Flash_Color", playerColor);
          GameObject.Destroy(hitParticle, 1f);

          Place(slot, int.Parse(position[0]), int.Parse(position[1]));
        }

      }
    }
  }

  //handle player move
  void Place(GameObject slotFrame, int row, int col)
  {
    if (winner.Equals(""))
    {
      if (board[row][col].Equals(""))
      {
        board[row][col] = currentPlayer;

        Color playerColor = Color.white;
        GameObject playerPrefab = null;
        if (currentPlayer.Equals("X"))
        {
          playerColor = player1Color * Mathf.Pow(2, 1.5f);
          playerPrefab = slotP1Prefab;
        }
        else
        {
          playerColor = player2Color * Mathf.Pow(2, 1.5f);
          playerPrefab = slotP2Prefab;
        }
        
        foreach (Transform child in slotFrame.transform)
        {
          if (child.name.Equals("slot"))
          {
            foreach (Transform childOfChild in child)
            { 
              foreach(Transform plank in childOfChild){
                ExplodeGO(plank.gameObject, slotFrame.transform.position);
              }
            
            }
            GameObject.Destroy(child.gameObject, 10f);
          }
        }

        //instantiate player object, either a X or an O
        GameObject playerSlot = Instantiate(playerPrefab, slotFrame.transform.position, playerPrefab.transform.rotation);
        playerSlot.name = playerSlot.name.Replace("(clone)", "").Trim();
        playerSlot.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
        playerSlot.GetComponent<Renderer>().material.SetColor("_EmissionColor", playerColor);

        playerSlot.transform.SetParent(slotFrame.transform);

        //render some nice reflections one time.
        slotFrame.GetComponentInChildren<ReflectionProbe>().RenderProbe();
        
        //check if current player won
        checkWinner(row, col);

        //change player
        if (winner.Equals(""))
        {
          currentPlayer = currentPlayer.Equals("X") ? "O" : "X";
        }
      }
    }
  }

  /*Algorithm to check the winner, actually I've never done a tic tac toe before, so I found a similar on the web and just tried to make it better,
  shorter and less complicated */
  void checkWinner(int rowP, int colP)
  {
    int row = 0, col = 0;
    for (int i = 0; i < 5; i++)
    {
      if (!winner.Equals(""))
      {
        break;
      }
      else
      {
        //goes through the board
        List<string> winnerBlocks = new List<string>();
        for (int j = 0; j < boardSize; j++)
        {
          //first case for rows, second for columns, third one for diagonals, fourth one for the anti diagonal and last for the draw
          switch (i)
          {
            case 0:
              row = j;
              col = colP;
              break;
            case 1:
              row = rowP;
              col = j;
              break;
            case 2:
              row = j;
              col = j;
              break;
            case 3:
              row = j;
              col = (boardSize - 1) - j;
              break;
            case 4:
              bool draw = true;
              foreach (List<string> rowTemp in board)
              {
                if (draw) { draw = !rowTemp.Contains(""); }
              }

              if (draw)
              {
                winner = "Draw";
              }
              break;
          }

          if (!board[row][col].Equals(currentPlayer) || !winner.Equals(""))
          {
            break;
          }else{
            winnerBlocks.Add(row + "," + col);
          }

          if (j == (boardSize - 1))
          {
            winner = currentPlayer;
            GameObject[] slots = GameObject.FindGameObjectsWithTag("Slot");
            PlayAudio(explosion, .15f);
            foreach (GameObject slot in slots)
            {
              if(!winnerBlocks.Contains(slot.name)){
                slot.gameObject.tag = "Untagged";
                // slot.GetComponent<Collider>().enabled = false;
                foreach (Transform child in slot.transform)
                {
                  
                  if (child.name.Equals("slot"))
                  {
                    foreach (Transform childOfChild in child)
                    { 
                      foreach(Transform plank in childOfChild){
                        ExplodeGO(plank.gameObject, slot.transform.position);
                      }
                    }
                  }
                }
                GameObject.Destroy(slot, 4f);
                ExplodeGO(slot, slot.transform.position);
              }else{
                Color playerColor = Color.white;
                if (currentPlayer.Equals("X"))
                {
                  playerColor = player1Color * Mathf.Pow(2, 1.5f);
                }
                else
                {
                  playerColor = player2Color * Mathf.Pow(2, 1.5f);
                }
                slot.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                slot.GetComponent<Renderer>().material.SetColor("_EmissionColor", playerColor);
              }
            }
          }
        }
      }
    }
  }

   //just for the reset button
  public void ResetBoard()
  {
    if(canReset){
      StartCoroutine("ExplodeEverything");
    }
  }

  //simple method to explode everything on reset
  IEnumerator ExplodeEverything(){
    canReset = false;
    board = new List<List<string>>();
    GameObject[] slots = GameObject.FindGameObjectsWithTag("Slot");
    PlayAudio(explosion, .15f);
    foreach (GameObject slot in slots)
    {
      slot.gameObject.tag = "Untagged";
      // slot.GetComponent<Collider>().enabled = false;
      foreach (Transform child in slot.transform)
      {
        
        if (child.name.Equals("slot"))
        {
          foreach (Transform childOfChild in child)
          { 
            foreach(Transform plank in childOfChild){
              ExplodeGO(plank.gameObject, slot.transform.position);
            }
          }
        }
      }
      GameObject.Destroy(slot, 4f);
      ExplodeGO(slot, slot.transform.position);
    }

    yield return new WaitForSeconds(.5f);
    
    SetupBoard();
  }

  //handle pause
  public void Pause(){
    isPaused = !isPaused;
    if(isPaused){
      Time.timeScale = 0;
      mainMenu.SetActive(true);
    }else{
      Time.timeScale = 1;
      mainMenu.SetActive(false);
    }
  }

  /* ######## REUSABLE CODE ######## */
  
  //simple method to player audio
  public void PlayAudio(AudioClip clip, float volume)  
  {
    AudioSource audio = GetComponent<AudioSource> ();
    audio.PlayOneShot(clip, volume);
  }

  //simple method to shuffle through audio clips
  void RandomPlay(AudioClip[] clips, float volume) 
  {
    AudioClip randomSound = clips[Random.Range (0, clips.Length)];
    PlayAudio(randomSound, volume);
  }

  //handle button audiable clicks
  public void ButtonClick(float volume)
  {
    RandomPlay(clicks, volume);
  }

  void ExplodeGO(GameObject obj, Vector3 explosionLocation)
  {
    if(!obj.GetComponent<Rigidbody>()){
      obj.AddComponent<Rigidbody>();
    }

    if(!obj.GetComponent<ConstantForce>()){
      obj.AddComponent<ConstantForce>();
    }
    
    obj.GetComponent<Rigidbody>().useGravity = true;
    obj.GetComponent<Rigidbody>().AddExplosionForce(power, explosionLocation, radius,  Random.Range(-1f, 1f));
    Vector3 objTorque = new Vector3();
    objTorque.x = Random.Range(-500f, 500f);
    objTorque.y = Random.Range(-500f, 500f);
    objTorque.z = Random.Range(-500f, 500f);
    obj.GetComponent<ConstantForce>().torque = objTorque;
  }

  //Fade in algorithm I've made for another game.
  IEnumerator FadeIn()
  {
    GameObject fadeInOut = Instantiate(sceneTransPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
    fadeInOut.GetComponent<Image>().material.SetFloat("_Cutoff", .75f);
    fadeInOut.name = "FadeInOut";
    fadeInOut.transform.SetParent(mainCanvas.transform);
    RectTransform fadeInOutRect = fadeInOut.GetComponent<RectTransform>();
    fadeInOutRect.anchorMin = new Vector2(0, 0);
    fadeInOutRect.anchorMax = new Vector2(1, 1);

    fadeInOutRect.anchoredPosition = Vector3.zero;
    fadeInOutRect.position = Vector3.zero;
    fadeInOutRect.anchoredPosition3D = Vector3.zero;

    fadeInOutRect.offsetMin = new Vector2(-10, -10);
    fadeInOutRect.offsetMax = new Vector2(10, 10);

    fadeInOutRect.localScale = new Vector3(1f, 1f, 1f);

    float tempCutoff = .75f;

    float timeToFade = 0f;

    while (timeToFade > 0)
    {
      timeToFade -= Time.deltaTime;
      yield return null;
    }

    while (tempCutoff > 0)
    {
      tempCutoff -= Time.deltaTime / 1.5f;
      fadeInOut.GetComponent<Image>().material.SetFloat("_Cutoff", tempCutoff);
      yield return null;
    }
    Destroy(fadeInOut);
  }

  //handle player controls
  private void OnEnable()
  {
    controls.Enable();
    controls.Player.Selection.performed += _ => Action();
    controls.Player.Pause.performed += _ => Pause();
  }

  private void OnDisable()
  {
    controls.Disable();
  }
}
