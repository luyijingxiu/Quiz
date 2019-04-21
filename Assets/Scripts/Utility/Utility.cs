using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility  {

    public static Vector2 GetRectTransformPixelSize(RectTransform transform, Camera camera)
    {
        Vector3[] corner = new Vector3[4];
        transform.GetWorldCorners(corner);
        return RectTransformUtility.WorldToScreenPoint(camera, corner[2]) - RectTransformUtility.WorldToScreenPoint(camera, corner[0]);
    }

    public static Vector2 ScreenPointToAnchorPos(RectTransform transform,Camera camera,Vector2 screenPoint)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform,screenPoint,camera,out pos);
        return pos;
    }
}
