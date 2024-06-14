using System.Collections.Generic;
using System.Windows;

namespace Panuon.WPF.UI.Internal.Controls
{
    public class WindowXMaskManager
    {
        public static Stack<int> GetMaskStack(WindowX obj)
        {
            return (Stack<int>)obj.GetValue(MaskStackPropertyKey.DependencyProperty);
        }
        public static readonly DependencyPropertyKey MaskStackPropertyKey = DependencyProperty.RegisterAttachedReadOnly("MaskStack", typeof(Stack<int>)
            , typeof(WindowXMaskManager), new PropertyMetadata(new Stack<int>()));
        public static void Push(WindowX windowX)
        {
            var stack = GetMaskStack(windowX);
            if (stack == null) return;
            stack.Push(1);
        }
        public static int Pop(WindowX windowX)
        {
            var stack = GetMaskStack(windowX);
            if (stack == null) return 0;
            stack.Pop();
            return stack.Count;
        }
    }
}
