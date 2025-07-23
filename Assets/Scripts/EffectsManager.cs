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

                GameEvent.onConnectFloorplans += DoubleConnection;
                void DoubleConnection(Floorplan firstFloorplan, Floorplan secondFloorplan)
                {
                    if(firstFloorplan != floorplan && secondFloorplan != floorplan) return;
                    Floorplan other = firstFloorplan == floorplan ? secondFloorplan : firstFloorplan;
                    other.connectedFloorplans.Add(floorplan);
                    GameEvent.onConnectFloorplans -= DoubleConnection;
                    GameEvent.onConnectFloorplans?.Invoke(firstFloorplan, secondFloorplan);
                    GameEvent.onConnectFloorplans += DoubleConnection;
                }
                break;
            case "Dormitory":
                GameEvent.onConnectFloorplans += DormitoryEffect;
                void DormitoryEffect(Floorplan firstFloorplan, Floorplan secondFloorplan)
                {
                    if(!floorplan.ConnectedToFloorplan(firstFloorplan,secondFloorplan, out var other)) return;
                    if(!NumberUtil.ContainsBytes((int)other.Category, (int)FloorCategory.RestRoom)) return;
                    //connected bedrooms gain extra points
                    other.pointBonus.Add(() => 2);
                    //first time entering a connected restroom gain steps
                    GameEvent.OnEnterFloorplan += AddStepsEffect;
                    void AddStepsEffect(Vector2Int coordinates, Floorplan restRoom)
                    {
                        if(restRoom != other) return;
                        Player.ChangeSteps(5);
                        GameEvent.OnEnterFloorplan -= AddStepsEffect;
                    }
                }
                break;
            case "Boudoir":
                GameEvent.OnEnterFloorplan += OnEnterBoudoir;
                GameEvent.OnExitFloorplan += OnExitBoudoir;
                void OnEnterBoudoir(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    if (currentFloorplan != floorplan) return;
                    GameEvent.onDrawFloorplans += IncreaseRestroomChance;
                }
                void OnExitBoudoir(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    if (currentFloorplan != floorplan) return;
                    GameEvent.onDrawFloorplans -= IncreaseRestroomChance;
                }
                void IncreaseRestroomChance(DrawFloorplanEvent evt)
                {
                    int restroomCount = 0;
                    for (int i = 0; i < evt.drawnFloorplans.Length; i++)
                    {
                        if (!NumberUtil.ContainsBytes((int)evt.drawnFloorplans[i].Category,
                                (int)FloorCategory.RestRoom)) continue;
                        restroomCount++;
                    }

                    if (restroomCount > 0) return;
                    List<Floorplan> possibleRestroom = new();
                    RarityPicker<Floorplan> modifiedList = new();
                    for (int i = 0; i < evt.possibleFloorplans.Count; i++)
                    {
                        Floorplan restRoom = evt.possibleFloorplans[i];
                        if (!NumberUtil.ContainsBytes((int)restRoom.Category, (int)FloorCategory.RestRoom))
                            continue;
                        //check if it is selected already
                        if (restRoom == evt.drawnFloorplans[0] ||
                            restRoom == evt.drawnFloorplans[1] ||
                            restRoom == evt.drawnFloorplans[2]) continue;
                        modifiedList.AddToPool(restRoom, restRoom.Rarity);
                        possibleRestroom.Add(restRoom);
                    }

                    if (possibleRestroom.Count <= 0) return;
                    int id = Random.Range(0, 2);
                    evt.drawnFloorplans[id] = modifiedList.PickRandom();
                }
                break;
            case "Guest Bedroom":
                GameEvent.OnEnterFloorplan += GuestBedroomStepsEffect;
                void GuestBedroomStepsEffect(Vector2Int coordinates, Floorplan restRoom)
                {
                    if(restRoom != floorplan) return;
                    Player.ChangeSteps(2);
                }
                GameEvent.onConnectFloorplans += GuestBedroomEffect;
                void GuestBedroomEffect(Floorplan firstFloorplan, Floorplan secondFloorplan)
                {
                    if(firstFloorplan != floorplan && secondFloorplan != floorplan) return;
                    Floorplan other = firstFloorplan == floorplan ? secondFloorplan : firstFloorplan;
                    if(!NumberUtil.ContainsBytes((int)other.Category, (int)FloorCategory.RestRoom)) return;
                    //bonus points equal to connected restrooms points
                    floorplan.pointBonus.Add(other.CalculatePoints);
                }
                break;
            case "Great Hall":
                //extra points for each different type of room connected
                FloorCategory connectedCategories = 0;
                floorplan.pointBonus.Add(() => NumberUtil.SeparateBits((int)connectedCategories).Length * 2);
                
                GameEvent.onConnectFloorplans += GreatHallEffect;
                void GreatHallEffect(Floorplan firstFloorplan, Floorplan secondFloorplan)
                {
                    if(!floorplan.ConnectedToFloorplan(firstFloorplan,secondFloorplan, out var other)) return;
                    connectedCategories |= other.Category;
                    Debug.Log(NumberUtil.SeparateBits((int)connectedCategories).Length);
                }
                break;
            case "Tunnel":
                //surprise if reach the edge?
                int exitId = (floorplan.entranceId + 2) % 4;
                if (!GridManager.instance.ValidCoordinate
                    (coordinates + Floorplan.IDToDirection(exitId)))
                {
                    floorplan.connections[exitId] = false;
                    floorplan.AddItemToFloorplan(new Key(5));
                    floorplan.OnChanged?.Invoke();
                }
                //Aways draw a tunnel when drafting from tunnel
                GameEvent.OnEnterFloorplan += OnEnterTunnel;
                GameEvent.OnExitFloorplan += OnExitTunnel;
                void OnEnterTunnel(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    if(currentFloorplan != floorplan) return;
                    GameEvent.onDrawFloorplans += AddTunnelToDrawnFloorplans;
                }
                void OnExitTunnel(Vector2Int currentCoordinates, Floorplan currentFloorplan)
                {
                    if(currentFloorplan != floorplan) return;
                    GameEvent.onDrawFloorplans -= AddTunnelToDrawnFloorplans;
                }
                void AddTunnelToDrawnFloorplans(DrawFloorplanEvent evt)
                {
                    Floorplan tunnel = floorplan.original.CreateInstance(Floorplan.IDToDirection(floorplan.entranceId));
                    int id = Random.Range(0, 2);
                    evt.drawnFloorplans[id] = tunnel;
                }
                break;
            case "":
                break;
        }
    }
}
