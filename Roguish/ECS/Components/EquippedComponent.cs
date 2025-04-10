using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguish.ECS.Components;
public class EquippedComponent : EcsComponent
{
    public int Headgear { get; set; } = -1;
    public int Footwear { get; set; } = -1;
    public int Chest { get; set; } = -1;
    public int LRing { get; set; } = -1;
    public int RRing { get; set; } = -1;
    public int WeaponLeft { get; set; } = -1;
    public int WeaponRight { get; set; } = -1;
    public int Arms { get; set; } = -1;
    public int Legs { get; set; } = -1;
    public int Amulet { get; set; } = -1;
    public int Gloves { get; set; } = -1;
    public int Belt { get; set; } = -1;
}
