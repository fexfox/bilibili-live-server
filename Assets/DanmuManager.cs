using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DanmuManager : MonoBehaviour
{
    public Image avatar;
    public GameObject G_avatar;
    public RectTransform T_content;
    public TMP_Text username;
    public TMP_Text content;
    public Image Guard;
    [InspectorShow("�ܶ�")]
    public Color G1;
    [InspectorShow("�ᶽ")]

    public Color G2;
    [InspectorShow("����")]

    public Color G3;
    [InspectorShow("Ĭ��")]
    public Color Normal;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void HideAvatar()
    {
        G_avatar.SetActive(false);
        T_content.rect.Set(T_content.rect.x, T_content.rect.y, 530, T_content.rect.height);
    }

    public void setGuardLevel(int guardLevel)
    {
        switch (guardLevel)
        {
            case 1:
                Guard.color = G1;
                break;
            case 2:
                Guard.color = G2;
                break;
            case 3:
                Guard.color = G3;
                break;
            default:
                Guard.color = Normal;
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
    public void DestoryMe()
    {
        Destroy(gameObject);
    }
}
