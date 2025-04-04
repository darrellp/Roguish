using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguish.ECS.Components;
internal class EquippedComponent : EcsComponent
{
    internal int Headgear { get; set; } = -1;
    internal int Footwear { get; set; } = -1;
    internal int Chest { get; set; } = -1;
    internal int LRing { get; set; } = -1;
    internal int RRing { get; set; } = -1;
    internal int WeaponLeft { get; set; } = -1;
    internal int WeaponRight { get; set; } = -1;
    internal int Arms { get; set; } = -1;
    internal int Legs { get; set; } = -1;
    internal int Amulet { get; set; } = -1;
    internal int Gloves { get; set; } = -1;
    internal int Belt { get; set; } = -1;
}
