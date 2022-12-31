using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class VoronoiGameField : MonoBehaviour
{

    [SerializeField] private Texture2D _texture;

    [SerializeField] private int _textureSize;

    [SerializeField] private int _baseGridSize;

    private float _baseGridCellSize;
    private float _minimalBasePointSpacing;

    private Vector2Int[,] _baseGrid;
    private List<Vector2Int> _activePoints = new List<Vector2Int>();
    private List<Vector2Int> _basePoints = new List<Vector2Int>();



    // Start is called before the first frame update
    private void OnValidate()
    {
        _baseGridCellSize = (float)_textureSize / _baseGridSize;
        _minimalBasePointSpacing = _baseGridCellSize * Mathf.Sqrt(2);
        _baseGrid = new Vector2Int[_baseGridSize, _baseGridSize];
        _basePoints.Clear();

        if (_texture == null)
        {
            _texture = new Texture2D(_textureSize, _textureSize);
        }

        if (_texture.width != _textureSize)
        {
            _texture.Reinitialize(_textureSize, _textureSize);
        }

        Vector2Int seed = new Vector2Int(UnityEngine.Random.Range(0, _textureSize), UnityEngine.Random.Range(0, _textureSize));
        _baseGrid[(int)(seed.x / _baseGridCellSize), (int)(seed.y / _baseGridCellSize)] = seed;
        _activePoints.Add(seed);

        while (_activePoints.Count > 0)
        {
            TryGenerateNewPoint();
        }

        for (int i = 0; i < _textureSize; i++)
        {
            for (int j = 0; j < _textureSize; j++)
            {
                _texture.SetPixel(i, j, Color.white);
            }
        }

        foreach (var point in _basePoints)
        {
            _texture.SetPixel(point.x, point.y, Color.red);
        }

        _texture.Apply();

        GetComponent<SpriteRenderer>().sprite =
         Sprite.Create(_texture, new Rect(0, 0, _textureSize, _textureSize), new Vector2(0.5f, 0.5f), 100f);
    }

    private void TryGenerateNewPoint()
    {
        Vector2Int activePoint = _activePoints[UnityEngine.Random.Range(0, _activePoints.Count)];

        Vector2Int? newPoint = null;

        for (int i = 0; i < 10; i++)
        {
            Vector2Int candidate = GenerateRandomAnnualPoint(activePoint);

            if (candidate.x < 0 || candidate.x >= _textureSize || candidate.y < 0 || candidate.y >= _textureSize)
            {
                continue;
            }

            if (!CheckPointsInRange(candidate))
            {
                newPoint = candidate;
                break;
            }
        }

        if (newPoint != null)
        {
            _baseGrid[(int)(newPoint?.x / _baseGridCellSize), (int)(newPoint?.y / _baseGridCellSize)] = (Vector2Int)newPoint;
            _activePoints.Add((Vector2Int)newPoint);
        }
        else
        {
            _basePoints.Add(activePoint);
            _activePoints.Remove(activePoint);
        }
    }

    // Generate random point in a ring around center point
    private Vector2Int GenerateRandomAnnualPoint(Vector2Int center)
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
        float distance = UnityEngine.Random.Range(_minimalBasePointSpacing, _minimalBasePointSpacing * 2);
        return center + new Vector2Int((int)(distance * Mathf.Sin(angle)), (int)(distance * Mathf.Cos(angle)));
    }

    // Returns true if found points in a circle around candidate
    private bool CheckPointsInRange(Vector2Int candidate)
    {
        int candidateGridX = (int)(candidate.x / _baseGridCellSize);
        int candidateGridY = (int)(candidate.y / _baseGridCellSize);

        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                if (math.abs(i) == 2 && math.abs(j) == 2 ||
                 candidateGridX + i < 0 || candidateGridX + i >= _baseGridSize ||
                  candidateGridY + j < 0 || candidateGridY + j >= _baseGridSize)
                {
                    continue;
                }

                Vector2Int point = _baseGrid[candidateGridX + i, candidateGridY + j];
                if (point != null && (point - candidate).magnitude <= _minimalBasePointSpacing)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
