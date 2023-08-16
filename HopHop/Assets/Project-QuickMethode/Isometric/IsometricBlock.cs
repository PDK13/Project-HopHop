using QuickMethode;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class IsometricBlock : MonoBehaviour
{
    #region Varible: Block Manager

    [Header("Manager")]
    [SerializeField] private string m_name = "";
    [SerializeField] private bool m_free = false;
    [SerializeField] private List<string> m_tag = new List<string>();

    #endregion

    #region Varible: World Manager

    [Header("World")]
    [SerializeField] private IsometricVector m_pos = new IsometricVector();

    private IsometricVector m_posPrimary = new IsometricVector();

    #endregion

    #region Varible: Data Manager

    [Header("Data")]
    [SerializeField] private IsometricDataBlockMove MoveData = new IsometricDataBlockMove();
    [SerializeField] private IsometricDataFollow FollowData = new IsometricDataFollow();
    [SerializeField] private IsometricDataBlockAction ActionData = new IsometricDataBlockAction();
    [SerializeField] private IsometricDataBlockEvent EventData = new IsometricDataBlockEvent();
    [SerializeField] private IsometricDataBlockTeleport TeleportData = new IsometricDataBlockTeleport();

    #endregion

    #region Varible: Scene Manager

    [Header("Scene")]
    [SerializeField] private IsometricGameDataScene m_scene = new IsometricGameDataScene();
    [SerializeField] private Vector3 m_centre = new Vector3();

    #endregion

    private IsometricManager m_worldManager;

    #region ================================================================== Mono

#if UNITY_EDITOR

    private void Update()
    {
        SetIsoTransform();
    }

#endif

    #endregion

    #region ================================================================== World Manager

    public bool Free => m_free;

    public string Name => m_name != "" ? m_name : QGameObject.GetNameReplaceClone(this.name);

    public List<string> Tag => m_tag;

    public IsometricManager WorldManager 
    { 
        get => m_worldManager;
        set
        {
            m_worldManager = value;
            m_scene = value.GameData.Scene;
        }
    }

    #endregion

    #region ================================================================== World Manager

    public IsometricVector Pos { get => m_pos; set { m_pos = value; SetIsoTransform(); } }

    public IsometricVector PosPrimary { get => m_posPrimary; set => m_posPrimary = value; }

    #endregion

    #region ================================================================== Data Manager

    public IsometricDataFileBlockData Data 
    {
        get 
        {
            IsometricDataFileBlockData Data = new IsometricDataFileBlockData();
            Data.Move = MoveData;
            Data.Follow = FollowData;
            Data.Action = ActionData;
            Data.Event = EventData;
            Data.Teleport = TeleportData;
            return Data;
        }
        set
        {
            MoveData = value.Move;
            FollowData = value.Follow;
            ActionData = value.Action;
            EventData = value.Event;
            TeleportData = value.Teleport;
        }
    }

    #endregion

    #region ================================================================== Scene Manager

    private Vector3 GetIsoTransform(IsometricVector Pos)
    {
        IsometricVector PosCentre = m_scene.Centre;
        float Angle = 0;
        //
        switch (m_scene.Rotate)
        {
            case IsometricRotateType._0:
                Angle = 0 * Mathf.Deg2Rad;
                break;
            case IsometricRotateType._90:
                Angle = 90f * Mathf.Deg2Rad;
                break;
            case IsometricRotateType._180:
                Angle = 180f * Mathf.Deg2Rad;
                break;
            case IsometricRotateType._270:
                Angle = 270f * Mathf.Deg2Rad;
                break;
        }
        IsometricVector PosValue = new IsometricVector(Pos);
        PosValue.X = (Pos.X - PosCentre.X) * Mathf.Cos(Angle) - (Pos.Y - PosCentre.Y) * Mathf.Sin(Angle) + PosCentre.X;
        PosValue.Y = (Pos.X - PosCentre.X) * Mathf.Sin(Angle) + (Pos.Y - PosCentre.Y) * Mathf.Cos(Angle) + PosCentre.Y;
        //
        Vector3 PosTransform = new Vector3();
        IsometricVector PosValueScale = PosValue;
        //
        //
        switch (m_scene.Renderer)
        {
            case IsometricRendererType.H:
                PosValueScale.X *= m_scene.Scale.X * 0.5f * -1;
                PosValueScale.Y *= m_scene.Scale.Y * 0.5f;
                PosValueScale.H *= m_scene.Scale.H * 0.5f;
                //
                PosTransform.x = PosValueScale.X + PosValueScale.Y;
                PosTransform.y = 0.5f * (PosValueScale.Y - PosValueScale.X) + PosValueScale.H;
                PosTransform.z = PosValue.X + PosValue.Y - PosValue.H;
                //
                break;
            case IsometricRendererType.XY:
                PosValueScale.X *= m_scene.Scale.X * 0.5f * -1;
                PosValueScale.Y *= m_scene.Scale.Y * 0.5f;
                PosValueScale.H *= m_scene.Scale.H * 0.5f;
                //
                PosTransform.x = PosValueScale.X + PosValueScale.Y;
                PosTransform.y = 0.5f * (PosValueScale.Y - PosValueScale.X) + PosValueScale.H;
                PosTransform.z = (PosValue.Y + PosValue.X) - PosValue.H * 2;
                //
                break;
            case IsometricRendererType.None: //Testing
                PosValueScale.X *= m_scene.Scale.X * 0.5f * -1;
                PosValueScale.Y *= m_scene.Scale.Y * 0.5f;
                PosValueScale.H *= m_scene.Scale.H * 0.5f;
                //
                PosTransform.x = PosValueScale.X + PosValueScale.Y;
                PosTransform.y = 0.5f * (PosValueScale.Y - PosValueScale.X) + PosValueScale.H;
                PosTransform.z = 0;
                //
                break;
        }
        //
        return PosTransform;
    }

    private void SetIsoTransform()
    {
        if (WorldManager != null)
            m_scene = WorldManager.GameData.Scene;

        Vector3 PosTransform = GetIsoTransform(m_pos);

        PosTransform += (Vector3)m_centre;

        transform.position = PosTransform;
    }

    #endregion

    #region ================================================================== Check

    public List<IsometricBlock> GetCheck(IsometricVector Dir, int Length)
    {
        return WorldManager.WorldData.GetBlockCurrentAll(Pos.Fixed + Dir * Length);
    }

    public List<IsometricBlock> GetCheck(IsometricVector Dir, int Length, params string[] TagFind)
    {
        return WorldManager.WorldData.GetBlockCurrentAll(Pos.Fixed + Dir * Length, TagFind);
    }

    #endregion
}