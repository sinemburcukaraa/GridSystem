using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GridSystem : MonoBehaviour
{
    public GameObject prefab;
    public Vector2Int gridSize;
    [SerializeField] private int _scale;
    
    public float offset;
    public List<TileContainer> tiles = new List<TileContainer>();
    public void CreateGrid()
    {
        ClearGrid();

#if UNITY_EDITOR

        Vector3 tileScale;
        float startX, startY;
        CalculateGridParameters(out tileScale, out startX, out startY, gridSize.x, gridSize.y);

        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                var newobj = PrefabUtility.InstantiatePrefab(prefab, this.transform) as GameObject;
                Vector3 position = new Vector3(startX + i * offset, startY - j * offset, 0);
                newobj.transform.localPosition = position;
                newobj.transform.localScale = tileScale;


                newobj.name = "(" + i + "," + j + ")";
                newobj.GetComponent<Tile>().Mypos = new Vector2Int(i, j);
                tiles.Add(new TileContainer(new Vector2Int(i, j), newobj.GetComponent<Tile>()));
            }
        }
        EditorUtility.SetDirty(this);
#endif
    }
    private void CalculateGridParameters(out Vector3 tileScale, out float startX, out float startY, int _width, int _height)
    {
        float gridWidth = _width * offset;
        float gridHeight = _height * offset;
        tileScale = new Vector3(_scale, _scale, 1f);

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        startX = -(gridWidth - offset) / 2;
        startY = (gridHeight - offset) / 2;
    }

    public void ClearGrid()
    {
#if UNITY_EDITOR
        foreach (var item in tiles)
        {
            DestroyImmediate(item.myTile.gameObject);
        }

        tiles.Clear();
        EditorUtility.SetDirty(this);
#endif
    }
    public GameObject FindGridObject(Vector2Int position)
    {
        return tiles.FirstOrDefault(item => item.myPos == position)?.myTile.gameObject;
    }
    public void Resort()
    {
#if UNITY_EDITOR
        tiles = tiles.OrderBy(tile => tile.myTile.transform.position.x)
                     .ThenByDescending(tile => tile.myTile.transform.position.y)
                     .ToList();
        EditorUtility.SetDirty(this);
#endif
    }

    [Serializable]
    public class TileContainer
    {
        public Vector2Int myPos;
        public Tile myTile;

        public TileContainer(Vector2Int pos, Tile tile)
        {
            myPos = pos;
            myTile = tile;
        }
    }
}

[CustomEditor(typeof(GridSystem))]
public class EditorTest : Editor
{
    public bool isActive;
    public int test;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GridSystem grid = (GridSystem)target;
        isActive = GUILayout.Toggle(isActive, "isActive");

        if (isActive)
        {
            if (GUILayout.Button(nameof(grid.CreateGrid)))
            {
                grid.CreateGrid();
            }

            if (GUILayout.Button(nameof(grid.ClearGrid)))
            {
                grid.ClearGrid();
            }

            if (GUILayout.Button(nameof(grid.Resort)))
            {
                grid.Resort();
            }
        }
    }
}
