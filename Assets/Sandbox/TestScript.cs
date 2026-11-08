using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Scene = Unity.VectorGraphics.Scene;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        var shape = new Shape
        {
            Contours = new[]
            {
                new BezierContour
                {
                    Segments = new[]
                    {
                        new BezierPathSegment { P0 = new Vector2(0,0),   P1 = new Vector2(50,0),  P2 = new Vector2(100,0)  },
                        new BezierPathSegment { P0 = new Vector2(100,0), P1 = new Vector2(100,50),P2 = new Vector2(100,100)},
                        new BezierPathSegment { P0 = new Vector2(100,100) }
                    },
                    Closed = true
                }
            },
            Fill = new SolidFill { Color = Color.white },
            PathProps = new PathProperties
            {
                Stroke = new Stroke { Color = Color.black, HalfThickness = 2f }
            }
        };
        var scene = new Scene
        {
            Root = new SceneNode
            {
                Shapes = new List<Shape> { shape },
                Children = new List<SceneNode>(),
                Transform = Matrix2D.identity
            }
        };
        var geoms = VectorUtils.TessellateScene(scene, new()
        {
            StepDistance = 10.0f,
            MaxCordDeviation = 0.1f,
            MaxTanAngleDeviation= 0.1f,
            SamplingStepSize = 1
        });
        //destroy after usage to avoid leaks
        var vImg = VectorUtils.BuildVectorImage(geoms,2u);
        Image el = new();
        el.vectorImage = vImg;
        el.tintColor = Color.green;
        FindFirstObjectByType<UIDocument>().rootVisualElement.Add(el);
        // var sprite = VectorUtils.BuildSprite(geoms, 100f, VectorUtils.Alignment.Center, Vector2.zero, 16, false);
    }

    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Right);
        if (Keyboard.current.xKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Left);
    }
}
