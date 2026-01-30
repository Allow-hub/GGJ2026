using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

// フォルダ作成用のエディタウィンドウ
public class CreateFolderEditor : EditorWindow
{

    [MenuItem("Features/CreateFirstFolders")]
    static void Init()
    {
        // ウィンドウを表示する
        CreateFolderEditor window = (CreateFolderEditor)EditorWindow.GetWindow(typeof(CreateFolderEditor));
        window.Show();
    }

    // フォルダを作成する起点となるパス
    private string baseFolder = "Assets";

    // 作成するフォルダのリスト（通常）
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

    // 作成するフォルダのリスト（特殊フォルダ）
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

    // 各フォルダのチェック状態を管理するためのDictionary
    private Dictionary<string, bool> folderStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> extraFolderStates = new Dictionary<string, bool>();
    private Vector2 scrollPosition;

    // ウィンドウが有効になった時に呼ばれる
    private void OnEnable()
    {
        // Dictionaryを初期化し、すべてのフォルダをデフォルトでチェック状態（true）にする
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

    // ウィンドウのUIを描画する
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Project Folders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("作成したいフォルダを選択してください。\n" + baseFolder + " フォルダ直下に作成されます。", MessageType.Info);

        // ★変更点：ベースフォルダを指定するためのテキストフィールドを追加
        baseFolder = EditorGUILayout.TextField("Base Folder", baseFolder);
        EditorGUILayout.Space(); // UIに見やすいようにスペースを挿入


        // フォルダリストが長くなる可能性を考慮してスクロールビューを追加
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Standard Folders", EditorStyles.boldLabel);

        var folderKeys = new List<string>(folderStates.Keys);
        foreach (string folder in folderKeys)
        {
            // チェックボックス（トグル）を表示し、ユーザーの操作でDictionaryの値を更新
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

        // ボタンが押されたらフォルダ作成処理を呼び出す
        if (GUILayout.Button("Create Selected Folders"))
        {
            CreateProjectFolders();
            // 処理後にウィンドウを閉じると便利です
            this.Close();
        }
    }

    // フォルダを作成する実際の処理
    private void CreateProjectFolders()
    {
        // チェックされているフォルダをリストアップ
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
            // ★変更点：指定されたbaseFolderを起点にパスを結合
            string path = Path.Combine(baseFolder, folder);

            // フォルダがまだ存在しない場合のみ作成
            // Directory.CreateDirectoryは、途中のディレクトリもまとめて作成してくれます。
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

        // AssetDatabaseを更新して、作成したフォルダをエディタに表示させる
        AssetDatabase.Refresh();
        Debug.Log("Folder creation process finished.");
    }
}

