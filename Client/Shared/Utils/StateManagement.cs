namespace iSportsRecruiting.Client.Shared.Utils;

public static class StateManagement<T>
{
    public static event Action RenderRequested;
    
    private static readonly Dictionary<string, List<(Action<T> StateHasChanged, int Sender)>> RenderGroups = new();

    public static void AddRender(string renderGroup, Action<T> stateHasChanged, int sender)
    {
        if (RenderGroups.ContainsKey(renderGroup))
        {
            if (RenderGroups[renderGroup].Any(r => r.Sender == sender))
            {
                RenderGroups[renderGroup] = new List<(Action<T> StateHasChanged, int Sender)>
                    {(stateHasChanged, sender)};
                return;
            }
            
            RenderGroups[renderGroup].Add((stateHasChanged, sender));
        }
        else
            RenderGroups.Add(renderGroup, new List<(Action<T> StateHasChanged, int Sender)> { (stateHasChanged, sender) });
    }

    public static void ExecuteRender(string renderGroup, T shared)
    {
        if (!RenderGroups.ContainsKey(renderGroup))
            return;
        
        foreach (var render in RenderGroups[renderGroup])
            render.StateHasChanged(shared);
    }
}