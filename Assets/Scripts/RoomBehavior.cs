using UnityEngine;

public class RoomBehavior : MonoBehaviour
{
    [Header("Walls - Tường kín hoàn toàn (Up/Down/Right/Left)")]
    public GameObject[] walls = new GameObject[4];   // Tường kín (ẩn khi có cửa)

    [Header("Entrances - Khung cửa / hốc cửa (Up/Down/Right/Left)")]
    public GameObject[] entrances = new GameObject[4]; // Khung/hốc cửa (bật khi có lối đi)

    [Header("Doors - Chỉ cánh cửa (Up/Down/Right/Left)")]
    public GameObject[] doors = new GameObject[4];     // Cánh cửa (bật khi có lối đi)

    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            bool hasDoor = status[i];

            // Tường kín: bật khi KHÔNG có lối đi
            if (walls[i] != null)
            {
                walls[i].SetActive(!hasDoor);
            }

            // Khung cửa / hốc cửa: bật khi CÓ lối đi
            if (entrances[i] != null)
            {
                entrances[i].SetActive(hasDoor);
            }

            // Cánh cửa: bật khi CÓ lối đi
            if (doors[i] != null)
            {
                doors[i].SetActive(hasDoor);
            }
        }
    }
}