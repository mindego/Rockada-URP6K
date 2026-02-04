using DWORD = System.UInt32;

public static class univars_dll
{
    public static iUnifiedVariable CreateByClassID(uint ClassId, iUniVarParent pParent, DWORD MemId)
    {
        switch (ClassId)
        {
            case iUnifiedVariableInt.ID: return new UniVarInt(pParent, MemId);
            case iUnifiedVariableFloat.ID: return new UniVarFloat(pParent, MemId);
            case iUnifiedVariableVector.ID: return new UniVarVector(pParent, MemId);
            case iUnifiedVariableString.ID: return new UniVarString(pParent, MemId);
            case iUnifiedVariableBlock.ID: return new UniVarBlock(pParent, MemId);
            case iUnifiedVariableReference.ID: return new UniVarReference(pParent, MemId);
            case iUnifiedVariableContainer.ID: return new UniVarContainer(pParent, MemId);
            case iUnifiedVariableArray.ID: return new UniVarArray(pParent, MemId);
            default: return null;
        }
    }

    public static string GetDescriptionByClassID(uint ClassId)
    {
        switch (ClassId)
        {
            case iUnifiedVariableInt.ID: return "iUnifiedVariableInt";
            case iUnifiedVariableFloat.ID: return "iUnifiedVariableFloat";
            case iUnifiedVariableVector.ID: return "iUnifiedVariableVector";
            case iUnifiedVariableString.ID: return "iUnifiedVariableString";
            case iUnifiedVariableBlock.ID: return "iUnifiedVariableBlock";
            case iUnifiedVariableReference.ID: return "iUnifiedVariableReference";
            case iUnifiedVariableContainer.ID: return "iUnifiedVariableContainer";
            case iUnifiedVariableArray.ID: return "iUnifiedVariableReference";
            case Constants.UndefinedID: return "Undefined";
            default: return "Unknown";
        }
    }
}

