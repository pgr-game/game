using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;

public class DamageAnimation : MonoBehaviour
{
    private float velcoity=2.0f;
    public Vector3 angle;
    private float coutner = 0;
    GameObject damageObject;
    TextMeshProUGUI damageText;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        damageObject = this.transform.Find("Damage").gameObject;
        damageText = damageObject.GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        this.transform.position += new Vector3(0+ (angle.x*Time.deltaTime), (Time.deltaTime * this.velcoity)- (coutner/100)+ (angle.y * Time.deltaTime), 0);
        this.coutner += Time.deltaTime*1.5f;

        float alpha = Mathf.Lerp(1f, 0f, this.coutner / 2);
        damageText.color= new Color(damageText.color.r, damageText.color.g, damageText.color.b, alpha);
        if (alpha == 0)
        {
            Destroy(gameObject);
        }
        //this.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        
        
        //textField.CrossFadeAlpha(0, 2f, true);
    }
}
