using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInfoManager : MonoBehaviour
{
    public Values[] values;

    private int _nextInfoId = 0;

    public struct Values
    {
        public float outputReward;
        public float outputHitStunDuration;

        public Values(float outputReward, float outputHitStunDuration)
        {
            this.outputReward = outputReward;
            this.outputHitStunDuration = outputHitStunDuration;
        }
    }

    private void Awake()
    {
        Services.AttackInfoManager = this;
    }

    private void Start()
    {
        values = new Values[Services.Characters[0].AttackInfos.Count + Services.Characters[1].AttackInfos.Count];

        for (int i = 0; i < 2; i++)
        {
            int length = Services.Characters[i].AttackInfos.Count;
            for (int j = 0; j < length; j++)
            {
                Services.Characters[i].AttackInfos[j].Initialize(this, _nextInfoId);
                _nextInfoId++;
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public IEnumerator DecayValues(AttackInfo attackInfo)
    {
        int id = attackInfo.Id;
        float preDecayReward = values[id].outputReward;
        values[id].outputReward = Mathf.Clamp(values[id].outputReward - (attackInfo.favorReward * Services.FavorManager.DecayValue), 0f, attackInfo.favorReward);
        values[id].outputHitStunDuration = Mathf.Clamp(values[id].outputHitStunDuration - (attackInfo.hitStunDuration * Services.FavorManager.DecayValue), 0f, attackInfo.hitStunDuration);
        float timer = 0f;
        float timerDelta = Time.fixedDeltaTime / Services.FavorManager.DecayResetDuration;
        while (values[id].outputReward < preDecayReward && timer < Services.FavorManager.DecayResetDuration)
        {
            values[id].outputReward += attackInfo.favorReward * Services.FavorManager.DecayValue * timerDelta;
            values[id].outputHitStunDuration += attackInfo.hitStunDuration * Services.FavorManager.DecayValue * timerDelta;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
}
