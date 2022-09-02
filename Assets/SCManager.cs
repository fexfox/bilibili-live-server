using CreativeVeinStudio.Simple_Pool_Manager;
using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SCManager : MonoBehaviour
{
    public TMP_Text Title;
    public TMP_Text Content;
    public Slider slider;
    private IPoolActions _spManager;
    // Start is called before the first frame update
    Transform p_parent;
    private void Awake()
    {
        _spManager = FindObjectOfType<SpManager>();
 
    }
    // Start is called before the first frame update
    void Start()
    {
        p_parent = _spManager.GetPoolItemParentTransform("SC");
    }
    bool startCountDown = false;
    // Update is called once per frame
    void Update()
    {
        if (startCountDown)
            if (slider.value > 0)
            {
                slider.value -= Time.deltaTime;
            }
            else
            {
                startCountDown = false;
                slider.value = 0;
                gameObject.transform.SetParent(p_parent);
                _spManager.DisablePoolObject(gameObject);
            }
    }
    public void setSC(string uname, string price, int time, string content)
    {
        startCountDown = false;
        Title.text = $"{uname}¡î£¤{price}¡î{time}s";
        Content.text = content;
        slider.maxValue = time;
        slider.value = time;
        slider.minValue = 0;
        startCountDown = true;
    }
}
