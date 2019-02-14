using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class CheckersBoard : MonoBehaviour
    {
        [Tooltip("Prefabs for Checker Pieces")]
        public GameObject whitePiecePrefab, blackPiecePrefab;
        [Tooltip("Where to attach the spawned pieces in the Hierarchy")]
        public Transform checkersParent;
        public Vector3 boardOffset = new Vector3(-4.0f, 0.0f, -4.0f);
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);
        public float rayDistance = 1000f;
        public LayerMask hitLayers;

        public Piece[,] pieces = new Piece[8, 8];

        private bool isWhiteTurn = true, hasKilled;
        private Vector2 mouseOver, startDrag, endDrag;
        private Piece selectedPiece = null;

        void Start()
        {
            GenerateBoard();
        }

        private void Update()
        {

            MouseOver();
            if (isWhiteTurn)
            {
                int x = (int)mouseOver.x;
                int y = (int)mouseOver.y;

                if (Input.GetMouseButtonDown(0))
                {
                    selectedPiece = SelectPiece(x, y);
                    startDrag = new Vector2(x, y);
                }

                if (selectedPiece)
                {
                    DragPiece(selectedPiece);

                }
                if (Input.GetMouseButtonUp(0))
                {
                    endDrag = new Vector2(x, y);
                    TryMove(startDrag, endDrag);
                    selectedPiece = null;
                }
            }

        }

        public void GeneratePiece(int x, int y, bool isWhite)
        {
            // What prefab are we using (white or black) ?
            GameObject prefab = isWhite ? whitePiecePrefab : blackPiecePrefab;
            // Generate Instance of prefab
            GameObject clone = Instantiate(prefab, checkersParent);
            Piece p = clone.GetComponent<Piece>();
            p.x = x;
            p.y = y;
            // Reposition clone
            MovePiece(p, x, y);
        }

        public void GenerateBoard()
        {
            // Generate White Team
            for (int y = 0; y < 3; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, true);
                }
            }
            // Generate Black Team
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, false);
                }
            }
        }

        private Piece SelectPiece(int x, int y)
        {
            if (OutOfBounds(x, y))
            {
                return null;
            }
            Piece piece = pieces[x, y];
            if (piece)
            {
                return piece;
            }
            return null;

        }

        private void MovePiece(Piece p, int x, int y)
        {
            pieces[p.x, p.y] = null;
            pieces[x, y] = p;

            p.x = x;
            p.y = y;
            p.transform.localPosition = new Vector3(x, 0, y) + boardOffset + pieceOffset;
        }
        private void MouseOver()
        {

            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                mouseOver.x = (int)(hit.point.x - boardOffset.x);
                mouseOver.y = (int)(hit.point.z - boardOffset.z);
            }
            else
            {
                mouseOver.x = -1;
                mouseOver.y = -1;
            }
        }
        private void DragPiece(Piece selected)
        {
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                selected.transform.position = hit.point + Vector3.up;
            }
        }
        private void TryMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;

            startDrag = new Vector2(x1, y1);
            endDrag = new Vector2(x2, y2);

            if (selectedPiece)
            {
                if (OutOfBounds(x2, y2))
                {
                    MovePiece(selectedPiece, x1, y1);
                    return;
                }
                if (ValidMove(start, end))
                {
                    MovePiece(selectedPiece, x2, y2);
                }
            }
        }
        private bool OutOfBounds(int x, int y)
        {
            return x < 0 || x >= 8 || y < 0 || y >= 8;
        }
        private bool ValidMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;

            if (start == end)
            {
                // You can move back where you were
                return true;
            }
 
            if (pieces[x2, y2])
            {
                // YA CAN'T DO DAT!
                return false;
            }

            // Yeah... Alright, you can do dat.
            return true;
        }
    }
}

/*if (!Camera.main)
            {
                Debug.LogError("Unable to find main camera");
                return;
            }
            */
