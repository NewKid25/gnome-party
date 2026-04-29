using Models.CharacterData;

namespace Models
{
    public static class SocketEvents
    {
        public class ActionUpdatedEventArgs : EventArgs
        {
            public Character Character { get; set; }
        }

        public static event EventHandler<ActionUpdatedEventArgs> OnActionUpdated;

        public static void RaiseActionUpdated(Character character)
        {
            OnActionUpdated?.Invoke(null, new ActionUpdatedEventArgs 
            { 
                Character = character
            });
        }
    }
}
