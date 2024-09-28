using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlemage.Dungen
{
    public class DungenAuthoring : MonoBehaviour
    {
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _nodeSize;
        [SerializeField] private float _wallHeight;
        [SerializeField] private Material _material;
        [SerializeField] private Transform _enemySpawner;
        
        private void OnValidate()
        {
            if (_width % 2 == 0) _width++;
            if (_height % 2 == 0) _height++;
        }

        [Button("Generate")]
        private void Generate()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
                i--;
            }

            var paths = new bool[_width,_height];
            var verticalWalls = new bool[_width + 1,_height];
            var horizontalWalls = new bool[_width,_height + 1];

            var stack = new Stack<Vector2Int>();
            var start = new Vector2Int(_width / 2, _height / 2);
            stack.Push(start);
            paths[start.x, start.y] = true;
            
            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var directions = new[]
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 0),
                };
                Shuffle(directions);
                var moved = false;
                foreach (var direction in directions)
                {
                    var next = current + direction;
                    if (next.x < 0 || next.x >= _width || next.y < 0 || next.y >= _height) continue;
                    if (paths[next.x, next.y]) continue;
                    stack.Push(next);
                    paths[next.x, next.y] = true;
                    if (direction == new Vector2Int(1, 0))
                    {
                        verticalWalls[current.x + 1, current.y] = true;
                    }
                    else if (direction == new Vector2Int(-1, 0))
                    {
                        verticalWalls[current.x, current.y] = true;
                    }
                    else if (direction == new Vector2Int(0, 1))
                    {
                        horizontalWalls[current.x, current.y + 1] = true;
                    }
                    else if (direction == new Vector2Int(0, -1))
                    {
                        horizontalWalls[current.x, current.y] = true;
                    }

                    moved = true;
                    break;
                }

                if (!moved) stack.Pop();
            }

            _enemySpawner.position = new Vector3((_width - 1) * _nodeSize , 0, (_height - 1) * _nodeSize);
            
            var floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(_width * _nodeSize, 0, _height * _nodeSize));
            floor.transform.position = new Vector3(-_nodeSize / 2f, 0, -_nodeSize / 2f);
            floor.transform.SetParent(transform);
            floor.GetComponent<MeshRenderer>().material = _material;
            floor.gameObject.AddComponent<NavMeshModifier>();
            floor.gameObject.AddComponent<MeshCollider>();
            floor.gameObject.isStatic = true;

            for (var x = 0; x < _width + 1; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    if (verticalWalls[x, y]) continue;
                    var wall = ShapeGenerator.GenerateCube(PivotLocation.Center,
                        new Vector3(_nodeSize / 10f, _wallHeight, _nodeSize));
                    wall.transform.position = new Vector3(x * _nodeSize - _nodeSize / 2f, _wallHeight / 2f, y * _nodeSize);
                    wall.transform.SetParent(transform);
                    wall.GetComponent<MeshRenderer>().material = _material;
                    wall.gameObject.AddComponent<NavMeshModifier>();
                    wall.gameObject.AddComponent<BoxCollider>();
                    wall.gameObject.isStatic = true;
                }
            }
            
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height + 1; y++)
                {
                    if (horizontalWalls[x, y]) continue;
                    var wall = ShapeGenerator.GenerateCube(PivotLocation.Center,
                        new Vector3(_nodeSize, _wallHeight, _nodeSize / 10f));
                    wall.transform.position = new Vector3(x * _nodeSize, _wallHeight / 2f, y * _nodeSize - _nodeSize / 2f);
                    wall.transform.SetParent(transform);
                    wall.GetComponent<MeshRenderer>().material = _material;
                    wall.gameObject.AddComponent<NavMeshModifier>();
                    wall.gameObject.AddComponent<BoxCollider>();
                    wall.gameObject.isStatic = true;
                }
            }
        }
        
        private static void Shuffle(Vector2Int[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var j = Random.Range(0, array.Length);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}