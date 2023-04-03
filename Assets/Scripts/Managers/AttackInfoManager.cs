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
        //foreach (AttackInfo info in Services.Characters[0].AttackInfos)
        //{
        //    attackInfos0.Add(new AttackInfo
        //    {
        //        attackType = info.attackType,
        //        hitStunDuration = info.hitStunDuration,
        //        favorReward = info.favorReward,
        //        knockbackDirection = info.knockbackDirection
        //    });
        //}

        attackInfos1 = new List<AttackInfo>();
        //foreach (AttackInfo info in Services.Characters[1].AttackInfos)
        //{
        //    attackInfos1.Add(new AttackInfo
        //    {
        //        attackType = info.attackType,
        //        hitStunDuration = info.hitStunDuration,
        //        favorReward = info.favorReward,
        //        knockbackDirection = info.knockbackDirection
        //    });
        //}

        values = new Values[Services.Characters[0].AttackInfos.Count + Services.Characters[1].AttackInfos.Count];

        for (int i = 0; i < Services.Characters[0].AttackInfos.Count; i++)
        {
            int idSO = Services.Characters[0].AttackInfos[i].idSO;
            AttackInfo newInfo = new AttackInfo
            {
                idSO = idSO,
                idManager = _nextInfoId,
                hitStunDuration = Services.Characters[0].AttackInfos[i].hitStunDuration,
                favorReward = Services.Characters[0].AttackInfos[i].favorReward
            };
            //attackInfos0.Add(new AttackInfo
            //    {
            //        idSO = idSO,
            //        idManager = _nextInfoId,
            //        hitStunDuration = Services.Characters[0].AttackInfos[i].hitStunDuration,
            //        favorReward = Services.Characters[0].AttackInfos[i].favorReward
            //    });
            attackInfos0.Add(newInfo);
            values[_nextInfoId] = new();
            values[_nextInfoId].outputReward = newInfo.favorReward;
            values[_nextInfoId].outputHitStunDuration = newInfo.hitStunDuration;
            _nextInfoId++;
        }

        for (int i = 0; i < Services.Characters[1].AttackInfos.Count; i++)
        {
            int idSO = Services.Characters[1].AttackInfos[i].idSO;
            AttackInfo newInfo = new AttackInfo
            {
                idSO = idSO,
                idManager = _nextInfoId,
                hitStunDuration = Services.Characters[1].AttackInfos[i].hitStunDuration,
                favorReward = Services.Characters[1].AttackInfos[i].favorReward
            };
            //attackInfos1.Add(new AttackInfo
            //{
            //    idSO = idSO,
            //    idManager = _nextInfoId,
            //    hitStunDuration = Services.Characters[1].AttackInfos[i].hitStunDuration,
            //    favorReward = Services.Characters[1].AttackInfos[i].favorReward
            //});
            attackInfos1.Add(newInfo);
            values[_nextInfoId] = new();
            values[_nextInfoId].outputReward = newInfo.favorReward;
            values[_nextInfoId].outputHitStunDuration = newInfo.hitStunDuration;
            _nextInfoId++;
        }


        //values = new Values[attackInfos0.Count + attackInfos1.Count];

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

        //for (int i = 0; i < attackInfos0.Count; i++)
        //{
        //    values[_nextInfoId] = new();
        //    attackInfos0[i].Id = _nextInfoId;
        //    values[_nextInfoId].outputReward = attackInfos0[i].favorReward;
        //    values[_nextInfoId].outputHitStunDuration = attackInfos0[i].hitStunDuration;
        //    _nextInfoId++;
        //}
        //for (int i = 0; i < attackInfos1.Count; i++)
        //{
        //    values[_nextInfoId] = new();
        //    attackInfos1[i].Id = _nextInfoId;
        //    values[_nextInfoId].outputReward = attackInfos1[i].favorReward;
        //    values[_nextInfoId].outputHitStunDuration = attackInfos1[i].hitStunDuration;
        //    _nextInfoId++;
        //}

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
        int id = attackInfo.idManager;
        //debugText.text = "id:"+id+ "info:"+attackInfo.hitStunDuration + "values:"+ values[id].outputHitStunDuration;

        float preDecayReward = Mathf.Clamp(values[id].outputReward, 0f, attackInfo.favorReward);
        float preOutputHitStun = Mathf.Clamp(values[id].outputHitStunDuration, 0f, attackInfo.hitStunDuration);

        values[id].outputReward = Mathf.Clamp(preDecayReward - (attackInfo.favorReward * Services.FavorManager.DecayValue), 0f, attackInfo.favorReward);
        values[id].outputHitStunDuration = Mathf.Clamp(preOutputHitStun - (attackInfo.hitStunDuration * Services.FavorManager.DecayValue), 0f, attackInfo.hitStunDuration);

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

        values[id].outputReward = Mathf.Clamp(values[id].outputReward, 0f, attackInfo.favorReward);
        values[id].outputHitStunDuration = Mathf.Clamp(values[id].outputHitStunDuration, 0f, attackInfo.hitStunDuration);

        yield break;
    }
}
