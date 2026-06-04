public struct InventoryAddResult
{
    public int Requested;
    public int Added;

    public int Remaining => Requested - Added;
    public bool FullyAdded => Added >= Requested && Requested > 0;
    public bool AnyAdded => Added > 0;

    public static InventoryAddResult None(int requested)
    {
        return new InventoryAddResult { Requested = requested, Added = 0 };
    }
}
