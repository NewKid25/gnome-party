using System.Collections.Generic;

namespace CombatService
{
    public class PlannedAction
    {
        public Character_Base User { get; set; }
        public Action Attack { get; set; }
        public Character_Base Target { get; set; }
        public List<Character_Base> GroupTargets { get; set; }
        public bool DuplicateTargets { get; set; }
    }
}