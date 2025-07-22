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
                GameEvent.onDraftedFloorplan -= AddFloorplanEffect;
                GameEvent.onDraftedFloorplan?.Invoke(coordinates,floorplan);
                GameEvent.onDraftedFloorplan += AddFloorplanEffect;
                break;
            case "Dormitory":
                GameEvent.onConnectFloorplan += DormitoryEffect;
                void DormitoryEffect(Floorplan firstFloorplan, Floorplan secondFloorplan)
                {
                    if(firstFloorplan != floorplan && secondFloorplan != floorplan) return;
                    Floorplan other = firstFloorplan == floorplan ? secondFloorplan : firstFloorplan;
                    if(!NumberUtil.ContainsBytes((int)other.Category, (int)FloorCategory.RestRoom)) return;
                    GameEvent.OnEnterFloorplan += AddStepsEffect;
                    void AddStepsEffect(Vector2Int coordinates, Floorplan restRoom)
                    {
                        if(restRoom != other) return;
                        Debug.Log("dormitory effect");
                        Player.ChangeSteps(5);
                        GameEvent.OnEnterFloorplan -= AddStepsEffect;
                    }
                }
                break;
            case "":
                break;
        }
    }
}
