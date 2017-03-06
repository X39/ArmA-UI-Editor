using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVirtuality.SQF
{
    public static class Extensions
    {
        public static SqfNode GetParent(this SqfNode node)
        {
            SqfNode weak;
            if (node.ParentWeak.TryGetTarget(out weak))
                return weak;
            return default(SqfNode);
        }
        public static void SetParent(this SqfNode node, SqfNode parent)
        {
            node.ParentWeak.SetTarget(parent);
        }
        public static void AddChild(this SqfNode node, SqfNode newChild)
        {
            node.Children.Add(newChild);
        }
    }
}
