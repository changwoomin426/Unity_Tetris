using UnityEngine;
using UnityEngine.Tilemaps;

public enum e_Tetromino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}


[System.Serializable]
public struct TetrominoData
{
    public e_Tetromino Tetromino;
    public Tile Tile;

    public Vector2Int[] Cells { get; private set; }
    public Vector2Int[,] WallKicks { get; private set; }

    public void Initailize()
    {
        this.Cells = Datas.Cells[this.Tetromino];
        this.WallKicks = Datas.WallKicks[Tetromino];
    }
}