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
public class MoveWithFinger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform card;
    [SerializeField] private Camera camera;
    //card到达边缘的最大z轴旋转
    [SerializeField] private float maxZRotation = 10;
    //选择被确定的临界比例
    [SerializeField]private float threhold=0.5f;
    private Vector2 initPos;
    private RectTransform panel;
    private Vector2 pressMouseAnchoredPos;
    private bool isBacking = false;
    //private Vector2 screentPixelSize;
    //[Tooltip("回弹速度")]
    //private float backSpeed = 10;
    // Use this for initialization
    void Start()
    {
        initPos = card.anchoredPosition;
        // RectTransform canvas = card.parent.parent as RectTransform;
        // screentPixelSize = Utility.GetRectTransformPixelSize(canvas, camera);
        panel = card.parent as RectTransform;
        Debug.Log(card.anchoredPosition);
        Debug.Log(panel.anchoredPosition);
    }

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
        card.eulerAngles = Vector3.forward * getOritention(pos, panel.rect);
        Debug.Log(getRegion(card.anchoredPosition, initPos));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isBacking)
            return;
        //float time = (card.position - initPos).magnitude/backSpeed;
        card.DOAnchorPos(initPos, 0.25f)
            .OnStart(() => isBacking = true)
            .OnComplete(() => isBacking = false)
            .SetEase(Ease.OutBack);
        card.DORotate(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
        Debug.Log("End Drag");
    }

    //根据panel大小以及当前鼠标位置计算出card的倾斜
    float getOritention(Vector2 mouseAnchorPos, Rect panel)
    {
        //Debug.Log(mouseAnchorPos+"  "+panel.center);
        float xDelta = panel.center.x - mouseAnchorPos.x;
        return xDelta / panel.width * 2 * maxZRotation;
    }

    //根据坐标判断当前位置在那个象限
    //1-右侧 2-上方 3-左侧 4-下方
    int getRegion(Vector2 curPos, Vector2 initCardPos)
    {
        Vector2 delta = curPos - initCardPos;
        //Debug.Log(curPos+" "+initCardPos+"  "+delta);
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
    float getProgress(Vector2 curPos,Vector2 initCardPos,Vector2 size){
        int region=getRegion(curPos,initCardPos);
        if(region==1||region==3)
        {
            return Mathf.Abs((curPos-initCardPos).x)/size.x*2;
        }else if(region==2||region==4){
            return Mathf.Abs((curPos-initCardPos).y)/size.y*2;
        }
        return 0;
    }
}
