using AddressableAsyncInstances;
using System;
using System.Collections.Generic;
using System.Text;
using SerializableMethods;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DraftManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject draftScreen;
    [SerializeField] private Transform floorplansContainer;
    [SerializeField] private Button rerollButton;

    private FloorplanDetails detailsPrefab;

    private int amountDrafted = 3;
    private PlayerDeck playerDeck;
    private List<FloorplanDetails> draftList = new();
    private List<Room> draftPool;

    public event Action<Room> OnDraftRoom;

    private TMP_Text rerollCount;
    private Vector2Int lastDraftDirection;
    private int lastDraftHeight;
    private Vector2Int lastTargetSlot;

    private const float commonRate = .7f;
    private const float uncommonRate = .27f;
    private const float rareRate = .03f;
    private const float legendRate = .0f;

    private float commonGrowth;
    private float uncommonGrowth;
    private float rareGrowth;
    private float legendGrowth;

    public void Setup(int draftOptions, PlayerDeck deck = null, Action<List<Room>> onDone = null)
    {
        if (!ReferenceEquals(detailsPrefab, null)) LoadDraftPool();
        else
        {
            AAComponent<FloorplanDetails>.LoadComponent("FloorplanDetails", prefab =>
            {
                detailsPrefab = prefab;
                LoadDraftPool();
            });
        }

        void LoadDraftPool()
        {
            amountDrafted = draftOptions;
            draftList.EnsureEnoughInstances(detailsPrefab, amountDrafted, floorplansContainer, i => i.onPickedFloorplan += PickRoom);
            playerDeck = deck;

            rerollButton.onClick.AddListener(RedrawRooms);
            rerollCount = rerollButton.GetComponentInChildren<TMP_Text>();

            if (!ReferenceEquals(playerDeck, null))
            {
                draftPool = new(playerDeck.deck.Count);
                for (int i = 0; i < draftPool.Capacity; i++)
                    draftPool.Add(playerDeck.deck[i].CreateInstance(Vector2Int.up));
                onDone?.Invoke(draftPool);
                CheckPoolData();
                return;
            }
            draftPool = new();
            Addressables.LoadAssetsAsync<Room>("BaseFloorplan", floorplan =>
                draftPool.Add(floorplan.CreateInstance(Vector2Int.up))).Completed += _ =>
                {
                    onDone?.Invoke(draftPool);
                    CheckPoolData();
                };
        }
    }

    private void CheckPoolData()
    {
        //Checking data
        float finalCommonRate = .4f;
        float finalUncommonRate = .3f;
        float finalRareRate = .28f;
        float finalLegendRate = .02f;

        commonGrowth = (finalCommonRate - commonRate) / (GridManager.ySize - 1);
        uncommonGrowth = (finalUncommonRate - uncommonRate) / (GridManager.ySize - 1);
        rareGrowth = (finalRareRate - rareRate) / (GridManager.ySize - 1);
        legendGrowth = (finalLegendRate - legendRate) / (GridManager.ySize - 1);

        int[] rarityCount = new int[4];
        int[] costCount = new int[5];
        Dictionary<RoomType, int> typesCount = new();
        Dictionary<int, int> pointsCount = new();
        for (int i = 0; i < draftPool.Count; i++)
        {
            int rarity = (int)draftPool[i].Rarity;
            rarityCount[rarity]++;
            costCount[draftPool[i].keyCost]++;

            if (!typesCount.ContainsKey(draftPool[i].Type)) typesCount.Add(draftPool[i].Type, 1);
            else typesCount[draftPool[i].Type]++;

            if (!pointsCount.ContainsKey(draftPool[i].basePoints)) pointsCount.Add(draftPool[i].basePoints, 1);
            else pointsCount[draftPool[i].basePoints]++;
        }

        StringBuilder sb = new("Rarities:");
        for (int i = 0; i < rarityCount.Length; i++)
            sb.Append($"\n{(Rarity)i}: {rarityCount[i]}");
        Debug.LogWarning(sb.ToString());

        sb = new($"Costs:");
        for (int i = 0; i < costCount.Length; i++)
            sb.Append($"\n{i}: {costCount[i]}");
        Debug.LogWarning(sb.ToString());

        sb = new($"Types:");
        foreach (var type in typesCount)
            sb.Append($"\n{type.Key}: {type.Value}");
        Debug.LogWarning(sb.ToString());

        sb = new($"Points:");
        foreach (var point in pointsCount)
            sb.Append($"\n{point.Key}: {point.Value}");
        Debug.LogWarning(sb.ToString());
    }

    public void DraftRoom(Vector2Int direction = default, Vector2Int? targetSlot = null, int draftHeight = 0)
    {
        bool choseDirection = direction.sqrMagnitude > 0;
        if (!choseDirection) direction = Vector2Int.up;

        Vector2Int slot = targetSlot ?? Vector2Int.zero;
        List<RoomType> possibleTypes =
            Helpers.GetPossibleRoomTypes(slot, out var possibleSlots);
        if (!possibleSlots.Contains(direction))
            possibleTypes.Remove(RoomType.Straw);

        //StringBuilder sb = new();
        //for (int i = 0; i < possibleTypes.Count; i++)
        //    sb.Append($"{possibleTypes[i]} | ");
        //Debug.Log(sb.ToString());

        //pick possible ones
        List<Room> possibleRooms = new();
        RarityPicker<Room> roomPicker = GetRarityPicker(draftHeight);
        for (int i = 0; i < draftPool.Count; i++)
        {
            if (!possibleTypes.Contains(draftPool[i].Type)) continue;
            Room floorplan = draftPool[i];
            possibleRooms.Add(floorplan);
            roomPicker.AddToPool(floorplan, floorplan.Rarity);
        }

        int missingRoom = amountDrafted - possibleRooms.Count;
        for (int i = 0; i < missingRoom; i++)
        {
            var spareRoom = Helpers.CreateSpareRoom(possibleTypes: possibleTypes);
            possibleRooms.Add(spareRoom);
            roomPicker.AddToPool(spareRoom, spareRoom.Rarity);
        }

        lastDraftDirection = direction;
        lastDraftHeight = draftHeight;
        lastTargetSlot = slot;
        rerollButton.gameObject.SetActive(Player.dices > 0);
        rerollCount?.SetText($"{Player.dices}");

        DrawRoomEvent evt = new();
        evt.targetCoordinate = (GridManager.instance?.currentPosition ?? Vector2Int.zero) + direction;
        evt.drawnRooms = new Room[amountDrafted];
        evt.possibleFloorTypes = possibleTypes;
        evt.possibleRooms = possibleRooms;
        evt.roomPicker = roomPicker;

        for (int i = 0; i < amountDrafted - 1; i++) AddToDraftList(i);
        //last one is rarer
        AddToDraftList(amountDrafted - 1, roomPicker.commonRate);
        GameEvent.onDrawRooms?.Invoke(evt);
        GameEvent.onDrawChange?.Invoke(evt);
        GameEvent.onModifyDraw?.Invoke(evt);

        void AddToDraftList(int id, float rarityOffset = 0)
        {
            Room floorplan = roomPicker.PickRandom(rarityOffset, true);
            evt.drawnRooms[id] = floorplan;
        }

        int keysRequiredRoom = 0;
        for (int i = 0; i < amountDrafted; i++)
        {
            if(evt.drawnRooms[i].keyCost <= 0) continue;
            keysRequiredRoom++;
        }

        bool removeCost = keysRequiredRoom >= amountDrafted;
        for (int i = 0; i < amountDrafted; i++)
        {
            Room floorplan = evt.drawnRooms[i].CreateInstance(-direction);
            if (i == 0 && removeCost) floorplan.keyCost = 0;
            int randomRotation = Random.Range(0, 3);
            for (int j = 0; j < randomRotation; j++) floorplan.Rotate();
            floorplan.CorrectRotation(possibleSlots);
            FloorplanDetails instance = draftList[i];
            instance.Setup(floorplan);
            instance.FloorplanUI.HighlightDirection(choseDirection ? -direction : Vector2Int.zero);
        }

        background?.SetActive(true);
        draftScreen.SetActive(true);
    }

    private RarityPicker<Room> GetRarityPicker(int height)
    {
        RarityPicker<Room> rarityPicker = new(
            commonRate + commonGrowth * height,
            uncommonRate + uncommonGrowth * height,
            rareRate + rareGrowth * height,
            legendRate + legendGrowth * height);
        //Debug.Log($"floor {height}:\n{rarityPicker.commonRate}\n{rarityPicker.uncommonRate}\n{rarityPicker.rareRate}\n{rarityPicker.legendRate}");

        return rarityPicker;
    }

    public void PickRoom(Room room)
    {
        OnDraftRoom?.Invoke(room);
    }

    public void RemoveRoomFromPool(Room room)
    {
        Room originalRoom = room.FindOriginal(draftPool);
        draftPool.Remove(originalRoom);
    }

    [SerializeMethod]
    public void RotateRooms()
    {
        for (int i = 0; i < draftList.Count; i++)
        {
            draftList[i].currentRoom.Rotate();
        }
    }

    public void RedrawRooms()
    {
        Player.dices--;
        DraftRoom(lastDraftDirection, lastTargetSlot, lastDraftHeight);
    }

    public void CloseWindow()
    {
        background?.SetActive(false);
        draftScreen.SetActive(false);
    }
}

public class DrawRoomEvent : Event
{
    public Vector2Int targetCoordinate;
    public Room[] drawnRooms;
    public List<Room> possibleRooms;
    public List<RoomType> possibleFloorTypes;
    public RarityPicker<Room> roomPicker;
}