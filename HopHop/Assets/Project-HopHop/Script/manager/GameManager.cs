using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{
    public static LevelConfig LevelConfig;
    public static CharacterConfig CharacterConfig;
    //
    [SerializeField] private LevelConfig m_levelConfig;
    [SerializeField] private CharacterConfig m_characterConfig;
    [SerializeField] private IsometricConfig m_isometricConfig;
    //
    [Space]
    [SerializeField] private IsometricManager m_isometricManager;

    #region Varible: Time

    public static float m_timeMove = 1.2f;
    public static float m_timeRatio = 1f;

    public static float TimeMove => m_timeMove * m_timeRatio;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        //
        LevelConfig = m_levelConfig;
        CharacterConfig = m_characterConfig;
        //
        Application.targetFrameRate = 60;
        //
        Time.timeScale = 2;
        //
        Screen.SetResolution(1920, 1080, true);
    }

    private void Start()
    {
        m_isometricManager.SetInit();
        m_isometricManager.List.SetList(m_isometricConfig, true);

        SetWorldLoad(m_levelConfig.Level[0].Level[0]);
    }

    //

    public void SetCameraFollow(Transform Target)
    {
        if (Camera.main == null)
        {
            Debug.Log("[Manager] Main camera not found, so can't change target!");
            return;
        }
        //
        if (Target == null)
            Camera.main.transform.parent = this.transform;
        else
            Camera.main.transform.parent = Target;
        //
        Camera.main.transform.localPosition = Vector3.back * 100f;
    }

    //
    private void SetWorldLoad(TextAsset WorldData)
    {
        StartCoroutine(ISetWorldLoad(WorldData));
    }

    private IEnumerator ISetWorldLoad(TextAsset WorldData)
    {
        m_isometricManager.World.SetWorldRemove(m_isometricManager.transform);

        yield return new WaitForSeconds(1f);

        IsometricDataFile.SetFileRead(m_isometricManager, WorldData);

        yield return new WaitForSeconds(1f);

        TurnManager.SetStart();
    }
}