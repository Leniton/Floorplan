using System;
using System.Collections;
using System.Collections.Generic;
using UI.Utils;
using UI.Utils.Builder;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Scene = Unity.VectorGraphics.Scene;

public class TestScript : MonoBehaviour
{
    private VisualElement root;
    private VectorImage vImg;
    
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
                        new BezierPathSegment { P0 = new Vector2(0,0),   P1 = new Vector2(0,0),  P2 = new Vector2(80,0)  },
                        new BezierPathSegment { P0 = new Vector2(80,0), P1 = new Vector2(80,0),P2 = new Vector2(100,100)},
                        new BezierPathSegment { P0 = new Vector2(100,100) }
                    },
                    Closed = true
                }
            },
            Fill = new SolidFill { Color = Color.white },
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
        vImg = VectorUtils.BuildVectorImage(geoms);
        root = FindFirstObjectByType<UIDocument>().rootVisualElement;
        root = new VisualElement()
            .AbsPos()
            .LayoutOffset(50, unit: LengthUnit.Percent)
            .Position(-50)
            .AlignItems(Align.Center)
            .JustifyContent(Justify.Center)
            .Size(200)
            .BgColor(Color.black)
            .AddTo(root);
        root.Add(new VisualElement()
            .Size(50,100)
            .BgColor(Color.green)
            .Position(-50,-50)
            .AbsPos());
        root.Add(new VisualElement()
            .Size(50,100)
            .BgColor(Color.green)
            .Position(50,-50)
            .AbsPos());
        Image el = new();
        el
            .Image(vImg)
            .Tint(Color.green)
            .Position(-50, unit: LengthUnit.Percent)
            .AbsPos()
            .AddTo(root);
        el = new();
        el
            .Image(vImg)
            .Tint(Color.green)
            .Position(49,-50, unit: LengthUnit.Percent)
            .AbsPos()
            .Scale(new(-1,1,1))
            .AddTo(root);
        el = new();
        el
            .Image(vImg)
            .Tint(Color.red)
            .Rotation(90)
            .Position(50,-49, unit: LengthUnit.Percent)
            .AbsPos()
            .AddTo(root);
        el = new();
        el
            .Image(vImg)
            .Tint(Color.red)
            .Rotation(90)
            .Position(50,50, unit: LengthUnit.Percent)
            .Scale(new(-1,1,1))
            .AbsPos()
            .AddTo(root);
        el = new();
        el
            .Image(vImg)
            .Tint(Color.blue)
            .Rotation(180)
            .Position(50, unit: LengthUnit.Percent)
            .AbsPos()
            .AddTo(root);
        el = new();
        el
            .Image(vImg)
            .Tint(Color.blue)
            .Rotation(180)
            .Position(-50,50, unit: LengthUnit.Percent)
            .Scale(new(-1,1,1))
            .AbsPos()
            .AddTo(root);
        el = new();
        el
            .Image(vImg)
            .Tint(Color.gray3)
            .Rotation(270)
            .Position(-50,50, unit: LengthUnit.Percent)
            .AbsPos()
            .AddTo(root);
        el = new();
        el
            .Image(vImg)
            .Tint(Color.gray3)
            .Rotation(270)
            .Position(-50, unit: LengthUnit.Percent)
            .Scale(new(-1,1,1))
            .AbsPos()
            .AddTo(root);
        root.Add(new VisualElement().AbsPos().BgColor(Color.grey).Size(150).BorderColor(Color.black).BorderSize(1));
        // var sprite = VectorUtils.BuildSprite(geoms, 100f, VectorUtils.Alignment.Center, Vector2.zero, 16, false);
    }

    private void OnDestroy()
    {
        Destroy(vImg);
    }

    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Right);
        if (Keyboard.current.xKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Left);
    }
}
