using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//1.拖动
//2.根据位置进行旋转
//TODO:3.根据位置显示相应的标签，并作相应淡出
//TODO:4.设置card处理完成的临界条件
//TODO:5.card到达临界条件的事件：动画，刷新card
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform card;
    [Tooltip("card到达边缘的最大z轴旋转")]
    [SerializeField] private float maxZRotation = 10;
    [Tooltip("被确定时候的临界值，四个数字，分别代表左,上,右,下")]
    [SerializeField] private float[] threhold = new float[4]{0.8f,0.5f,0.8f,0.5f};
    [SerializeField] private GameObject[] labels;
    private Vector2 initPos;
    private RectTransform panel;
    private Vector2 pressMouseAnchoredPos;
    private Camera camera;
    private bool isBacking = false;

    void Start()
    {
        initPos = card.anchoredPosition;
        panel = card.parent as RectTransform;
        camera=Camera.main;
    }

    /// <summary>
    /// 开始拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isBacking)
            return;
        pressMouseAnchoredPos = Utility.ScreenPointToAnchorPos(panel, camera, Input.mousePosition);
        Debug.Log("Begin Drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isBacking)
            return;
        Vector2 pos = Utility.ScreenPointToAnchorPos(panel, camera, Input.mousePosition);
        card.anchoredPosition = initPos + pos - pressMouseAnchoredPos;
        card.eulerAngles = Vector3.forward * getOritention(card.anchoredPosition, initPos, panel.rect);

        int region = getRegion(card.anchoredPosition, initPos);
        float progress = getProgress(card.anchoredPosition, initPos, panel.rect.size);

        for (int i = 0; i < labels.Length; i++)
        {
            if (i == region - 1)
                setLabelAlpha(labels[i], Mathf.Clamp(progress / threhold[region-1], 0, 1));
            else
            {
                setLabelAlpha(labels[i], 0);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isBacking)
            return;

        int region = getRegion(card.anchoredPosition, initPos);
        float progress = getProgress(card.anchoredPosition, initPos, panel.rect.size);


        if (progress < threhold[region-1])
        {
            labels[region - 1].GetComponent<Text>().DOFade(0, 0.25f);

            card.DOAnchorPos(initPos, 0.25f)
                .OnStart(() => isBacking = true)
                .OnComplete(() => isBacking = false)
                .SetEase(Ease.OutBack);
            card.DORotate(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
        }
        else
        {
            Vector2 endPos = initPos;
            switch (region)
            {
                case 1: endPos = initPos + Vector2.right * panel.rect.width; break;
                case 2: endPos = initPos + Vector2.up * panel.rect.height; break;
                case 3: endPos = initPos + Vector2.left * panel.rect.width; break;
                case 4: endPos = initPos + Vector2.down * panel.rect.height; break;
            }
            card.DOAnchorPos(endPos, 0.5f)
                .OnStart(() => isBacking = true)
                .OnComplete(() => isBacking = false)
                .SetEase(Ease.OutBack);
        }
        Debug.Log("End Drag");
    }

    //根据panel大小以及当前鼠标位置计算出card的倾斜
    float getOritention(Vector2 curPos, Vector2 initPos, Rect panel)
    {
        float xDelta = initPos.x - curPos.x;
        return xDelta / panel.width * 2 * maxZRotation;
    }

    //根据坐标判断当前位置在那个象限
    //1-右侧 2-上方 3-左侧 4-下方
    int getRegion(Vector2 curPos, Vector2 initCardPos)
    {
        Vector2 delta = curPos - initCardPos;

        if (delta.x == 0) return delta.y > 0 ? 2 : delta.y == 0 ? 0 : 4;
        else
        {
            float tan = Mathf.Abs(delta.y / delta.x);
            if (tan <= 1)
            {
                return delta.x > 0 ? 1 : 3;
            }
            else
            {
                return delta.y > 0 ? 2 : 4;
            }
        }
    }

    //获取label的比例值
    float getProgress(Vector2 curPos, Vector2 initCardPos, Vector2 size)
    {
        int region = getRegion(curPos, initCardPos);
        if (region == 1 || region == 3)
        {
            return Mathf.Abs((curPos - initCardPos).x) / size.x * 2;
        }
        else if (region == 2 || region == 4)
        {
            return Mathf.Abs((curPos - initCardPos).y) / size.y * 2;
        }
        return 0;
    }

    //设置标签的alpha值
    void setLabelAlpha(GameObject label, float alpha)
    {
        Text text = label.GetComponent<Text>();
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }
}
