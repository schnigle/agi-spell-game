using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TalkScript : MonoBehaviour
{
    public GameObject playerObj;
    public GameObject textObj;
    public HelperAI helperai;
    private Text textbox;

    List<string> conversion1 = new List<string>();
    List<string> conversion2 = new List<string>();
    List<string> conversion3 = new List<string>();

    void Start() //7 words per bubble or 25 ASCII
    {
        textbox = textObj.GetComponent<Text>();
        conversion1.Add("Yo, do the following to cast spells:");
        conversion1.Add("Hold down the button under the controller.");
        conversion1.Add("Move it to make a shape.");
        conversion1.Add("Then release the button and fire at a target.");

        conversion2.Add("Good work.");
        conversion2.Add("Use the spellbook to see spells.");

        conversion3.Add("This is the end of the tutorial.");
        conversion3.Add("It's time to attack wizard-town!!");
    }
    int counter = 0;
    int selectedChat = -1;
    float timer = 0;
    float chatTime = 4.0f;

    void proceedConvo(List<string> convoList)
    {
        if (timer > chatTime)
        {
            timer -= chatTime;
            counter = (counter + 1) % convoList.Count;
        }
        textbox.text = convoList.ElementAt(counter);
    }

    void chat1()
    {
        proceedConvo(conversion1);

    }

    void chat2()
    {
        proceedConvo(conversion2);
    }

    void chat3()
    {
        proceedConvo(conversion3);
    }

    void Update()
    {
        timer += Time.deltaTime;

        GameObject nowFlag = helperai.getFlagClosestToPlayer();
        if (nowFlag.name.ToString().Contains("1"))
        {
            if(selectedChat != 1)
            {
                counter = 0;
                selectedChat = 1;
            }
            chat1();
        }
        else if(nowFlag.name.ToString().Contains("2"))
        {
            if (selectedChat != 2)
            {
                counter = 0;
                selectedChat = 2;
            }
            chat2();
        }else if (nowFlag.name.ToString().Contains("3"))
        {
            if (selectedChat != 3)
            {
                counter = 0;
                selectedChat = 3;
            }
            chat3();
        }

        Vector3 v = playerObj.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt(playerObj.transform.position - v);
        transform.Rotate(0, 180, 0);

        float playDist = Vector3.Distance(playerObj.transform.position, transform.position);
        if(playDist > 5)
        {
            textObj.gameObject.SetActive(false);
        }
        else
        {
            textObj.gameObject.SetActive(true);
        }
    }


}
