using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

/// <summary>
/// This class implements the IVirtualButtonEventHandler interface and
/// contains the logic to start animations depending on what 
/// virtual button has been pressed.
/// </summary> 
public class VirtualButtonController : MonoBehaviour
{
    #region PUBLIC_MEMBERS
    public GameObject[] textComponents = new GameObject[10];
    public Animator[] animButtons = new Animator[10];
    public Animator[] animTexts = new Animator[10];

    public AudioSource[] soundsRadio = new AudioSource[10];
    public AudioSource soundNoise;
    public AudioSource soundAM;
    public AudioSource soundPress;

    public image_event IE;

    #endregion // PUBLIC_MEMBERS

    #region PRIVATE_MEMBERS
    VirtualButtonBehaviour[] virtualButtonBehaviours;
    VirtualButtonBehaviour lastButtonPressed;
    private int quantPlays;
    private int hour = 12;
    private int min = 0;
    private bool stateOnOff = false;
    private bool setHour = true;
    private int radioMax = 10; // has 10 radios stations
    private int radioIndex = 0; // current radio index
    private float radioVolume = 0.5f;
    private int textCompMax = 9;
    private int animMax = 9;
    public String[] radioNames = new String[10];
    public String[] radioFreq = new String[10];

    private int mode = 1;
    private int M_CLOCK = 0;
    private int M_FM = 1;
    private int M_AM = 2;
    #endregion // PRIVATE_MEMBERS

