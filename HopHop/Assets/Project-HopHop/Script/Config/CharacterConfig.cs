using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "character-config", menuName = "", order = 0)]
public class CharacterConfig : ScriptableObject
{
    [SerializeField] private string m_animatorPath = "Animation/Character";

    public ConfigCharacter Alphaca;
    public ConfigCharacter Angel;
    public ConfigCharacter Bug;
    public ConfigCharacter Bunny;
    public ConfigCharacter Cat;
    public ConfigCharacter Devil;
    public ConfigCharacter Fish;
    public ConfigCharacter Frog;
    public ConfigCharacter Mole;
    public ConfigCharacter Mow;
    public ConfigCharacter Pig;
    public ConfigCharacter Wolf;

    public ConfigCharacter GetConfig(CharacterType Character)
    {
        switch (Character)
        {
            case CharacterType.Alphaca:
                return Alphaca;
            case CharacterType.Angel:
                return Angel;
            case CharacterType.Bug:
                return Bug;
            case CharacterType.Bunny:
                return Bunny;
            case CharacterType.Cat:
                return Cat;
            case CharacterType.Devil:
                return Devil;
            case CharacterType.Fish:
                return Fish;
            case CharacterType.Frog:
                return Frog;
            case CharacterType.Mole:
                return Mole;
            case CharacterType.Mow:
                return Mow;
            case CharacterType.Pig:
                return Pig;
            case CharacterType.Wolf:
                return Wolf;
        }
        //
        return null;
    }

    public void SetLoadAnimator()
    {
        List<RuntimeAnimatorController> AnimatorGet;
        //
        GetConfig(CharacterType.Alphaca).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Alphaca", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Alphaca).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Angel).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Angel", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Angel).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Bug).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Bug", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Bug).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Bunny).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Bunny", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Bunny).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Cat).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Cat", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Cat).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Devil).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Devil", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Devil).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Fish).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Fish", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Fish).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Frog).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Frog", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Frog).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Mole).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Mole", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Mole).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Mow).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Mow", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Mow).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Pig).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Pig", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Pig).m_skin.Add(AnimatorGet[i]);
        //
        GetConfig(CharacterType.Wolf).m_skin.Clear();
        AnimatorGet = QAssetsDatabase.GetAnimatorController("Wolf", "Project-HopHop/" + m_animatorPath);
        for (int i = 0; i < AnimatorGet.Count; i++)
            GetConfig(CharacterType.Wolf).m_skin.Add(AnimatorGet[i]);
    }
}

public enum CharacterType
{
    Angel = 0,
    Devil = 1,
    Bunny = 2,
    Cat = 3,
    Frog = 4,
    Mow = 5,
    Alphaca = 6,
    Bug = 7,
    Fish = 8,
    Mole = 9,
    Pig = 10,
    Wolf = 11,
}

[Serializable]
public class ConfigCharacter
{
    public List<RuntimeAnimatorController> m_skin;
}

#if UNITY_EDITOR

[CustomEditor(typeof(CharacterConfig))]
public class CharacterConfigEditor : Editor
{
    private CharacterConfig Target;

    private void OnEnable()
    {
        Target = target as CharacterConfig;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //
        if (QEditor.SetButton("Refresh"))
            Target.SetLoadAnimator();
        //
        QEditorCustom.SetApply(this);
    }
}

#endif