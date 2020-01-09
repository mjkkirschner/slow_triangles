using System.Drawing;

public interface IImage
{
    void Flip();
    int Width { get; }
    int Height { get; }
    Color[] Colors { get; }
}