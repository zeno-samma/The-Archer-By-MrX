using System;

namespace MrX.Armory
{
    // Serialized values are stable. Hooded Rogue is a separate hero using the Rogue class.
    public enum HeroClass { Ranger = 0, Mage = 1, Barbarian = 2, Knight = 3, Rogue = 4 }
    [Flags]
    public enum HeroClasses { Ranger = 1, Mage = 2, Barbarian = 4, Knight = 8, Rogue = 16, All = 31 }
}
