using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    private const int MAX_ITEM_COUNT = 100;
    public GameItem[] m_items = null;
    public static GameData Instance = null;
    private Camera m_iconCamera = null;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GameData.Instance.Initialize(); //kts. Initialize()
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        m_iconCamera = GameObject.Find("IconCamera").GetComponent<Camera>();
        ItemCount = 0;
        m_items = new GameItem[MAX_ITEM_COUNT];
        
    }
    //tämä koska laitoin lecture 7:n samaan skeneen ja skeneä lataillaan uudestaan
    public void Initialize()
    {
        m_items = new GameItem[MAX_ITEM_COUNT];
        ItemCount = 0;
    }


    public void AddItem(GameItem item)
    {
        m_items[ItemCount] = item;
        ++ItemCount;
    }

    public GameItem GetItem(int index)
    {
        return m_items[index];
    }

    public Camera IconCamera
    {
        get
        {
            if (!m_iconCamera)
            {
                m_iconCamera = GameObject.Find("IconCamera").GetComponent<Camera>();
            }
            return m_iconCamera;
        }
    }

    public float PickRadius
    {
        get
        {
            return 1.0f;
        }
    }

    public int ItemCount { get; set; }
}
