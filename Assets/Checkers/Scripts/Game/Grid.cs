using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Checkers
{
    using ForcedMoves = Dictionary<Piece, List<Vector2Int>>;
    public class Grid : MonoBehaviour
    {
        public GameObject redPiecePrefab, whitePiecePrefab;
        public Vector3 boardOffset = new Vector3(-4f, 0, -4f);
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);
        public Piece[,] pieces = new Piece[8, 8];
        public bool isWhiteTurn = true;

        private Vector2Int mouseOver;
        private Piece selectedPiece;

        private ForcedMoves forcedMoves = new ForcedMoves();

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
                if (Input.GetMouseButtonUp(0))
                {
                    TryMove(selectedPiece, mouseOver);
                    selectedPiece = null;
                }
            }
        }

        bool IsForcedMove(Piece selected, Vector2Int desiredCell)
        {
            // Does the selected piece have a forced move?
            if (forcedMoves.ContainsKey(selected))
            {
                // Is there any forced moves for this piece?
                if (forcedMoves[selected].Contains(desiredCell))
                {
                    // It is a forced move
                    return true;
                }
            }
            // It is not a forced move
            return false;
        }

        bool HasForcedMoves(Piece selected)
        {
            foreach (var move in forcedMoves)
            {
                Piece piece = move.Key;
                if (piece.isWhite == selected.isWhite)
                {
                    // Has forced moves!
                    return true;
                }
            }
            // Does not have any forced moves
            return false;
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
            #region Rule 2: Can't select the cell same as desired.
            if (selected.cell == desiredCell)
            {
                Debug.Log("<color=red>MOVEMENT DENIED - PLACING YOUR PIECE BACK IS NOT A VALID MOVE.</color>");
                return false;
            }
            #endregion
            #region Rule 3: You can't place a piece on top of another one.
            if (GetPiece(desiredCell))
            {
                Debug.Log("<color=red>MOVEMENT DENIED - PLACING YOUR PIECE ON TOP OF ANOTHER IS NOT ALLOWED.</color>");
                return false;
            }
            #endregion
            #region Rule 4: forced move

            // Is there any forced moves?
            if (HasForcedMoves(selected))
            {
                // If it is not a 
                if (!IsForcedMove(selected, desiredCell))
                {
                    Debug.Log("<color=red>Invalid - You have to use forced moves!</color>");
                    return false;
                }
            }
            #endregion
            #region Rule 5: 
            if (direction.magnitude > 2)
            {
                if (forcedMoves.Count == 0)
                {
                    Debug.Log("<color=red>Invalid - You can only move two spaces if there are forced moves on selected piece</color>");
                    return false;
                }
            }
            #endregion
            #region Rule 6: moving diagonally
            // Is the player not moving diagonally?
            if (Mathf.Abs(direction.x) != Mathf.Abs(direction.y))
            {
                Debug.Log("<color=red>Invalid - You have to be moving diagonally</color>");
                return false;
            }
            #endregion
            #region Rule 7: cant move back unless king
            // Is the selected piece not a king?
            if (!selectedPiece.isKing)
            {
                // Is the selected piece white?
                if (selectedPiece.isWhite)
                {
                    // Is it moving down?
                    if (direction.y > 0)
                    {
                        Debug.Log("<color=red>Invalid - Can't move a white piece backwards</color>");
                        return false;
                    }
                }
                // Is the selected piece red?
                else
                {
                    // Is it moving up?
                    if (direction.y < 0)
                    {
                        Debug.Log("<color=red>Invalid - Can't move a red piece backwards</color>");
                        return false;
                    }
                }
            }
            #endregion

            Debug.Log("<color=green>MOVEMENT GRANTED</color>");
            return true;
        }
        Piece GetPieceBetween(Vector2Int start, Vector2Int end)
        {
            Vector2Int cell = Vector2Int.zero;
            cell.x = (start.x + end.x) / 2;
            cell.y = (start.y + end.y) / 2;
            return GetPiece(cell);
        }

        void RemovePiece(Piece pieceToRemove)
        {
            Vector2Int cell = pieceToRemove.cell;
            // Clear cell in 2D array
            pieces[cell.x, cell.y] = null;
            // Destroy the gameobject of the piece immediately
            DestroyImmediate(pieceToRemove.gameObject);
        }

        bool IsPieceTaken(Piece selected)
        {
            // Get the piece in between move
            Piece pieceBetween = GetPieceBetween(selected.oldCell, selected.cell);
            // If there is a piece between and the piece isn't the same color
            if (pieceBetween != null && pieceBetween.isWhite != selected.isWhite)
            {
                // Destroy the piece between
                RemovePiece(pieceBetween);
                // Piece taken
                return true;
            }
            // Piece not taken
            return false;
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
        void CheckForcedMove(Piece piece)
        {
            // Get cell location of piece
            Vector2Int cell = piece.cell;

            // Loop through adjacent cells of cell
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    // Create offset cell from index
                    Vector2Int offset = new Vector2Int(x, y);
                    // Creating a new X from piece coordinates using offset
                    Vector2Int desiredCell = cell + offset;

                    #region Check #01 - Correct Direction?
                    // Is the piece not king?
                    if (!piece.isKing)
                    {
                        // Is the piece white?
                        if (piece.isWhite)
                        {
                            // Is the piece moving backwards?
                            if (desiredCell.y < cell.y)
                            {
                                // Invalid - Check next one
                                continue;
                            }
                        }
                        // Is the piece red?
                        else
                        {
                            // Is the piece moving backwards?
                            if (desiredCell.y > cell.y)
                            {
                                // Invalid - Check next one
                                continue;
                            }
                        }
                    }
                    #endregion

                    #region Check #02 - Is the adjacent cell out of bounds?
                    if (IsOutOfBounds(desiredCell))
                    {
                        // Invalid - Check next one
                        continue;
                    }
                    #endregion

                    // Try getting the piece at coordinates
                    Piece detectedPiece = GetPiece(desiredCell);

                    #region Check #03 - Is the desired cell empty?
                    if (detectedPiece == null)
                    {
                        // Invalid - Check next one
                        continue;
                    }
                    #endregion

                    #region Check #04 - Is the detected piece the same color?
                    // Is the detected piece the same color
                    if (detectedPiece.isWhite == piece.isWhite)
                    {
                        // Invalid - Check the next one
                        continue;
                    }
                    #endregion

                    // Try getting the diagonal cell next to detected piece
                    Vector2Int jumpCell = cell + (offset * 2);

                    #region Check #05 - Is the jump cell out of bounds?
                    // Is the destination cell out of bounds?
                    if (IsOutOfBounds(jumpCell))
                    {
                        // Invalid - Check the next one
                        continue;
                    }
                    #endregion

                    #region Check #06 - Is there a piece at the jump cell?
                    // Get piece next to the one we want to jump
                    detectedPiece = GetPiece(jumpCell);
                    // Is there a piece there?
                    if (detectedPiece)
                    {
                        // Invalid - Check the next one
                        continue;
                    }
                    #endregion

                    // If you made it here, a forced move has been detected!

                    // Check if forced moves contains the piece we're currently checking
                    if (!forcedMoves.ContainsKey(piece))
                    {
                        // Add it to list of forced moves
                        forcedMoves.Add(piece, new List<Vector2Int>());
                    }
                    // Add the jump cell to the piece's forced moves
                    forcedMoves[piece].Add(jumpCell);
                }
            }
        }
    }
}