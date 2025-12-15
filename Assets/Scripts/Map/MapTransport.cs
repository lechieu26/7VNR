using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapTransport : MonoBehaviour
{
    public Transport leftTransport;
    public Transport rightTransport;
    public GameObject edgeParent;
    public MapJsonData mapData;
    private GamePlay gamePlay;

    void Start()
    {
        if (leftTransport != null)
            leftTransport.OnEnterTransport += HandleLeftTransport;
        if (rightTransport != null)
            rightTransport.OnEnterTransport += HandleRightTransport;
    }

    public void SetData(GamePlay gamePlay, MapJsonData mapData)
    {
        this.gamePlay = gamePlay;
        this.mapData = mapData;
        leftTransport.mapNext = mapData.leftMapPrefab;
        rightTransport.mapNext = mapData.rightMapPrefab;
    }

    private void HandleLeftTransport()
    {
        gamePlay.LoadMap(leftTransport.mapNext);
        gamePlay.SetPlayerPositionWhenEnterWaypoint(true);
        Destroy(gameObject);
    }
    private void HandleRightTransport()
    {

        gamePlay.LoadMap(rightTransport.mapNext);
        gamePlay.SetPlayerPositionWhenEnterWaypoint(false);
        Destroy(gameObject);
    }
}
