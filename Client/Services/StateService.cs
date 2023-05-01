namespace iSportsRecruiting.Client.Services
{
    public class StateService
    {
        public event Action OnStateChange;

        public void NotifyStateChange() => OnStateChange?.Invoke();
    }
}
