using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject redPiecePrefab, whitePiecePrefab;
    public Vector3 boardOffset = new Vector3(-0.4f,0,-0.4f);
    public Vector3 pieceOffset = new Vector3(.5f,0,.5f);
    public Piece[,] pieces = new Piece[8, 8];

    private Vector2Int mouseOver;
    private Piece selectedPiece;

    Piece GetPiece(Vector2Int cell)
    {
        return pieces[cell.x, cell.y];
    }

    bool IsOutOfBounds(Vector2Int cell)
    {
        return cell.x < 0 || cell.x >= 8 ||
               cell.y < 0 || cell.y >= 8;
    }

    Piece SelectPiece(Vector2Int cell)
    {
        if (IsOutOfBounds(cell))
        {
            return null;
        }

        Piece piece = GetPiece(cell);

        if (piece)
        {
            return piece;
        }
        return null;
    }

    void MouseOver()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(camRay, out hit))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver = new Vector2Int(-1, -1);
        }
    }

    bool TryMove(Piece selected, Vector2Int desiredCell)
    {
        Vector2Int startCell = selected.cell;
        if (!ValidMove(selected, desiredCell))
        {
            MovePiece(selected, startCell);
            return false;
        }
        MovePiece(selected, desiredCell);
        return true;
    }

    void DragPiece(Piece selected)
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(camRay, out hit))
        {
            selected.transform.position = hit.point + Vector3.up;
        }
    }

    void Update()
    {
        MouseOver();
        if (Input.GetMouseButtonDown(0))
        {
            selectedPiece = SelectPiece(mouseOver);
        }
        if (selectedPiece)
        {
            DragPiece(selectedPiece);
            if(Input.GetMouseButtonUp(0))
            {
                TryMove(selectedPiece, mouseOver);
                selectedPiece = null;
            }
        }
    }

    bool ValidMove(Piece selected, Vector2Int desiredCell)
    {
        Vector2Int direction = selected.cell - desiredCell;
        #region Rule 1: You can't go out of bounds.
        if (IsOutOfBounds(desiredCell))
        {
            Debug.Log("<color=red>MOVEMENT DENIED - LEAVING THE BATTLEFIELD IS PROHIBITED</color>");
            return false;
        }
        #endregion
        #region Rule 2: 
        #endregion
        #region Rule 3: 
        #endregion
        #region Rule 4: 
        #endregion
        #region Rule 5: 
        #endregion
        #region Rule 6: 
        #endregion
        #region Rule 7: 
        #endregion

        Debug.Log("<color=green>MOVEMENT GRANTED</color>");
        return true;
    }

    Vector3 GetWorldPosition(Vector2Int cell)
    {
        return new Vector3(cell.x, 0, cell.y) + boardOffset + pieceOffset;
    }

    void MovePiece(Piece piece, Vector2Int newCell)
    {
        Vector2Int oldCell = piece.cell;
        pieces[oldCell.x, oldCell.y] = null;
        pieces[newCell.x, newCell.y] = piece;
        piece.oldCell = oldCell;
        piece.cell = newCell;
        piece.transform.localPosition = GetWorldPosition(newCell);
    }

    void GeneratePiece(GameObject prefab, Vector2Int desiredCell)
    {
        GameObject clone = Instantiate(prefab, transform);
        Piece piece = clone.GetComponent<Piece>();
        piece.oldCell = desiredCell;
        piece.cell = desiredCell;
        MovePiece(piece, desiredCell);
    }

    void GenerateBoard()
    {
        Vector2Int desiredCell = Vector2Int.zero;
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = y % 2 == 0;
            for (int x = 0; x < 8; x += 2)
            {
                desiredCell.x = oddRow ? x : x + 1;
                desiredCell.y = y;
                GeneratePiece(whitePiecePrefab, desiredCell);
            }
        }
        for (int y = 5; y < 8; y++)
        {
            bool oddRow = y % 2 == 0;
            for (int x = 0; x < 8; x += 2)
            {
                desiredCell.x = oddRow ? x : x + 1;
                desiredCell.y = y;
                GeneratePiece(redPiecePrefab, desiredCell);
            }
        }
    }

    private void Start()
    {
        GenerateBoard();
    }
}
