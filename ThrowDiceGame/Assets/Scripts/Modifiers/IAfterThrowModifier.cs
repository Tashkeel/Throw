/// <summary>
/// Applied once after all dice have settled.
/// Return the modified total score from ApplyAfterThrow.
/// </summary>
public interface IAfterThrowModifier : IModifier
{
    int ApplyAfterThrow(AfterThrowContext ctx);
}
