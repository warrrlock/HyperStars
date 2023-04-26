using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class P2Indicator : MonoBehaviour
{
    
    [SerializeField] TextMeshProUGUI [] comboTexts;
    [SerializeField] private string TextBeforeComboNum;
    [SerializeField] private string TextAfterComboNum;
    [SerializeField] ShakeRect shaker;

    private static int comboCounter = 0;
    private static int maxComboCountDmg = 20;//How much the combo Multiplier can apply to attacks, at most
    //private List<PlayerAction> playerActions;

    private static float dmgMultiplier = 0.2f;
    private bool _p1Comboing;

    public TextMeshProUGUI text;
    private Color newColor;
    private Color normalColor;
    public float fadeSpeed = 0.1f;
    private bool fadeTime = false;
    private bool fadeBool = false;
    private float fadeTimer = 1f;
    public float secsToWait = 1f; //number of seconds you want to wait before fading
    

    void Awake() {
        //Global.ComboIndicator = this;

        //playerActions = new List<PlayerAction>(FindObjectsOfType<PlayerAction>());
        
        SetCombo(0);

        //var trans = 0.01f;
        //var col = gameObject.GetComponent<Renderer> ().material.color;
        newColor.a = 0;
    }


    void Start()
    {
        Subscribe();

        normalColor = text.color;
    }

    private void OnDestroy() {
        UnSubscribe();
    }


    void FixedUpdate(){
        //var trans = 0.01f;
        //var col = gameObject.GetComponent<Renderer> ().material.color;
        //text.a -= trans;
        if (fadeTime == true){
            if (fadeTimer < secsToWait){
                fadeTimer += Time.deltaTime; //adds the time between two frames to the timer
            } else {
                FadeOut();
            }
        }
        
    }

    private void FadeOut()
    {
        text.color = Color.Lerp(text.color, newColor, fadeSpeed * Time.deltaTime);
    }

    private void Subscribe()
        {
            //_fighter is a reference to fighter object
            Services.Fighters[0].Events.onAttackHit += IncrementPlayer1Combo; //note that block is a separate event
            Services.Fighters[1].Events.onAttackHit += IncrementPlayer2Combo; //note that block is a separate event

            Services.Fighters[0].Events.onEndHitstun += ResetCombo;
        }
        
        private void UnSubscribe() //remember to unsubscribe when object is destroyed (OnDestroy)
        {
            //_fighter is a reference to fighter object
            Services.Fighters[0].Events.onAttackHit -= IncrementPlayer1Combo; //note that block is a separate event
            Services.Fighters[1].Events.onAttackHit -= IncrementPlayer2Combo; //note that block is a separate event

            Services.Fighters[0].Events.onEndHitstun -= ResetCombo;
        }


    public int GetCombo() {
        return comboCounter;
    }

    private void IncrementPlayer1Combo(Dictionary<string, object> msg){
        // Debug.Log("subscribed combo, incrementing for player 1");
        if (!_p1Comboing)
        {
            _p1Comboing = true;
            SetCombo(0);
            fadeTime = false;
            fadeTimer = 0;
        }
        //IncrementCombo();
    }
    
    private void IncrementPlayer2Combo(Dictionary<string, object> msg){
        // Debug.Log("subscribed combo, incrementing for player 2");
        if (_p1Comboing)
        {
            _p1Comboing = false;
            SetCombo(0);
            fadeTime = false;
            fadeTimer = 0;
            
        }
        IncrementCombo();

        fadeTime = true;
        fadeTimer = 0;

        text.color = normalColor;
        //var col = gameObject.GetComponent<Renderer> ().material.color;
        //col.a = 1;
    }

    private void ResetCombo(){
        // Debug.Log("subscribed combo, incrementing for player 2");
        //IncrementCombo();

        SetCombo(0);
    }


    public void IncrementCombo(int numChangeBy = 1) { //Global.ComboIndicator.IncrementCombo();
        SetCombo(comboCounter + numChangeBy);
    }

    public void SetCombo(int numSetTo) { //Global.ComboIndicator.SetCombo(0);

        comboCounter = numSetTo;

        StringBuilder builder = new StringBuilder();
        builder.Append(TextBeforeComboNum);
        builder.Append(comboCounter);
        builder.Append(TextAfterComboNum);
        if(comboCounter > 1) {
            //builder.Append('!', comboCounter - 1);
        }

        foreach(TextMeshProUGUI comboText in comboTexts) {
            
            comboText.text = builder.ToString();

            if(comboCounter >= 1){
                comboText.enabled = true;
                shaker.ShakeIt();
            } else{
                comboText.enabled = false;
            }
        }



        /*foreach (PlayerAction action in playerActions )
        {
            if(action.IsComboable) {
                action.damage = comboMultiplier(action.baseDamage);// * ((comboCounter + 1) * 1.2f);
                //action.damage = action.baseDamage;

                if(comboCounter == 0){
                    action.damage = action.baseDamage;
                }
            }
        }*/
    }

    /*public static float comboMultiplier(float inputAmt) {
        int comboAmt = Mathf.Min(comboCounter, maxComboCountDmg);
        inputAmt = inputAmt + (comboAmt * dmgMultiplier);
        
        return inputAmt;

    }*/

    IEnumerator ComboJuice() {
        yield return null;
    }

}

