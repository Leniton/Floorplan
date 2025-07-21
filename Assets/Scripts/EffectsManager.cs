using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lenix.NumberUtilities;

public class EffectsManager : MonoBehaviour
{
    private void Start()
    {
        GameEvent.onDraftedFloorplan += AddFloorplanEffect;
    }

    private void AddFloorplanEffect(Vector2Int coordinates, Floorplan floorplan)
    {
        switch (floorplan.Name)
        {
            case "Bedroom":
                GameEvent.OnEnterFloorplan += BedroomEffect;
                void BedroomEffect(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    if (currentFloorplan != floorplan) return;
                    Player.ChangeSteps(5);
                    GameEvent.OnEnterFloorplan -= BedroomEffect;
                }
                break;
            case "Bathroom":
                GameEvent.OnEnterFloorplan += BathroomEffect;
                void BathroomEffect(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    if (currentFloorplan != floorplan) return;
                    int currentSteps = Player.steps;
                    currentSteps = Mathf.CeilToInt(currentSteps / 10f);
                    Player.ChangeSteps((currentSteps * 10) - Player.steps);
                    GameEvent.OnEnterFloorplan -= BathroomEffect;
                }
                break;
            case "Bunk Room":
                GameEvent.OnEnterFloorplan += BunkRoomEnter;
                GameEvent.OnExitFloorplan += BunkRoomExit;
                void BunkRoomEnter(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    GameEvent.OnEnterFloorplan -= BunkRoomEnter;
                    GameEvent.OnEnterFloorplan?.Invoke(currentCoordinates, currentFloorplan);
                    GameEvent.OnEnterFloorplan += BunkRoomEnter;
                }
                void BunkRoomExit(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    GameEvent.OnExitFloorplan -= BunkRoomExit;
                    GameEvent.OnExitFloorplan?.Invoke(currentCoordinates, currentFloorplan);
                    GameEvent.OnExitFloorplan += BunkRoomExit;
                }
                break;
            case "Dormitory":

                void DormitoryEffect(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    
                }
                break;
            case "":
                break;
        }
    }
}
