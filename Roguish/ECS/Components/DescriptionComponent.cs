using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsRx.Components;

namespace Roguish.ECS.Components;

internal class DescriptionComponent(string name, string description) : IComponent
{
    public string Name = name;
    public string Description = description;
}