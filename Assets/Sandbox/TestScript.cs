using System;
using System.Collections;
using System.Collections.Generic;
using Lenix.NumberUtilities;
using SerializableMethods;
using UI.Utils;
using UI.Utils.Builder;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Scene = Unity.VectorGraphics.Scene;

public class TestScript : MonoBehaviour
{
    [SerializeField] private FloorplanColors colors;
    private VisualElement root;
    private VectorImage vImg;

    private RoomUI roomUi;
    
    private void Start()
    {
        GameSettings.current = new();
        GameSettings.current.roomColors = colors;
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
        RoomUI.vImg = vImg;
        root = FindFirstObjectByType<UIDocument>().rootVisualElement;
        root.Add(roomUi = new RoomUI().Size(200).Position(-50).LayoutOffset(50, unit: LengthUnit.Percent));
        // var sprite = VectorUtils.BuildSprite(geoms, 100f, VectorUtils.Alignment.Center, Vector2.zero, 16, false);
    }

    private void OnDestroy()
    {
        Destroy(vImg);
    }

    [SerializeMethod]
    private void SetRoom(Room room)
    {
        roomUi.Setup(room.CreateInstance(Vector2Int.down));
    }
    
    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Right);
        if (Keyboard.current.xKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Left);
    }
}

public class RoomUI : VisualElement
{
    public static VectorImage vImg;
    
    private Label name;
    private Image[] colorSections;
    private VisualElement[] doorBlockers;

    private Room room;

    public RoomUI(Room room = null)
    {
        int offset = 50;
        int space = 49;
        Vector3 flip = new(-1, 1, 1);
        this
            .JustifyContent(Justify.Center)
            .AlignItems(Align.Center)
            .BgColor(Color.black);
        doorBlockers = new[]
        {
            new VisualElement()
                .Size(25, 50, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(-50)
                .AbsPos()
                .AddTo(this),
            new VisualElement()
                .Size(25, 50, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(50, -50)
                .AbsPos()
                .AddTo(this),
            new VisualElement()
                .Size(50, 25, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(50, -50)
                .AbsPos()
                .AddTo(this),
            new VisualElement()
                .Size(50, 25, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(50)
                .AbsPos()
                .AddTo(this),
            new VisualElement()
                .Size(25, 50, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(50, 50)
                .AbsPos()
                .AddTo(this),
            new VisualElement()
                .Size(25, 50, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(-50, 50)
                .AbsPos()
                .AddTo(this),
            new VisualElement()
                .Size(50, 25, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(-50, 50)
                .AbsPos()
                .AddTo(this),
            new VisualElement()
                .Size(50, 25, LengthUnit.Percent)
                .BgColor(Color.gray8)
                .Position(-50, -50)
                .AbsPos()
                .AddTo(this),
        };
        colorSections = new[]
        {
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(-space, -offset, unit: LengthUnit.Percent)
                .AbsPos()
                .AddTo(this),
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(space, -offset, unit: LengthUnit.Percent)
                .Scale(flip)
                .AbsPos()
                .AddTo(this),
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(offset, -space, unit: LengthUnit.Percent)
                .Rotation(90)
                .AbsPos()
                .AddTo(this),
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(offset, space, unit: LengthUnit.Percent)
                .Scale(flip)
                .Rotation(90)
                .AbsPos()
                .AddTo(this),
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(space, offset, unit: LengthUnit.Percent)
                .Rotation(180)
                .AbsPos()
                .AddTo(this),
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(-space, offset, unit: LengthUnit.Percent)
                .Scale(flip)
                .Rotation(180)
                .AbsPos()
                .AddTo(this),
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(-offset, space, unit: LengthUnit.Percent)
                .Rotation(270)
                .AbsPos()
                .AddTo(this),
            new Image()
                .Image(vImg)
                .Tint(Color.gray5)
                .Position(-offset, -space, unit: LengthUnit.Percent)
                .Scale(flip)
                .Rotation(270)
                .AbsPos()
                .AddTo(this),
        };
        Add(new VisualElement().AbsPos().BgColor(Color.gray7).Size(75,unit:LengthUnit.Percent).BorderColor(Color.black).BorderSize(1));
        Add(name = new Label("Room Name")
            .AbsPos()
            .Size(75, unit: LengthUnit.Percent)
            .TextAlign(TextAnchor.MiddleCenter)
            .WrapText(WhiteSpace.Normal)
            .AutoSize(30, 50));
        if (room != null) Setup(room);
    }

    public void Setup(Room newRoom)
    {
        if (ReferenceEquals(newRoom, null)) return;
        room = newRoom;
        name.text = room.Name;
        for (int i = 0; i < room.connections.Length; i++)
        {
            bool open = room.connections[i];
            var display = open ? DisplayStyle.None : DisplayStyle.Flex;
            doorBlockers[i * 2].Display(display);
            doorBlockers[(i * 2) + 1].Display(display);
        }
        
        SetColors();
    }

    private void SetColors()
    {
        int[] categories = NumberUtil.SeparateBits((int)room.Category);
        int step = colorSections.Length / categories.Length;
        for (int id = 0; id < colorSections.Length; id++)
        {
            var colorId = Math.Min(id / step, categories.Length - 1);
            var color = GameSettings.current.roomColors.GetColor((RoomCategory)categories[colorId]);
            int i = (id + 2) % colorSections.Length;
            colorSections[i].Tint(color);
            doorBlockers[i].BgColor(color);
        }
    }
}