using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXCleanUp : MonoBehaviour
{
    private VisualEffect vfx;
    private float lifeTime;
    [HideInInspector] public Fighter receiver;
    
    void Start()
    {
        vfx = GetComponent<VisualEffect>();
        lifeTime = vfx.GetFloat("Life");
        StartCoroutine(Kill());
    }

    IEnumerator Kill()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
    
    void Update() {
        vfx.SetBool("FaceLeft", receiver.FacingDirection == Fighter.Direction.Left);
    }
}
