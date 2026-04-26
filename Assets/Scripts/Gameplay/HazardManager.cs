using UnityEngine;
using Util;

namespace Gameplay
{
    public static class HazardManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            Debug.Log("initialize");
            GameEvent.onGameStart += TestHazard;
            GameEvent.onGameStart += RelicTest;
        }

        private static void TestHazard(Event evt)
        {
            Room hazardRoom = null;
            hazardRoom = Helpers.CreateRoom("Hazard Room", "-20 points, +30 points when a room connects to this room", -20, 
                RoomType.DeadEnd, RoomCategory.Blank,
                entrance: Vector2Int.up, onDraftEffect: RoomEffect);
            hazardRoom.Rotate();
            GameManager.PlaceRoom(hazardRoom, new(2, 2));
            GameEvent.onGameStart -= TestHazard;
            void RoomEffect(CoordinateEvent coordinates)
            {
                hazardRoom?.EveryTime().RoomConnected().AddPointsToRoom(30);
            }
        }

        private static void RelicTest(Event evt)
        {
            GameEvent.onDrawRooms += EnsureLastModify;
            void EnsureLastModify(DrawRoomEvent evt)
            {
                GameEvent.onModifyDraw += HideRooms;
            }
            
            void HideRooms(DrawRoomEvent data)
            {
                Debug.Log("relic effect");
                GameEvent.onModifyDraw -= HideRooms;
                GameEvent.onDraftedRoom += ResetEffect;
                for (int i = 0; i < data.drawnRooms.Length; i++)
                {
                    var room = data.drawnRooms[i];
                    room = room.CreateInstance(Room.IDToDirection(room.entranceId));
                    room.Name = "?????";
                    room.basePoints = 0;
                    room.Description = "?????";
                    room.Category = RoomCategory.Blank;
                    data.drawnRooms[i] = room;
                }
            }

            void ResetEffect(RoomEvent evt)
            {
                Debug.Log("revert relic effect");
                GameEvent.onDraftedRoom -= ResetEffect;
                var room = evt.Room;
                var original = room.original;
                while (original.Name == "?????") original = original.original;
                room.Name = original.Name;
                room.basePoints = original.basePoints;
                room.Description = original.Description;
                room.Category = original.Category;
                room.OnChanged?.Invoke();
            }
            
            return;
            //bonus for room drafted test
            Room relicRoom = null;
            int value = 0;
            HouseStatsWindow.OnCheckBonus += PointBonus;
            relicRoom = Helpers.CreateRoom("Does it work?", "", 0, RoomType.DeadEnd, RoomCategory.Blank);
            relicRoom.EveryTime().AnyRoomIsDrafted().Do(_ =>
            {
                Debug.Log("Relic Test");
                value++;
            });
            void PointBonus(SequenceManager sequence) =>
                sequence.Add(new SequenceManager().Add(HouseStatsWindow.PointBonusSequence("relic", value))
                    .Add(HouseStatsWindow.delaySequence));
        }
    }
}