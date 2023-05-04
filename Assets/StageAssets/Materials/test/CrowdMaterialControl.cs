using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdMaterialControl : MonoBehaviour
{
    public List<Texture> texList = new List<Texture>();
    private int texListLength;
    // Start is called before the first frame update
    void Start()
    {
        texListLength = texList.Count;
        Randomization();
    }

    // Update is called once per frame
    void Randomization() //randomize sprite and color
    {
        //crowd sprite
        Renderer rend = gameObject.GetComponent<Renderer>();
        int texIndex = Random.Range(0, texListLength - 1);
        Texture crowdTexture = texList[texIndex];
        rend.material.SetTexture("_BaseMap", crowdTexture);

        
        float H, S, V;
        Color col = rend.material.color;
        Color.RGBToHSV(col, out H, out S, out V);
        float tempcol;
        if (Random.Range(0.0f, 1.0f) <= 0.3f)
            tempcol = Random.Range(0f, 0.14f);
        else tempcol = Random.Range(0.0f, 1.0f);
        //else
        //{
        //    tempcol = Random.Range(0.0f, 1.0f);
        //    if (tempcol > 0.18f && tempcol < 0.45f)
        //        tempcol += Random.Range(-0.1f, 0.35f); // extra should go to red?
        //}
        rend.material.color = Color.HSVToRGB(tempcol, S, V);
    }
}
