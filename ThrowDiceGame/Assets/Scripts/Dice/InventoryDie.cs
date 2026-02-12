using UnityEngine;

public class InventoryDie
{
    public DiceData _dieType;
    private int[] _faceValues;
    private DieSideType[] _faceTypes;

    public InventoryDie(int[] faceValues, DiceData dieType, DieSideType[] faceTypes = null)
    {
        _faceValues = (int[])faceValues.Clone();
        _dieType = dieType;

        if (faceTypes != null)
        {
            _faceTypes = (DieSideType[])faceTypes.Clone();
        }
        else
        {
            _faceTypes = new DieSideType[faceValues.Length];
            for (int i = 0; i < _faceTypes.Length; i++)
                _faceTypes[i] = DieSideType.Score;
        }
    }

    /// <summary>
    /// Gets a copy of the face values array.
    /// </summary>
    public int[] GetFaceValues()
    {
        var copy = new int[_faceValues.Length];
        System.Array.Copy(_faceValues, copy, _faceValues.Length);
        return copy;
    }

    /// <summary>
    /// Gets a copy of the face types array.
    /// </summary>
    public DieSideType[] GetFaceTypes()
    {
        return (DieSideType[])_faceTypes.Clone();
    }

    public void UpgradeDie(IEnhancement enhancement)
    {
        _faceValues = enhancement.ApplyToDie(_faceValues);
    }
}
