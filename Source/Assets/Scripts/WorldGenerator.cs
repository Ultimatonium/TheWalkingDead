using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Extensions;
using System;

public class WorldGenerator : MonoBehaviour
{
    private static GameObject[] roomsPrefab;
    private static GameObject startPrefab;
    private static GameObject endPrefab;
    private static GameObject pressurePlatePrefab;
    private static Material[] blood;
    private static Material standartMaterial;
    private static int size;

    private static GameObject world;
    private static float roomSize;
    public static GameObject[,] rooms;

    private enum RoomSearchType { asWall, asOpen, asExactWall, asExactOpen }
    private const int right = 1; /*0001*/
    private const int bottom = 2; /*0010*/
    private const int left = 4; /*0100*/
    private const int top = 8; /*1000*/

    private void Awake()
    {
        size = 5;
        rooms = new GameObject[size, size];
        for (int i = 0; i < rooms.GetLength(0); i++)
        {
            for (int ii = 0; ii < rooms.GetLength(1); ii++)
            {
                rooms[i, ii] = null;
            }
        }
        LoadRooms();
        if (roomsPrefab.Length > 0)
        {
            roomSize = roomsPrefab[1].transform.Find("Floor").lossyScale.x;
        }
    }

    private void LoadRooms()
    {
        roomsPrefab = Resources.LoadAll<GameObject>("Prefabs/Rooms");
        startPrefab = Resources.Load<GameObject>("Prefabs/Start");
        endPrefab = Resources.Load<GameObject>("Prefabs/End");
        pressurePlatePrefab = Resources.Load<GameObject>("Prefabs/PressurePlate");
        blood = Resources.LoadAll<Material>("External Assets/Blood decal pack/blood/Materials");
        standartMaterial = Resources.Load<Material>("External Assets/Angrave Delight/Materials/AngraveDelight_01");
    }

