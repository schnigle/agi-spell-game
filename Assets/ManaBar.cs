using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    private RawImage barImage;

    private RectTransform maskRectTransform;
    float barMaskWidth = 0;

    public GameObject playerObject;
    Mana mana;

    // Start is called before the first frame update
    void Start()
    {
        maskRectTransform = transform.Find("Mask").GetComponent<RectTransform>();
        barImage = maskRectTransform.Find("Bar").GetComponent<RawImage>();
        barMaskWidth = maskRectTransform.sizeDelta.x;
        if(playerObject != null)
        {
            var temp = playerObject.GetComponent<PlayerScript>();
            if(temp != null)
                mana = temp.GetPlayerData().mana;
        }
           

    }

    // Update is called once per frame
    void Update()
    {
        if(mana == null)
        {
            mana = playerObject.GetComponent<PlayerScript>().GetPlayerData().mana;
            return;
        }

        mana.Update();
        //barImage.fillAmount = mana.GetManaNormalized();
        Rect uvRect = barImage.uvRect;
        uvRect.x -= 0.5f * Time.deltaTime;
        barImage.uvRect = uvRect;

        Vector2 barMaskSizeDelta = maskRectTransform.sizeDelta;
        barMaskSizeDelta.x = mana.GetManaNormalized() * barMaskWidth;
        maskRectTransform.sizeDelta = barMaskSizeDelta;

        float rand = Random.Range(0, 1f);
        if (rand > 0.98f)
        {
            mana.TrySpendMana(30);
        }
            

    }
}

