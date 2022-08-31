using CreativeVeinStudio.Simple_Pool_Manager;
using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
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
    [InspectorShow("总督")]
    public Color G1;
    [InspectorShow("提督")]

    public Color G2;
    [InspectorShow("舰长")]

    public Color G3;
    [InspectorShow("默认")]
    public Color Normal;
    RectTransform self_rt;
    private IPoolActions _spManager;
    // Start is called before the first frame update
    Transform p_parent;
    private void Awake()
    {
        _spManager = FindObjectOfType<SpManager>();
        self_rt = GetComponent<RectTransform>();
        p_parent = _spManager.GetPoolItemParentTransform("Danmu");
    }
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
    {if(gameObject.activeSelf)
        if (self_rt.anchoredPosition.y >= -200f&&transform.parent?.name!="Danmu") {
            gameObject.transform.SetParent(p_parent);
            _spManager.DisablePoolObject(gameObject);
        }
    }
}
