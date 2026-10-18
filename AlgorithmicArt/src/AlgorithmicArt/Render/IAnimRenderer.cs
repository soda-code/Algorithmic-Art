using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>动画渲染器统一接口。</summary>
public interface IAnimRenderer
{
    void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current);
}
