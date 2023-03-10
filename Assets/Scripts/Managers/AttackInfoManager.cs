using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AttackInfoManager : MonoBehaviour
{
    public Values[] values;
    //public TextMeshProUGUI debugText;

    private int _nextInfoId = 0;

    public List<AttackInfo> attackInfos0;
    public List<AttackInfo> attackInfos1;

    public struct Values
    {
        public float outputReward;
        public float outputHitStunDuration;

        //public Values(float outputReward, float outputHitStunDuration)
        //{
        //    this.outputReward = outputReward;
        //    this.outputHitStunDuration = outputHitStunDuration;
        //}
    }

    private void Awake()
    {
        Services.AttackInfoManager = this;
    }

    private void Start()
    {
        //attackInfos0 = new(Services.Characters[0].AttackInfos);
        //attackInfos1 = new(Services.Characters[1].AttackInfos);

        attackInfos0 = new List<AttackInfo>();
        foreach (AttackInfo info in Services.Characters[0].AttackInfos)
        {
            attackInfos0.Add(new AttackInfo
            {
                attackType = info.attackType,
                hitStunDuration = info.hitStunDuration,
                favorReward = info.favorReward,
                knockbackDirection = info.knockbackDirection
            });
        }

        attackInfos1 = new List<AttackInfo>();
        foreach (AttackInfo info in Services.Characters[1].AttackInfos)
        {
            attackInfos1.Add(new AttackInfo
            {
                attackType = info.attackType,
                hitStunDuration = info.hitStunDuration,
                favorReward = info.favorReward,
                knockbackDirection = info.knockbackDirection
            });
        }


        values = new Values[attackInfos0.Count + attackInfos1.Count];

        //for (int i = 0; i < 2; i++)
        //{
        //    int length = Services.Characters[i].AttackInfos.Count;
        //    for (int j = 0; j < length; j++)
        //    {
        //        values[_nextInfoId] = new();
        //        //Services.Characters[i].AttackInfos[j].Initialize(this, _nextInfoId);
        //        Services.Characters[i].AttackInfos[j].Id = _nextInfoId;
        //        values[_nextInfoId].outputReward = Services.Characters[i].AttackInfos[j].favorReward;
        //        values[_nextInfoId].outputHitStunDuration = Services.Characters[i].AttackInfos[j].hitStunDuration;
        //        _nextInfoId++;
        //    }
        //}

        for (int i = 0; i < attackInfos0.Count; i++)
        {
            values[_nextInfoId] = new();
            attackInfos0[i].Id = _nextInfoId;
            values[_nextInfoId].outputReward = attackInfos0[i].favorReward;
            values[_nextInfoId].outputHitStunDuration = attackInfos0[i].hitStunDuration;
            _nextInfoId++;
        }
        for (int i = 0; i < attackInfos1.Count; i++)
        {
            values[_nextInfoId] = new();
            attackInfos1[i].Id = _nextInfoId;
            values[_nextInfoId].outputReward = attackInfos1[i].favorReward;
            values[_nextInfoId].outputHitStunDuration = attackInfos1[i].hitStunDuration;
            _nextInfoId++;
        }

        //if (debugText)
        //{
        //    debugText.text = _nextInfoId.ToString();
        //}
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public IEnumerator DecayValues(AttackInfo attackInfo)
    {
        int id = attackInfo.Id;
        //debugText.text = "id:"+id+ "info:"+attackInfo.hitStunDuration + "values:"+ values[id].outputHitStunDuration;

        float preDecayReward = Mathf.Clamp(values[id].outputReward, 0f, attackInfo.favorReward);
        float preOutputHitStun = Mathf.Clamp(values[id].outputHitStunDuration, 0f, attackInfo.hitStunDuration);

        //if (values[id].outputReward > attackInfo.favorReward)
        //{
        //    values[id].outputReward = attackInfo.favorReward;
        //}
        //else if (values[id].outputHitStunDuration < 0f)
        //{
        //    values[id].outputReward = 0f;
        //}

        //if (values[id].outputHitStunDuration > attackInfo.hitStunDuration)
        //{
        //    values[id].outputHitStunDuration = attackInfo.hitStunDuration;
        //}
        //else if (values[id].outputHitStunDuration < 0f)
        //{
        //    values[id].outputHitStunDuration = 0f;
        //}

        //float preDecayReward = values[id].outputReward;
        //float preOutputHitStun = values[id].outputHitStunDuration;

        //Debug.Log("predecay: " + preDecayReward);

        values[id].outputReward = Mathf.Clamp(preDecayReward - (attackInfo.favorReward * Services.FavorManager.DecayValue), 0f, attackInfo.favorReward);
        values[id].outputHitStunDuration = Mathf.Clamp(preOutputHitStun - (attackInfo.hitStunDuration * Services.FavorManager.DecayValue), 0f, attackInfo.hitStunDuration);

        //values[id].outputReward -= preDecayReward * Services.FavorManager.DecayValue;
        //values[id].outputHitStunDuration -= preOutputHitStun * Services.FavorManager.DecayValue;

        //if (values[id].outputReward < 0f)
        //{
        //    values[id].outputReward = 0f;
        //}
        //if (values[id].outputHitStunDuration < 0f)
        //{
        //    values[id].outputHitStunDuration = 0f;
        //}

        //debugText.text = attackInfo.hitStunDuration.ToString();
        //Debug.Log("postdecay:" + values[id].outputReward);
        //debugText.text = "infostun: "+attackInfo.hitStunDuration + "decay: "+Services.FavorManager.DecayValue+ "outstun: " + values[id].outputHitStunDuration.ToString();
        //float timer = 0f;
        float timerDelta = Time.fixedDeltaTime / Services.FavorManager.DecayResetDuration;
        //while (values[id].outputReward < preDecayReward && timer < Services.FavorManager.DecayResetDuration)
        //{
        //    values[id].outputReward += attackInfo.favorReward * Services.FavorManager.DecayValue * timerDelta;
        //    values[id].outputHitStunDuration += attackInfo.hitStunDuration * Services.FavorManager.DecayValue * timerDelta;
        //    timer += Time.fixedDeltaTime;
        //    yield return new WaitForFixedUpdate();
        //}
        while (values[id].outputReward < preDecayReward)
        {
            values[id].outputReward += attackInfo.favorReward * Services.FavorManager.DecayValue * timerDelta;
            values[id].outputHitStunDuration += attackInfo.hitStunDuration * Services.FavorManager.DecayValue * timerDelta;
            yield return new WaitForFixedUpdate();
        }
        //values[id].outputReward = preDecayReward;
        //values[id].outputHitStunDuration = preOutputHitStun;

        //if (values[id].outputReward > attackInfo.favorReward)
        //{
        //    values[id].outputReward = attackInfo.favorReward;
        //}
        //else if (values[id].outputHitStunDuration < 0f)
        //{
        //    values[id].outputReward = 0f;
        //}

        //if (values[id].outputHitStunDuration > attackInfo.hitStunDuration)
        //{
        //    values[id].outputHitStunDuration = attackInfo.hitStunDuration;
        //}
        //else if (values[id].outputHitStunDuration < 0f)
        //{
        //    values[id].outputHitStunDuration = 0f;
        //}

        values[id].outputReward = Mathf.Clamp(values[id].outputReward, 0f, attackInfo.favorReward);
        values[id].outputHitStunDuration = Mathf.Clamp(values[id].outputHitStunDuration, 0f, attackInfo.hitStunDuration);

        yield break;
    }
}
