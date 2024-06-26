﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RedBjorn.ProtoTiles.Example
{
    public class PathDrawer : MonoBehaviour
    {
        public LineDrawer Line;
        public SpriteRenderer Tail;
        public TextMeshProUGUI TailText;
        public Color ActiveColor;
        public Color InactiveColor;
        public Gradient ActiveGradient;
        public Gradient InactiveGradient;

        public bool IsEnabled { get; set; }

        public void Init(Color ActiveColor, Color InactiveColor, int longPathNumberOfTurns)
        {
            this.ActiveColor = ActiveColor;
            this.InactiveColor = InactiveColor;
            ActiveGradient = new Gradient();
            ActiveGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(ActiveColor, 0.0f), new GradientColorKey(ActiveColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(ActiveColor.a, 0.0f), new GradientAlphaKey(ActiveColor.a, 1.0f) }
            );
            InactiveGradient = new Gradient();
            InactiveGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(InactiveColor, 0.0f), new GradientColorKey(InactiveColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(InactiveColor.a, 0.0f), new GradientAlphaKey(InactiveColor.a, 1.0f) }
            );
            SetNumberOfTurns(longPathNumberOfTurns);
        }

        public void SetNumberOfTurns(int longPathNumberOfTurns)
        {
            if(longPathNumberOfTurns > 0 )
            {
                TailText.text = longPathNumberOfTurns.ToString();
            }
            else
            {
                TailText.text = string.Empty;
            }
        }

        public void ActiveState()
        {
            SetColor(ActiveColor, ActiveGradient);
        }

        public void InactiveState()
        {
            SetColor(InactiveColor, InactiveGradient);
        }

        void SetColor(Color color, Gradient gradient)
        {
            Line.Line.colorGradient = gradient;
            Tail.color = color;
        }

        public void Show(List<Vector3> points, MapEntity map)
        {
            var offset = map.Settings.VectorCreateOrthogonal(0.01f);
            Line.Line.transform.localPosition = offset;
            Tail.transform.localPosition = offset;
            Tail.transform.rotation = map.Settings.RotationPlane();

            if (points == null || points.Count == 0)
            {
                Hide();
            }
            else
            {
                var tailPos = points[points.Count - 1];
                Tail.transform.localPosition = map.Settings.Projection(tailPos, 0.01f);
                Tail.gameObject.SetActive(true);
                if (points.Count > 1)
                {
                    Line.Line.transform.localRotation = map.Settings.RotationPlane();
                    points[points.Count - 1] = (points[points.Count - 1] + points[points.Count - 2]) / 2f;
                    var pointsXY = new Vector3[points.Count];
                    for (int i = 0; i < pointsXY.Length; i++)
                    {
                        pointsXY[i] = map.Settings.ProjectionXY(points[i]);
                    }

                    Line.Show(pointsXY);
                }
            }
        }

        public void Hide()
        {
            Line.Hide();
            Tail.gameObject.SetActive(false);
        }
    }
}