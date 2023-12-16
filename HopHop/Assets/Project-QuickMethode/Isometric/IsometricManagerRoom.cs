using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class IsometricManagerRoom
{
    public const string NAME_CURSON = "iso-curson";
    public const string NAME_ROOM = "iso-room-";

    private IsometricManager m_manager;
    //
    [SerializeField] private string m_name;
    [SerializeField] private Transform m_root;
    private bool m_active = true;

    public string Name { get => m_name; set => m_name = value; }

    public string NameFixed => string.Format("{0}{1}", NAME_ROOM, m_name);

    public Transform Root => m_root;

    public bool Active
    {
        get
        {
            if (m_root == null)
            {
                Debug.LogFormat("[Isometric] Room not exist for excute command!");
                return false;
            }
            m_root.gameObject.SetActive(m_active);
            return m_active;
        }
        set
        {
            if (m_root == null)
            {
                m_active = false;
                Debug.LogFormat("[Isometric] Room not exist for excute command!");
                return;
            }
            m_active = value;
            m_root.gameObject.SetActive(m_active);
        }
    }
    
    public Action onCreate;
    public Action onRemove;

    public List<string> Command = new List<string>();
    public List<IsometricDataRoomPosH> PosH;
    public List<IsometricDataRoomTag> Tag;
    
    public IsometricManagerRoom(IsometricManager Manager, string Name)
    {
        m_manager = Manager;
        m_name = Name;
        //
        m_root = new GameObject(NameFixed).transform;
        m_root.transform.parent = m_manager.transform;
        //
        PosH = new List<IsometricDataRoomPosH>();
        Tag = new List<IsometricDataRoomTag>();
    }

    public IsometricManagerRoom(IsometricManager Manager, Transform Root)
    {
        if (!Root.name.Contains(NAME_ROOM))
        {
            Debug.LogFormat("[Isometric] Manager can't add {0} at a room in world", Root.name);
            return;
        }
        //
        m_manager = Manager;
        m_name = Root.name;
        //
        m_root = Root;
        //
        PosH = new List<IsometricDataRoomPosH>();
        Tag = new List<IsometricDataRoomTag>();
    }

    public void SetDestroy()
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return;
        }
        //
        MonoBehaviour.Destroy(m_root.gameObject);
    }

    #region ======================================================================== Block

    #region Block Create

    public IsometricBlock SetBlockCreate(IsometricVector Pos, GameObject BlockPrefab, IsometricDataFileBlockData Data = null)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return null;
        }
        //
        if (BlockPrefab == null)
        {
            Debug.LogWarningFormat("Block {0} not found!", Pos.Encypt);
            return null;
        }

        if (BlockPrefab.GetComponent<IsometricBlock>() == null)
        {
            Debug.LogWarningFormat("Block {0} {1} not found IsometricBlock!", Pos.Encypt, BlockPrefab.name);
            return null;
        }

        //Create
        GameObject BlockObject = QGameObject.SetCreate(BlockPrefab);

        //Block
        IsometricBlock Block = QComponent.GetComponent<IsometricBlock>(BlockObject);
        Block.WorldManager = m_manager;
        Block.Pos = Pos.Fixed;
        Block.PosPrimary = Pos.Fixed;

        //Block Data
        Block.Data = Data != null ? Data : new IsometricDataFileBlockData();

        if (Block.PosType == IsometricPosType.Free && Application.isPlaying)
        {
            //When in playing, FREE Block's Pos Primary will not be track, so just can be find by it own Tag!
        }
        else
        {
            //Delete
            SetBlockRemovePrimary(Pos.Fixed);

            //World
            int IndexPosH = GetWorldIndexPosH(Pos.HInt);
            if (IndexPosH == -1)
            {
                PosH.Add(new IsometricDataRoomPosH(Pos.Fixed.HInt, new List<IsometricBlock>()));
                IndexPosH = PosH.Count - 1;
                PosH[IndexPosH].Block.Add(Block);
            }
            else
            {
                PosH[IndexPosH].Block.Add(Block);
            }
        }

        //Tag
        List<string> TagFind = Block.GetComponent<IsometricBlock>().Tag;
        if (TagFind.Count == 0)
        {
            //None Tag!
            int TagIndex = GetWorldIndexTag("");
            if (TagIndex == -1)
            {
                Tag.Add(new IsometricDataRoomTag("", new List<IsometricBlock>()));
                TagIndex = Tag.Count - 1;
                Tag[TagIndex].Block.Add(Block);
            }
            else
            {
                Tag[TagIndex].Block.Add(Block);
            }
        }
        else
        {
            //Got Tag!
            foreach (string TagCheck in TagFind)
            {
                int TagIndex = GetWorldIndexTag(TagCheck);
                if (TagIndex == -1)
                {
                    Tag.Add(new IsometricDataRoomTag(TagCheck));
                    TagIndex = Tag.Count - 1;
                    Tag[TagIndex].Block.Add(Block);
                }
                else
                {
                    Tag[TagIndex].Block.Add(Block);
                }
            }
        }

        //Scene
        Transform ParentPosH = m_root.Find(GetWorldNamePosH(Pos.Fixed));
        if (ParentPosH != null)
        {
            Block.transform.SetParent(ParentPosH, true);
        }
        else
        {
            ParentPosH = QGameObject.SetCreate(GetWorldNamePosH(Pos.Fixed), m_root).transform;
            Block.transform.SetParent(ParentPosH, true);
        }

        return Block;
    }

    #endregion

    #region Block Get

    public IsometricBlock GetBlockPrimary(IsometricVector Pos)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return null;
        }
        //
        //World
        int IndexPosH = GetWorldIndexPosH(Pos.Fixed.HInt);
        if (IndexPosH == -1)
        {
            return null;
        }

        for (int i = 0; i < PosH[IndexPosH].Block.Count; i++)
        {
            if (PosH[IndexPosH].Block[i].PosPrimary != Pos.Fixed)
            {
                continue;
            }

            return PosH[IndexPosH].Block[i];
        }

        return null;
    }

    public IsometricBlock GetBlockCurrent(IsometricVector Pos, params string[] Tag)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return null;
        }
        //
        if (Tag.Length > 0)
        {
            //Find all Block with know tag - More Quickly!!
            foreach (string TagFind in Tag)
            {
                int TagIndex = GetWorldIndexTag(TagFind);
                if (TagIndex == -1)
                {
                    //Not exist Tag in Tag List!
                    continue;
                }

                for (int BlockIndex = 0; BlockIndex < this.Tag[TagIndex].Block.Count; BlockIndex++)
                {
                    if (this.Tag[TagIndex].Block[BlockIndex].Pos.Fixed != Pos.Fixed)
                    {
                        continue;
                    }

                    return this.Tag[TagIndex].Block[BlockIndex];
                }
            }
        }
        else
        {
            //Find all block with unknow tag - More slower!! (But always found Block)
            foreach (IsometricDataRoomTag TagCheck in this.Tag)
            {
                foreach (IsometricBlock BlockCheck in TagCheck.Block)
                {
                    if (BlockCheck.Pos.Fixed != Pos.Fixed)
                    {
                        continue;
                    }

                    return BlockCheck;
                }
            }
        }

        return null;
    }

    public List<IsometricBlock> GetBlockCurrentAll(IsometricVector Pos, params string[] Tag)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return null;
        }
        //
        List<IsometricBlock> List = new List<IsometricBlock>();

        if (Tag.Length > 0)
        {
            //Find all Block with know tag - More Quickly!!
            foreach (string TagFind in Tag)
            {
                int TagIndex = GetWorldIndexTag(TagFind);
                if (TagIndex == -1)
                {
                    //Not exist Tag in Tag List!
                    continue;
                }

                for (int BlockIndex = 0; BlockIndex < this.Tag[TagIndex].Block.Count; BlockIndex++)
                {
                    if (this.Tag[TagIndex].Block[BlockIndex].Pos.Fixed != Pos.Fixed)
                    {
                        continue;
                    }

                    List.Add(this.Tag[TagIndex].Block[BlockIndex]);
                }
            }
        }
        else
        {
            //Find all block with unknow tag - More slower!! (But always found Block)
            foreach (IsometricDataRoomTag TagCheck in this.Tag)
            {
                foreach (IsometricBlock BlockCheck in TagCheck.Block)
                {
                    if (BlockCheck.Pos.Fixed != Pos.Fixed)
                    {
                        continue;
                    }

                    List.Add(BlockCheck);
                }
            }
        }

        return List;
    }

    public List<IsometricBlock> GetBlockCurrentAll(string Tag)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return null;
        }
        //
        foreach (IsometricDataRoomTag Check in this.Tag)
        {
            if (Check.Tag != Tag)
            {
                continue;
            }

            return Check.Block;
        }
        return null;
    }

    #endregion

    #region Block Remove

    public void SetBlockRemovePrimary(IsometricVector Pos, float Delay = 0)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return;
        }
        //
        //World
        int IndexPosH = GetWorldIndexPosH(Pos.Fixed.HInt);
        if (IndexPosH == -1)
        {
            return;
        }

        for (int i = 0; i < PosH[IndexPosH].Block.Count; i++)
        {
            if (PosH[IndexPosH].Block[i].PosPrimary != Pos.Fixed)
            {
                continue;
            }

            IsometricBlock Block = PosH[IndexPosH].Block[i];

            //World
            PosH[IndexPosH].Block.Remove(Block);
            if (PosH[IndexPosH].Block.Count == 0)
            {
                PosH.RemoveAt(IndexPosH);
            }

            //Tag
            List<string> TagFind = Block.Tag;
            foreach (string TagCheck in TagFind)
            {
                int TagIndex = GetWorldIndexTag(TagCheck);
                if (TagIndex != -1)
                {
                    Tag[TagIndex].Block.Remove(Block);
                    if (Tag[TagIndex].Block.Count == 0)
                    {
                        Tag.RemoveAt(TagIndex);
                    }
                }
            }

            //Scene
            if (Application.isEditor && !Application.isPlaying)
            {
                GameObject.DestroyImmediate(Block.gameObject);
            }
            else
            {
                GameObject.Destroy(Block.gameObject, Delay);
            }

            break;
        }
    }

    public void SetBlockRemoveInstant(IsometricBlock Block, float Delay)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return;
        }
        //
        if (Block.PosType == IsometricPosType.Track)
        {
            //World
            PosH[GetWorldIndexPosH(Block.Pos.HInt)].Block.Remove(Block);
        }

        //Tag
        foreach (string TagCheck in Block.Tag)
        {
            Tag[GetWorldIndexTag(TagCheck)].Block.Remove(Block);
        }

        //Scene
        if (Application.isEditor && !Application.isPlaying)
        {
            GameObject.DestroyImmediate(Block.gameObject);
        }
        else
        {
            GameObject.Destroy(Block.gameObject, Delay);
        }
    }

    #endregion

    #endregion

    #region ======================================================================== World

    #region World Read

    public bool SetWorldRead()
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return false;
        }
        //
        //Clear Current World!!
        SetWorldRemove();

        //Store Block(s) Found!!
        List<IsometricBlock> BlockFound = m_root.GetComponentsInChildren<IsometricBlock>().ToList();
        GameObject BlockStore = QGameObject.SetCreate("BlockStore");
        foreach (IsometricBlock Block in BlockFound)
        {
            if (Block.gameObject.name == NAME_CURSON)
            {
                continue;
            }

            Block.transform.SetParent(BlockStore.transform);
        }
        //
        //Remove All GameObject!!
        for (int i = m_root.transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (m_root.GetChild(i).gameObject.name == NAME_CURSON)
            {
                continue;
            }
#endif
            if (m_root.GetChild(i).GetComponent<Camera>() != null)
            {
                continue;
            }

            if (Application.isEditor && !Application.isPlaying)
            {
                GameObject.DestroyImmediate(m_root.GetChild(i).gameObject);
            }
            else
            {
                GameObject.Destroy(m_root.GetChild(i).gameObject);
            }
        }
        //
        //Add Block(s) Found!!
        foreach (IsometricBlock Block in BlockFound)
        {
            if (Block.gameObject.name == NAME_CURSON)
            {
                continue;
            }

            SetWorldReadBlock(Block);
        }
        //
        //Destroy Block(s) Store!!
        if (Application.isEditor && !Application.isPlaying)
        {
            GameObject.DestroyImmediate(BlockStore);
        }
        else
        {
            GameObject.Destroy(BlockStore);
        }
        //
        onCreate?.Invoke();
        //
        return true;
    }

    public void SetWorldReadBlock(IsometricBlock Block)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return;
        }
        //
        Block.WorldManager = m_manager;
        Block.PosPrimary = Block.Pos;

        //World
        int IndexPosH = GetWorldIndexPosH(Block.Pos.HInt);
        if (IndexPosH == -1)
        {
            PosH.Add(new IsometricDataRoomPosH(Block.Pos.HInt));
            IndexPosH = PosH.Count - 1;
            PosH[IndexPosH].Block.Add(Block);
        }
        else
        {
            PosH[IndexPosH].Block.Add(Block);
        }

        //Tag
        List<string> TagFind = Block.GetComponent<IsometricBlock>().Tag;
        foreach (string TagCheck in TagFind)
        {
            int TagIndex = GetWorldIndexTag(TagCheck);
            if (TagIndex == -1)
            {
                Tag.Add(new IsometricDataRoomTag(TagCheck));
                IndexPosH = Tag.Count - 1;
                Tag[IndexPosH].Block.Add(Block);
            }
            else
            {
                Tag[TagIndex].Block.Add(Block);
            }
        }

        //Scene
        Transform ParentPosH = m_root.Find(GetWorldNamePosH(Block.Pos));
        if (ParentPosH != null)
        {
            Block.transform.SetParent(ParentPosH, true);
        }
        else
        {
            ParentPosH = QGameObject.SetCreate(GetWorldNamePosH(Block.Pos), m_root).transform;
            Block.transform.SetParent(ParentPosH, true);
        }
    }

    #endregion

    #region World Remove

    public void SetWorldRemove(bool Full = false)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return;
        }
        //
        for (int i = PosH.Count - 1; i >= 0; i--)
        {
            for (int j = PosH[i].Block.Count - 1; j >= 0; j--)
            {
                IsometricBlock Block = PosH[i].Block[j];

                if (Block == null)
                {
                    continue;
                }

                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(Block.gameObject);
                }
                else
                {
                    GameObject.Destroy(Block.gameObject);
                }
            }
        }
        PosH.Clear();

        for (int i = Tag.Count - 1; i >= 0; i--)
        {
            for (int j = Tag[i].Block.Count - 1; j >= 0; j--)
            {
                IsometricBlock Block = Tag[i].Block[j];

                if (Block == null)
                {
                    continue;
                }

                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(Block.gameObject);
                }
                else
                {
                    GameObject.Destroy(Block.gameObject);
                }
            }
        }
        Tag.Clear();

        if (Full)
        {
            //Remove All GameObject!!
            for (int i = m_root.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (m_root.GetChild(i).gameObject.name == NAME_CURSON)
                {
                    continue;
                }
#endif
                if (m_root.GetChild(i).GetComponent<Camera>() != null)
                {
                    continue;
                }

                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(m_root.GetChild(i).gameObject);
                }
                else
                {
                    GameObject.Destroy(m_root.GetChild(i).gameObject);
                }
            }
        }

        onRemove?.Invoke();
    }

    #endregion

    #region World Progess

    private int GetWorldIndexPosH(int PosH)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return 0;
        }
        //
        for (int i = 0; i < this.PosH.Count; i++)
        {
            if (this.PosH[i].PosH != PosH)
            {
                continue;
            }

            return i;
        }
        return -1;
    }

    private int GetWorldIndexTag(string Tag)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return 0;
        }
        //
        for (int i = 0; i < this.Tag.Count; i++)
        {
            if (this.Tag[i].Tag != Tag)
            {
                continue;
            }

            return i;
        }
        return -1;
    }

    private string GetWorldNamePosH(IsometricVector Pos)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return "";
        }
        //
        return Pos.HInt.ToString();
    }

    public void SetWorldOrder()
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return;
        }
        //
        PosH = PosH.OrderByDescending(h => h.PosH).ToList();
        for (int i = 0; i < PosH.Count; i++)
        {
            PosH[i] = new IsometricDataRoomPosH(PosH[i].PosH, PosH[i].Block.OrderByDescending(a => a.Pos.X).OrderByDescending(b => b.Pos.Y).ToList());
        }
    }

    #endregion

    #endregion

    #region ======================================================================== Editor

    public bool SetEditorMask(IsometricVector Pos, Color Mask, Color UnMask, Color Centre)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return false;
        }
        //
        bool CentreFound = false;
        for (int i = 0; i < PosH.Count; i++)
        {
            for (int j = 0; j < PosH[i].Block.Count; j++)
            {
                IsometricBlock Block = PosH[i].Block[j].GetComponent<IsometricBlock>();
                if (Block == null)
                    continue;
                //
                if (PosH[i].Block[j].Pos == Pos)
                {
                    CentreFound = true;
                    Block.SetSpriteColor(Centre, 1f);
                }
                else
                if (PosH[i].Block[j].Pos.X == Pos.X || PosH[i].Block[j].Pos.Y == Pos.Y)
                    Block.SetSpriteColor(Mask, 1f);
                else
                    Block.SetSpriteColor(UnMask, 1f);
            }
        }
        //
        return CentreFound;
    }

    public void SetEditorHidden(int FromH, float UnMask)
    {
        if (m_root == null)
        {
            Debug.LogFormat("[Isometric] Room not exist for excute command!");
            return;
        }
        //
        for (int i = 0; i < PosH.Count; i++)
        {
            for (int j = 0; j < PosH[i].Block.Count; j++)
            {
                IsometricBlock Block = PosH[i].Block[j].GetComponent<IsometricBlock>();
                if (Block == null)
                    continue;
                //
                if (PosH[i].Block[j].Pos.H > FromH)
                    Block.SetSpriteAlpha(UnMask);
                else
                    Block.SetSpriteAlpha(1f);
            }
        }
    }

    #endregion
}

[Serializable]
public class IsometricDataRoomPosH
{
    public int PosH;
    public List<IsometricBlock> Block;

    public IsometricDataRoomPosH(int PosH)
    {
        this.PosH = PosH;
        Block = new List<IsometricBlock>();
    }

    public IsometricDataRoomPosH(int PosH, List<IsometricBlock> Block)
    {
        this.PosH = PosH;
        this.Block = Block;
    }
}

[Serializable]
public class IsometricDataRoomTag
{
    public string Tag;
    public List<IsometricBlock> Block;

    public IsometricDataRoomTag(string Tag)
    {
        this.Tag = Tag;
        Block = new List<IsometricBlock>();
    }

    public IsometricDataRoomTag(string Tag, List<IsometricBlock> Block)
    {
        this.Tag = Tag;
        this.Block = Block;
    }
}