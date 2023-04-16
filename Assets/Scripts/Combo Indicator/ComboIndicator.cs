using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class ComboIndicator : MonoBehaviour
{
    
    [SerializeField] TextMeshProUGUI [] comboTexts;
    [SerializeField] private string TextBeforeComboNum;
    [SerializeField] private string TextAfterComboNum;

    private static int comboCounter = 0;
    private static int maxComboCountDmg = 20;//How much the combo Multiplier can apply to attacks, at most
    private List<PlayerAction> playerActions;

    private static float dmgMultiplier = 0.2f;

    [SerializeField] ShakeRect shaker;

    void Awake() {
        Global.ComboIndicator = this;

        playerActions = new List<PlayerAction>(FindObjectsOfType<PlayerAction>());
        
        SetCombo(0);
    }
    void Start()
    {

    }

    public int GetCombo() {
        return comboCounter;
    }

    public void IncrementCombo(int numChangeBy = 1) {
        SetCombo(comboCounter + numChangeBy);
    }

    public void SetCombo(int numSetTo) {

        comboCounter = numSetTo;

        StringBuilder builder = new StringBuilder();
        builder.Append(TextBeforeComboNum);
        builder.Append(comboCounter);
        builder.Append(TextAfterComboNum);
        if(comboCounter > 1) {
            builder.Append('!', comboCounter - 1);
        }

        foreach(TextMeshProUGUI comboText in comboTexts) {
            
            comboText.text = builder.ToString();

            if(comboCounter >= 2){
                comboText.enabled = true;
                shaker.ShakeIt();
            } else{
                comboText.enabled = false;
            }
        }

        foreach (PlayerAction action in playerActions )
        {
            if(action.IsComboable) {
                action.damage = comboMultiplier(action.baseDamage);// * ((comboCounter + 1) * 1.2f);
                //action.damage = action.baseDamage;

                if(comboCounter == 0){
                    action.damage = action.baseDamage;
                }
            }
        }
    }

    public static float comboMultiplier(float inputAmt) {
        int comboAmt = Mathf.Min(comboCounter, maxComboCountDmg);
        inputAmt = inputAmt + (comboAmt * dmgMultiplier);
        
        return inputAmt;

    }

    IEnumerator ComboJuice() {
        yield return null;
    }

}
