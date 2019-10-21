using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spellbook : MonoBehaviour
{
    [SerializeField]
    GameObject leftPage;
    [SerializeField]
    GameObject rightPage;
    GameObject flipPage;
    // float flipPageScale = -1;
    float flipPageProgress = 0;
    Vector3 flipPageOriginalScale;
    // Start is called before the first frame update
    void Start()
    {
        // rightPage.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector3(-1, 1);
        flipPage = Instantiate(leftPage);
        flipPage.transform.SetParent(transform);
        flipPage.transform.localPosition = leftPage.transform.localPosition;
        flipPage.transform.localRotation = leftPage.transform.localRotation;
        flipPage.transform.localScale = leftPage.transform.localScale;
        flipPageOriginalScale = flipPage.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        flipPageProgress += Time.deltaTime;
        if (flipPageProgress > 1)
        {
            flipPageProgress = 0;
        }
        var flipPageWidth = flipPageProgress * 2 - 1;
        var flipPageHeight = Mathf.Sin(flipPageProgress * Mathf.PI) + 1;
        flipPage.transform.localScale = new Vector3(
            flipPageOriginalScale.x * flipPageWidth * -1,
            flipPage.transform.localScale.y,
            flipPageOriginalScale.z * flipPageHeight
        );
    }
}
