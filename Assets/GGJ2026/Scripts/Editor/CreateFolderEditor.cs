using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

// �t�H���_�쐬�p�̃G�f�B�^�E�B���h�E
public class CreateFolderEditor : EditorWindow
{

    [MenuItem("Features/CreateFirstFolders")]
    static void Init()
    {
        // �E�B���h�E��\������
        CreateFolderEditor window = (CreateFolderEditor)EditorWindow.GetWindow(typeof(CreateFolderEditor));
        window.Show();
    }

    // �t�H���_���쐬����N�_�ƂȂ�p�X
    private string baseFolder = "Assets/GGJ2026";

    // �쐬����t�H���_�̃��X�g�i�ʏ�j
    private readonly string[] folders = new string[]
    {
        "Animations",
        "Audio/BGM",
        "Audio/SE",
        "Fonts",
        "Materials",
        "Meshes",
        "Models",
        "PhysicsMaterials",
        "Prefabs",
        "Scenes",
        "Scripts",
        "ScriptableObjects",
        "Textures"
    };

    // �쐬����t�H���_�̃��X�g�i����t�H���_�j
    private readonly string[] extra_folders = new string[]
    {
        "Editor",
        "EditorDefaultResources",
        "Gizmos",
        "Plugins",
        "Resources",
        "StandardAssets",
        "StreamingAssets"
    };

    // �e�t�H���_�̃`�F�b�N��Ԃ��Ǘ����邽�߂�Dictionary
    private Dictionary<string, bool> folderStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> extraFolderStates = new Dictionary<string, bool>();
    private Vector2 scrollPosition;

    // �E�B���h�E���L���ɂȂ������ɌĂ΂��
    private void OnEnable()
    {
        // Dictionary�����������A���ׂẴt�H���_���f�t�H���g�Ń`�F�b�N��ԁitrue�j�ɂ���
        folderStates.Clear();
        foreach (var folder in folders)
        {
            folderStates[folder] = true;
        }

        extraFolderStates.Clear();
        foreach (var extra_folder in extra_folders)
        {
            extraFolderStates[extra_folder] = true;
        }
    }

    // �E�B���h�E��UI��`�悷��
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Project Folders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("�쐬�������t�H���_��I�����Ă��������B\n" + baseFolder + " �t�H���_�����ɍ쐬����܂��B", MessageType.Info);

        // ���ύX�_�F�x�[�X�t�H���_���w�肷�邽�߂̃e�L�X�g�t�B�[���h��ǉ�
        baseFolder = EditorGUILayout.TextField("Base Folder", baseFolder);
        EditorGUILayout.Space(); // UI�Ɍ��₷���悤�ɃX�y�[�X��}��


        // �t�H���_���X�g�������Ȃ�\�����l�����ăX�N���[���r���[��ǉ�
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Standard Folders", EditorStyles.boldLabel);

        var folderKeys = new List<string>(folderStates.Keys);
        foreach (string folder in folderKeys)
        {
            // �`�F�b�N�{�b�N�X�i�g�O���j��\�����A���[�U�[�̑����Dictionary�̒l���X�V
            folderStates[folder] = EditorGUILayout.ToggleLeft("  " + folder, folderStates[folder]);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Special Folders", EditorStyles.boldLabel);
        var extraFolderKeys = new List<string>(extraFolderStates.Keys);
        foreach (string extra_folder in extraFolderKeys)
        {
            extraFolderStates[extra_folder] = EditorGUILayout.ToggleLeft("  " + extra_folder, extraFolderStates[extra_folder]);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // �{�^���������ꂽ��t�H���_�쐬�������Ăяo��
        if (GUILayout.Button("Create Selected Folders"))
        {
            CreateProjectFolders();
            // ������ɃE�B���h�E�����ƕ֗��ł�
            this.Close();
        }
    }

    // �t�H���_���쐬������ۂ̏���
    private void CreateProjectFolders()
    {
        // �`�F�b�N����Ă���t�H���_�����X�g�A�b�v
        var foldersToCreate = new List<string>();
        foreach (var pair in folderStates)
        {
            if (pair.Value) foldersToCreate.Add(pair.Key);
        }
        foreach (var pair in extraFolderStates)
        {
            if (pair.Value) foldersToCreate.Add(pair.Key);
        }

        foreach (string folder in foldersToCreate)
        {
            // ���ύX�_�F�w�肳�ꂽbaseFolder���N�_�Ƀp�X������
            string path = Path.Combine(baseFolder, folder);

            // �t�H���_���܂����݂��Ȃ��ꍇ�̂ݍ쐬
            // Directory.CreateDirectory�́A�r���̃f�B���N�g�����܂Ƃ߂č쐬���Ă���܂��B
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log("Created folder: " + path);
            }
            else
            {
                Debug.LogWarning("Folder already exists: " + path);
            }
        }

        // AssetDatabase���X�V���āA�쐬�����t�H���_���G�f�B�^�ɕ\��������
        AssetDatabase.Refresh();
        Debug.Log("Folder creation process finished.");
    }
}

