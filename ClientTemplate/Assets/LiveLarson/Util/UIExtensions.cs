using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LiveLarson.Util
{
    public static class UIExtensions
    {
        [Flags]
        public enum GrayscaleApplyingOption
        {
            None = 0,
            IncludeInactive = 1 << 0,
            UseKeptTargets = 1 << 1
        }

        [Flags]
        public enum GrayscaleUnapplyingOption
        {
            None = 0,
            KeepTargets = 1 << 0
        }

        private static readonly List<Vector3> BezierPoints = new();

        public static Vector3 BezierCurvePoint(this IEnumerable<Vector3> list, float rate)
        {
            BezierPoints.Clear();
            BezierPoints.AddRange(list);
            var points = BezierPoints;
            var count = points.Count - 1;

            while (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var result = Vector3.Lerp(points[i], points[i + 1], rate);
                    points[i] = result;
                }

                count--;
            }

            return points[0];
        }

        public static Vector2 GetRandomPointInRect(RectTransform rectTransform, bool useLocal = false)
        {
            var rect = rectTransform.rect;
            var position = useLocal ? rectTransform.localPosition : rectTransform.position;
            var randomX = Random.Range(position.x - rect.width / 2, position.x + rect.width / 2);
            var randomY = Random.Range(position.y - rect.height / 2, position.y + rect.height / 2);
            var result = new Vector2(randomX, randomY);
            return result;
        }

        public static bool GetMousePositionOnCanvas(Canvas canvas, out Vector2 localPoint)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(),
                    new Vector2(Input.mousePosition.x, Input.mousePosition.y), canvas.worldCamera,
                    out var mousePos))
            {
                localPoint = mousePos;
                return true;
            }

            localPoint = Vector2.zero;
            return false;
        }
    }
}