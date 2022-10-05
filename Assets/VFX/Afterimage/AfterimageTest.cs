using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AfterimageTest : MonoBehaviour
{
    // [SerializeField] private ParticleSystem _particleSystem;
    // private Renderer particleRenderer;
    
    [SerializeField] private VisualEffect _visualEffect;
    
    private SpriteRenderer _spriteRenderer;

    private float delayTimer;
    
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // particleRenderer = _particleSystem.GetComponent<Renderer>();
    }
    
    void LateUpdate()
    {
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
        }
        else
        {
            _visualEffect.SetTexture("MainTex2D", _spriteRenderer.sprite.texture);
            // particleRenderer.material.mainTexture = _spriteRenderer.sprite.texture;
            delayTimer = 1;
        }
    }
}