    #region MONOBEHAVIOUR_METHODS
    void Awake()
    {
        //Debug.Log("Awake: " );
        // Init radio names and frequency
        radioNames[0] = "Rádio Aconchego";
        radioFreq[0] = "88.5 Mhz";
        radioNames[1] = "Rádio Jornal de Recife";
        radioFreq[1] = "90.3 Mhz";
        radioNames[2] = "Radio Jovem Pan";
        radioFreq[2] = "95.9 Mhz";
        radioNames[3] = "Rádio Folha de PE";
        radioFreq[3] = "96.7 Mhz";
        radioNames[4] = "Rádio Mix Recife";
        radioFreq[4] = "97.1 Mhz";
        radioNames[5] = "Rádio Dimensão";
        radioFreq[5] = "98.1";
        radioNames[6] = "Rádio Clube";
        radioFreq[6] = "99.1";
        radioNames[7] = "Rádio Excesso";
        radioFreq[7] = "102.1 Mhz";
        radioNames[8] = "Rádio Hits Recife";
        radioFreq[8] = "103.1 Mhz";
        radioNames[9] = "Rádio CBN Recife";
        radioFreq[9] = "105.7";

        // Register with the virtual buttons TrackableBehaviour
        IE = GameObject.Find("Image").GetComponent<image_event>();
        virtualButtonBehaviours = GetComponentsInChildren<VirtualButtonBehaviour>();
        soundNoise.GetComponent<AudioSource>();
        soundAM.GetComponent<AudioSource>();
        for (int i = 0; i < radioMax; i++)
        {
            soundsRadio[i].GetComponent<AudioSource>();
        }
        for (int i = 0; i < textCompMax; i++)
        {
            textComponents[i].GetComponent<TextMesh>();
        }
        for (int i = 0; i < animMax; i++)
        {
            animButtons[i].GetComponent<Animator>();
            animTexts[i].GetComponent<Animator>();
        }

        quantPlays = 0;

        //Debug.Log("Awake: len: " + virtualButtonBehaviours.Length);
        for (int i = 0; i < virtualButtonBehaviours.Length; ++i)
        {
            //Debug.Log("Awake name: " + virtualButtonBehaviours[i].VirtualButtonName);
            virtualButtonBehaviours[i].RegisterOnButtonPressed(OnButtonPressed);
            virtualButtonBehaviours[i].RegisterOnButtonReleased(OnButtonReleased);
        }

        DateTime time = System.DateTime.Now;
        hour = time.Hour;
        min = time.Minute;
        changeTime(0); // init time
        changeVolume(0); // init volume
        // inicia com a tela apagada.
        if (!stateOnOff)
        {
            turnOffScreen();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        VuforiaBehaviour.Instance.World.OnStateUpdated += OnTrackablesUpdated;
    }

    void OnDestroy()
    {
        // Unregister Vuforia life-cycle callbacks:
        if (VuforiaBehaviour.Instance.World != null)
            VuforiaBehaviour.Instance.World.OnStateUpdated -= OnTrackablesUpdated;
    }

    /// 
    /// Called each time the Vuforia state is updated
    /// 
    void OnTrackablesUpdated()
    {
        //changeTime(true);
    }

    void Destroy()
    {
        // Register with the virtual buttons TrackableBehaviour
        virtualButtonBehaviours = GetComponentsInChildren<VirtualButtonBehaviour>();

        for (int i = 0; i < virtualButtonBehaviours.Length; ++i)
        {
            virtualButtonBehaviours[i].UnregisterOnButtonPressed(OnButtonPressed);
            virtualButtonBehaviours[i].UnregisterOnButtonReleased(OnButtonReleased);
        }
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS
    private void turnOffScreen()
    {
        for (int i = 0; i < textCompMax; i++)
        {
            textComponents[i].SetActive(false);
        }
    }
    private void changeVolume(float inc)
    {
        radioVolume += inc;
        if (radioVolume > 1)
        {
            radioVolume = 1;
        }
        if (radioVolume < 0)
        {
            radioVolume = 0;
        }


        for (int i = 0; i < radioMax; i++)
        {
            soundsRadio[i].volume = radioVolume;
        }
        soundNoise.volume = radioVolume;
        soundAM.volume = radioVolume;
        String volText = "> |";
        for (int i = 0; i < 10; i++)
        {
            if (i < ((1 - radioVolume) * 10))
            {
                volText += "  ";
            }
            else
            {
                volText += "--";
            }
        }
        volText += "|";
        textComponents[8].GetComponent<TextMesh>().text = volText;
    }

    private void stopAllSounds()
    {
        for (int i = 0; i < radioMax; i++)
        {
            if (soundsRadio[i].isPlaying)
            {
                soundsRadio[i].Stop();
            }
        }
        if (soundNoise.isPlaying)
        {
            soundNoise.Stop();
        }
        if (soundAM.isPlaying)
        {
            soundAM.Stop();
        }
    }

    private void changeRadio(int inc)
    {
        if (inc > 0)
        {
            radioIndex = (radioIndex + 1) % radioMax;
        } else if (inc < 0)
        {
            radioIndex--;
            if (radioIndex < 0)
            {
                radioIndex = radioMax - 1;
            }
        }

        textComponents[2].GetComponent<TextMesh>().text = radioFreq[radioIndex];
        textComponents[3].GetComponent<TextMesh>().text = radioNames[radioIndex];

        stopAllSounds();
        if (mode == M_FM)
        {
            soundsRadio[radioIndex].Play();
        } else if (mode == M_AM)
        {
            soundAM.Play();
        }
    }

    private void changeTime(int inc)
    {
        if (setHour)
        {
            if (inc > 0)
            {
                hour = (hour + inc) % 24;
            }
            else if (inc < 0)
            {
                hour += inc;
                if (hour < 0)
                {
                    hour = 23;
                }
            }
        }
        else
        {
            if (inc > 0)
            {
                min = (min + inc) % 60;
            }
            else if (inc < 0)
            {
                min += inc;
                if (min < 0)
                {
                    min = 59;
                }
            }
        }

        string hh = string.Format("{0:00}", hour);
        string mm = string.Format("{0:00}", min);
        textComponents[0].GetComponent<TextMesh>().text = hh + ":" + mm;
        textComponents[1].GetComponent<TextMesh>().text = hh + ":" + mm;
    }

    private void configureMode()
    {
        if (mode == M_FM)
        {
            // show
            textComponents[1].SetActive(true); // small clock
            textComponents[2].SetActive(true); // radio frequency
            textComponents[3].SetActive(true); // radio name
            textComponents[5].SetActive(true); // radio mode
            textComponents[6].SetActive(true); // FM mode
            textComponents[8].SetActive(true); // Volume

            // hide
            textComponents[0].SetActive(false); // big clock
            textComponents[4].SetActive(false); // clock mode
            textComponents[7].SetActive(false); // AM mode

            if (stateOnOff)
            {
                changeRadio(0);
            }
        }
        else if (mode == M_AM)
        {
            // show
            textComponents[1].SetActive(true); // small clock
            textComponents[2].SetActive(true); // radio frequency
            textComponents[5].SetActive(true); // radio mode
            textComponents[7].SetActive(true); // AM mode
            textComponents[8].SetActive(true); // Volume

            // hide
            textComponents[0].SetActive(false); // big clock
            textComponents[3].SetActive(false); // radio name
            textComponents[4].SetActive(false); // clock mode
            textComponents[6].SetActive(false); // FM mode

            if (stateOnOff)
            {
                changeRadio(0);
            }
        }
        else if (mode == M_CLOCK)
        {
            // show
            textComponents[0].SetActive(true); // big clock
            textComponents[4].SetActive(true); // clock mode

            // hide
            textComponents[1].SetActive(false); // small clock
            textComponents[2].SetActive(false); // radio frequency
            textComponents[3].SetActive(false); // radio name
            textComponents[5].SetActive(false); // radio mode
            textComponents[6].SetActive(false); // FM mode
            textComponents[7].SetActive(false); // AM mode
            textComponents[8].SetActive(false); // Volume

            stopAllSounds();
        }
    }
    #endregion //PRIVATE_METHODS

    #region PUBLIC_METHODS
    /// <summary>
    /// Called when the virtual button has just been pressed:
    /// </summary>
    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        //Debug.Log("OnButtonPressed: " + vb.VirtualButtonName);

        StopAllCoroutines();


        // Maximum of 2 anims simultaneous
        if (quantPlays >= 2)
        {
            // These 3 have priority, because they are "harder" to click
            if ((vb.VirtualButtonName == "onoff") ||
                (vb.VirtualButtonName == "set") ||
                (vb.VirtualButtonName == "prev"))
            {
                ;
            }
            else
            {
                quantPlays++; // is it necessary?
                return;
            }
        }


        bool clicked = false;
        
        lastButtonPressed = vb;

        // if turned off, ignore the cliks, exept the onoff
        //if (!stateOnOff && vb.VirtualButtonName != "onoff") {
        //    return;
        //}

        if (vb.VirtualButtonName == "onoff")
        {
            turnOffScreen();
            clicked = true;
            animTexts[4].Play("txOnOff");
            animButtons[4].Play("onOff");
            quantPlays++;

            stateOnOff = !stateOnOff;

            // if turned On
            if (stateOnOff)
            {
                configureMode();
            }
            else
            {
                stopAllSounds();
            }
        }
        else if (vb.VirtualButtonName == "set")
        {
            clicked = true;
            animButtons[0].Play("set");
            animTexts[0].Play("txSet");
            quantPlays++;

            // if turned off, does no action
            if (stateOnOff && mode == M_CLOCK)
            {
                setHour = !setHour;
            }
        }
        else if (vb.VirtualButtonName == "prev")
        {
            clicked = true;
            animTexts[7].Play("txPrev");
            animButtons[7].Play("prev");

            if (mode == M_FM && stateOnOff)
            {
                changeRadio(-1);
            }
            else if (mode == M_AM && stateOnOff)
            {
                if (soundAM.isPlaying)
                {
                    stopAllSounds();
                    soundAM.Play();
                }
            }
            quantPlays++;
        }
        else if (vb.VirtualButtonName == "v-")
        {
            clicked = true;
            animTexts[5].Play("txV-");
            animButtons[5].Play("v-");
            quantPlays++;
            // if turned off, does no action
            if (stateOnOff)
            {
                changeVolume(-0.2f);
            }

        }
        else if (vb.VirtualButtonName == "t+")
        {
            clicked = true;
            animTexts[3].Play("txT+");
            animButtons[3].Play("t+");

            quantPlays++;

            if (mode == M_CLOCK && stateOnOff)
            {
                changeTime(1);
            }
        }
        else if (vb.VirtualButtonName == "next")
        {
            clicked = true;
            animTexts[8].Play("txNext");
            animButtons[8].Play("next");

            if (mode == M_FM && stateOnOff)
            {
                changeRadio(1);
            }
            else if (mode == M_AM && stateOnOff)
            {
                if (soundAM.isPlaying)
                {
                    stopAllSounds();
                    soundAM.Play();
                }
            }
            quantPlays++;
        }
        else if (vb.VirtualButtonName == "v+")
        {
            clicked = true;
            animTexts[6].Play("txV+");
            animButtons[6].Play("v+");
            quantPlays++;
            // if turned off, does no action
            if (stateOnOff)
            {
                changeVolume(0.2f);
            }
            
        }
        else if (vb.VirtualButtonName == "mode")
        {
            clicked = true;
            animTexts[1].Play("txMode");
            animButtons[1].Play("mode");
            quantPlays++;
            // if turned off, does no action
            if (stateOnOff)
            {
                mode = (mode + 1) % 3;
                configureMode();
            }
        }
        else if (vb.VirtualButtonName == "t-")
        {
            clicked = true;
            animTexts[2].Play("txT-");
            animButtons[2].Play("t-");
            
            quantPlays++;

            if (mode == M_CLOCK && stateOnOff)
            {
                changeTime(-1);
            }
        }

        if (clicked && !soundPress.isPlaying)
        {
            soundPress.Play();
        }

        //BroadcastMessage("HandleVirtualButtonPressed", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Called when the virtual button has just been released:
    /// </summary>
    public void OnButtonReleased(VirtualButtonBehaviour vb)
    {
        //Debug.Log("OnButtonReleased: " + vb.VirtualButtonName);

        quantPlays--;

        if (vb.VirtualButtonName == "onoff")
        {
            animTexts[4].Play("none");
            animButtons[4].Play("none");
        }
        else if (vb.VirtualButtonName == "next")
        {
            animTexts[8].Play("none");
            animButtons[8].Play("none");
        }
        else if (vb.VirtualButtonName == "prev")
        {
            animTexts[7].Play("none");
            animButtons[7].Play("none");
        }
        else if (vb.VirtualButtonName == "v+")
        {
            animTexts[6].Play("none");
            animButtons[6].Play("none");
        }
        else if (vb.VirtualButtonName == "v-")
        {
            animTexts[5].Play("none");
            animButtons[5].Play("none");
        }
        else if (vb.VirtualButtonName == "mode")
        {
            animTexts[1].Play("none");
            animButtons[1].Play("none");
        }
        else if (vb.VirtualButtonName == "set")
        {
            animButtons[0].Play("none");
            animTexts[0].Play("none");
        }
        else if (vb.VirtualButtonName == "t+")
        {
            animTexts[3].Play("none");
            animButtons[3].Play("none");
        }
        else if (vb.VirtualButtonName == "t-")
        {
            animTexts[2].Play("none");
            animButtons[2].Play("none");
        }
    }
    #endregion //PUBLIC_METHODS
}