    public static void GenerateWorld()
    {
        world = new GameObject("World");
        world.transform.position = Vector3.zero;
        SetStartAndEnd();
        for (int heightCount = 1; heightCount <= size; heightCount++)
        {
            for (int widthCount = 1; widthCount <= size; widthCount++)
            {
                if (rooms[heightCount - 1, widthCount - 1] != null) continue;
                //top left corner
                if (heightCount == size && widthCount == 1)
                {
                    CreateRoom(FindRoom(top + left, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //top right corner
                if (heightCount == size && widthCount == size)
                {
                    CreateRoom(FindRoom(top + right, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //bottom left corner
                if (heightCount == 1 && widthCount == 1)
                {
                    CreateRoom(FindRoom(bottom + left, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //bottom right corner
                if (heightCount == 1 && widthCount == size)
                {
                    CreateRoom(FindRoom(bottom + right, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //top wall
                if (heightCount == size && (widthCount != 1 || widthCount != size))
                {
                    CreateRoom(FindRoom(top, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //left wall
                if (widthCount == 1 && (heightCount != 1 || heightCount != size))
                {
                    CreateRoom(FindRoom(left, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //right wall
                if (widthCount == size && (heightCount != 1 || heightCount != size))
                {
                    CreateRoom(FindRoom(right, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //bottom wall
                if (heightCount == 1 && (widthCount != 1 || widthCount != size))
                {
                    CreateRoom(FindRoom(bottom, RoomSearchType.asExactWall), heightCount, widthCount);
                    continue;
                }
                //middle field
                if (heightCount != 1 && heightCount != size && widthCount != 1 && widthCount != size)
                {
                    CreateRoom(GetRandomRoom(widthCount, heightCount), heightCount, widthCount);
                    continue;
                }
            }
        }
        AddObstacle();
        AddPressurePlate();
        PaintEnvoirement();
    }

    private static void PaintEnvoirement()
    {
        foreach (GameObject wall in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Wall"))
        {
            wall.GetComponent<MeshRenderer>().materials = new Material[] { standartMaterial, blood[UnityEngine.Random.Range(0, blood.Length)] };
        }
        foreach (GameObject floor in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Floor"))
        {
            floor.GetComponent<MeshRenderer>().materials = new Material[] { standartMaterial, blood[UnityEngine.Random.Range(0, blood.Length)] };
        }
        foreach (GameObject roof in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Roof"))
        {
            roof.GetComponent<MeshRenderer>().materials = new Material[] { standartMaterial, blood[UnityEngine.Random.Range(0, blood.Length)] };
        }
    }

    private static void AddPressurePlate()
    {
        Vector3 leftBorder = new Vector3(CalcNavBounds().center.x - CalcNavBounds().size.x / 2, 0, 0);
        Vector3 rightBorder = new Vector3(CalcNavBounds().center.x + CalcNavBounds().size.x / 2, 0, 0);
        Vector3 bottomBorder = new Vector3(0, 0, CalcNavBounds().center.z - CalcNavBounds().size.z / 2);
        Vector3 topBorder = new Vector3(0, 0, CalcNavBounds().center.z + CalcNavBounds().size.z / 2);
        for (int i = 0; i < size * 30; i++)
        {
            Vector3 randomPoint = new Vector3(UnityEngine.Random.Range(leftBorder.x, rightBorder.x), 0, UnityEngine.Random.Range(bottomBorder.z, topBorder.z));
            Instantiate(pressurePlatePrefab, randomPoint, Quaternion.identity, world.transform);
        }
    }

    private static void SetStartAndEnd()
    {
        Vector2 startPos = GetRandomPosition(2, size - 1);
        Vector2 endPos;
        do
        {
            endPos = GetRandomPosition(2, size - 1);
        } while (startPos == endPos);
        CreateRoom(startPrefab, (int)startPos.x, (int)startPos.y);
        CreateRoom(endPrefab, (int)endPos.x, (int)endPos.y);
    }

    private static Vector2 GetRandomPosition(int start, int end)
    {
        return new Vector2(UnityEngine.Random.Range(start, end), UnityEngine.Random.Range(start, end));
    }

    private static void AddObstacle()
    {
        foreach (GameObject wall in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Wall"))
        {
            NavMeshObstacle obstacle;
            obstacle = wall.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
            {
                obstacle = wall.AddComponent<NavMeshObstacle>();
            }
            obstacle.carving = true;
        }
    }

    private static void CreateRoom(GameObject roomPrefab, int heightCount, int widthCount)
    {
        if (roomPrefab != null)
        {
            GameObject newRoom = Instantiate(roomPrefab, new Vector3(widthCount * roomSize, 0, heightCount * roomSize), roomPrefab.transform.rotation);
            newRoom.name = roomPrefab.name;
            rooms[heightCount - 1, widthCount - 1] = newRoom;
            newRoom.transform.parent = world.transform;
        }
        else
        {
            Debug.LogWarning("roomPrefab is null");
        }
    }

    private static GameObject GetRandomRoom(int heightCount, int widthCount)
    {
        heightCount -= 1;
        widthCount -= 1;
        GameObject rightRoom = rooms[heightCount, widthCount + 1];
        GameObject bottomRoom = rooms[heightCount - 1, widthCount];
        GameObject leftRoom = rooms[heightCount, widthCount - 1];
        GameObject topRoom = rooms[heightCount + 1, widthCount];

        int rightNumber = left;
        int bottomNumber = top;
        int leftNumber = right;
        int topNumer = bottom;
        if (rightRoom != null) rightNumber = GetRoomNumber(rightRoom.name) & left;
        rightNumber = rightNumber >> 2;
        if (bottomRoom != null) bottomNumber = GetRoomNumber(bottomRoom.name) & top;
        bottomNumber = bottomNumber >> 2;
        if (leftRoom != null) leftNumber = GetRoomNumber(leftRoom.name) & right;
        rightNumber = rightNumber << 2;
        if (topRoom != null) topNumer = GetRoomNumber(topRoom.name) & bottom;
        topNumer = topNumer << 2;
        return FindRoom(rightNumber + bottomNumber + leftNumber + topNumer, RoomSearchType.asOpen);
    }

    private static GameObject FindRoom(int room, RoomSearchType roomSearchType)
    {
        if (room > 15 || room < 0) new ArgumentException();
        roomsPrefab.Shuffle();
        SetRoom15AtEnd();
        switch (roomSearchType)
        {
            case RoomSearchType.asWall:
                foreach (GameObject roomPrefab in roomsPrefab)
                {
                    if ((RoomReverse(GetRoomNumber(roomPrefab.name)) & room) == room)
                    {
                        return roomPrefab;
                    }
                }
                break;
            case RoomSearchType.asOpen:
                foreach (GameObject roomPrefab in roomsPrefab)
                {
                    if (((GetRoomNumber(roomPrefab.name)) & room) == room)
                    {
                        return roomPrefab;
                    }
                }
                break;
            case RoomSearchType.asExactWall:
                foreach (GameObject roomPrefab in roomsPrefab)
                {
                    if (RoomReverse(GetRoomNumber(roomPrefab.name)) == room)
                    {
                        return roomPrefab;
                    }
                }
                break;
            case RoomSearchType.asExactOpen:
                foreach (GameObject roomPrefab in roomsPrefab)
                {
                    if (GetRoomNumber(roomPrefab.name) == room)
                    {
                        return roomPrefab;
                    }
                }
                break;
            default:
                break;
        }
        return null;
    }

    private static void SetRoom15AtEnd()
    {
        GameObject room15 = null;
        for (int i = 0; i < roomsPrefab.Length; i++)
        {
            if (GetRoomNumber(roomsPrefab[i].name) == 15)
            {
                room15 = roomsPrefab[i];
            }
            if (room15 != null)
            {
                if (i < roomsPrefab.Length - 1)
                {
                    roomsPrefab[i] = roomsPrefab[i + 1];
                }
                else
                {
                    roomsPrefab[i] = room15;
                }
            }
        }
    }

    private static int GetRoomNumber(string name)
    {
        if (name == "Start") return 8;
        if (name == "End") return 2;
        if (name.Length <= 4) return int.MinValue;
        if (name.Length > 6) return int.MinValue;
        if (name.Substring(0, 4) != "Room") return int.MinValue;
        return int.Parse(name.Substring(4, name.Length - 4));
    }

    private static int RoomReverse(int number)
    {
        const int b15 = 15; /*1111*/
        return ~number & b15;
    }

    public static void GenerateNavMesh()
    {
        NavMeshData navMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(navMeshData);
        NavMeshBuilder.UpdateNavMeshData(navMeshData, NavMesh.GetSettingsByID(0), CollectMeshSource(), CalcNavBounds());
    }

    private static Bounds CalcNavBounds()
    {
        return new Bounds(new Vector3(1, 0, 1) * ((size + 1)) / 2 * roomSize, Vector3.one * size * roomSize);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(CalcNavBounds().center, CalcNavBounds().size);
    }

    private static List<NavMeshBuildSource> CollectMeshSource()
    {
        List<NavMeshBuildSource> meshSource = new List<NavMeshBuildSource>();
        foreach (GameObject floor in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Floor"))
        {
            meshSource.Add(new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = floor.GetComponent<MeshFilter>().sharedMesh,
                transform = floor.GetComponent<MeshFilter>().transform.localToWorldMatrix,
                area = 0
            });
        }
        return meshSource;
    }
}
