using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    public Board Board { get; private set; }
    public TetrominoData Data { get; private set; }
    public Vector3Int[] Cells { get; private set; }
    public Vector3Int Position { get; private set; }
    public int RotationIndex { get; private set; }

    public float StepDelay = 1f;
    public float MoveDelay = 0.1f;
    public float LockDelay = 0.5f;

    private float _stepTime;
    private float _moveTime;
    private float _lockTime;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.Board = board;
        this.Position = position;
        this.Data = data;

        RotationIndex = 0;
        _stepTime = Time.time + StepDelay;
        _moveTime = Time.time + MoveDelay;
        _lockTime = 0f;

        if (this.Cells == null)
        {
            this.Cells = new Vector3Int[data.Cells.Length];
        }

        for (int i = 0; i < Data.Cells.Length; i++)
        {
            this.Cells[i] = (Vector3Int)Data.Cells[i];
        }
    }

    private void Update()
    {
        Board.Clear(this);

        _lockTime += Time.deltaTime;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Rotate(-1);
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Rotate(1);
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HardDrop();
        }

        if (Time.time > _moveTime)
        {
            HandleMoveInputs();
        }

        if (Time.time > _stepTime)
        {
            Step();
        }

        Board.Set(this);
    }

    private void HandleMoveInputs()
    {
        if (Keyboard.current.sKey.isPressed)
        {
            if (Move(Vector2Int.down))
            {
                _stepTime = Time.time + StepDelay;
            }
        }

        if (Keyboard.current.aKey.isPressed)
        {
            Move(Vector2Int.left);
        }

        if (Keyboard.current.dKey.isPressed)
        {
            Move(Vector2Int.right);
        }
    }

    private void Step()
    {
        _stepTime = Time.time + StepDelay;

        Move(Vector2Int.down);

        if (_lockTime >= LockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        Board.Set(this);
        Board.ClearLines();
        Board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = Position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = Board.IsValidPosition(this, newPosition);

        if (valid)
        {
            Position = newPosition;
            _moveTime = Time.time + MoveDelay;
            _lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = RotationIndex;

        RotationIndex = Wrap(RotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(RotationIndex, direction))
        {
            RotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Datas.RotationMatrix;

        for (int i = 0; i < Cells.Length; i++)
        {
            Vector3 cell = Cells[i];

            int x, y;

            switch (Data.Tetromino)
            {
                case e_Tetromino.I:
                case e_Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            Cells[i] = new Vector3Int(x, y, 0);
        }
    }
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < Data.WallKicks.GetLength(1); i++)
        {
            Vector2Int translation = Data.WallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, Data.WallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
