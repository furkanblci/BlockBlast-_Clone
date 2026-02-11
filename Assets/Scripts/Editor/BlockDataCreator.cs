using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Editor utility to quickly create example block shapes.
/// Access via: Tools > Block Blast > Create Example Blocks
/// </summary>
public class BlockDataCreator : EditorWindow
{
    [MenuItem("Tools/Block Blast/Create Example Blocks")]
    public static void CreateExampleBlocks()
    {
        string folderPath = "Assets/Data/Blocks";
        
        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Blocks");
        }

        // Create example blocks
        CreateIBlock(folderPath);
        CreateLBlock(folderPath);
        CreateTBlock(folderPath);
        CreateSquareBlock(folderPath);
        CreateSingleCell(folderPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created example blocks in {folderPath}");
    }

    /// <summary>
    /// Helper method to add cells to a BlockData using reflection.
    /// Needed because Shape property is now IReadOnlyList.
    /// </summary>
    private static void AddCellToBlock(BlockData block, Vector2Int cell)
    {
        // Access the private serialized field using reflection
        FieldInfo shapeField = typeof(BlockData).GetField("shape", BindingFlags.NonPublic | BindingFlags.Instance);
        if (shapeField != null)
        {
            List<Vector2Int> shape = shapeField.GetValue(block) as List<Vector2Int>;
            if (shape != null)
            {
                shape.Add(cell);
            }
        }
    }

    private static void CreateIBlock(string folderPath)
    {
        BlockData block = ScriptableObject.CreateInstance<BlockData>();
        block.name = "Block_I";
        
        // Horizontal I-piece (4 cells in a row)
        AddCellToBlock(block, new Vector2Int(0, 0));
        AddCellToBlock(block, new Vector2Int(1, 0));
        AddCellToBlock(block, new Vector2Int(2, 0));
        AddCellToBlock(block, new Vector2Int(3, 0));
        
        AssetDatabase.CreateAsset(block, $"{folderPath}/Block_I.asset");
    }

    private static void CreateLBlock(string folderPath)
    {
        BlockData block = ScriptableObject.CreateInstance<BlockData>();
        block.name = "Block_L";
        
        // L-shape
        AddCellToBlock(block, new Vector2Int(0, 0));
        AddCellToBlock(block, new Vector2Int(0, 1));
        AddCellToBlock(block, new Vector2Int(0, 2));
        AddCellToBlock(block, new Vector2Int(1, 0));
        
        AssetDatabase.CreateAsset(block, $"{folderPath}/Block_L.asset");
    }

    private static void CreateTBlock(string folderPath)
    {
        BlockData block = ScriptableObject.CreateInstance<BlockData>();
        block.name = "Block_T";
        
        // T-shape
        AddCellToBlock(block, new Vector2Int(0, 1));
        AddCellToBlock(block, new Vector2Int(1, 1));
        AddCellToBlock(block, new Vector2Int(2, 1));
        AddCellToBlock(block, new Vector2Int(1, 0));
        
        AssetDatabase.CreateAsset(block, $"{folderPath}/Block_T.asset");
    }

    private static void CreateSquareBlock(string folderPath)
    {
        BlockData block = ScriptableObject.CreateInstance<BlockData>();
        block.name = "Block_Square";
        
        // 2x2 Square
        AddCellToBlock(block, new Vector2Int(0, 0));
        AddCellToBlock(block, new Vector2Int(1, 0));
        AddCellToBlock(block, new Vector2Int(0, 1));
        AddCellToBlock(block, new Vector2Int(1, 1));
        
        AssetDatabase.CreateAsset(block, $"{folderPath}/Block_Square.asset");
    }

    private static void CreateSingleCell(string folderPath)
    {
        BlockData block = ScriptableObject.CreateInstance<BlockData>();
        block.name = "Block_Single";
        
        // Single cell
        AddCellToBlock(block, new Vector2Int(0, 0));
        
        AssetDatabase.CreateAsset(block, $"{folderPath}/Block_Single.asset");
    }
}
