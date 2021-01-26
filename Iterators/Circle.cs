using Jobs.Implementations.Construction;
using Jobs.Implementations.Construction.Iterators;
using Pipliz;

namespace Improved_Construction
{
  public class Circle : IIterationType
  {
    protected ConstructionArea area;
    protected Vector3Int positionMin;
    protected Vector3Int positionMax;
    protected Vector3Int cursor;
    protected Vector3Int iterationChunkLocation;
        protected Vector3Int center;
    protected int iterationIndex;
        protected double radius;


    public Circle(ConstructionArea area)
    {
      this.area = area;
      this.positionMin = area.Minimum;
      this.positionMax = area.Maximum;
      this.iterationChunkLocation = new Vector3Int(this.positionMin.x & -16, this.positionMin.y & -16, this.positionMin.z & -16);
      this.iterationIndex = -1;
            this.center = area.Minimum + ((area.Maximum - area.Minimum) / 2);

            this.radius = Math.Min(positionMax.x - positionMin.x, positionMax.z - positionMin.z) / 2;

      this.MoveNext();
    }

    public Vector3Int CurrentPosition
    {
      get
      {
        return this.cursor;
      }
    }

    public bool IsInBounds(Vector3Int location)
    {
            if (!(location.y >= this.positionMin.y && location.y <= this.positionMax.y))
                return false;
            if (!(location.x >= this.positionMin.x && location.x <= this.positionMax.x))
                return false;
            if (!(location.z >= this.positionMin.z && location.z <= this.positionMax.z))
                return false;


                Vector3Int offset = location - this.center;

                double distOff = Math.Abs(Math.Sqrt((double)(offset.x * offset.x) + (offset.z * offset.z)) - radius);
                return distOff <= 0.5;
    }

        

    public bool MoveNext()
    {
      do
      {
        ++this.iterationIndex;
        if (this.iterationIndex >= 4096)
        {
          this.iterationIndex = 0;
          this.iterationChunkLocation.z += 16;
          if (this.iterationChunkLocation.z > (this.positionMax.z & -16))
          {
            this.iterationChunkLocation.z = this.positionMin.z & -16;
            this.iterationChunkLocation.x += 16;
            if (this.iterationChunkLocation.x > (this.positionMax.x & -16))
            {
              this.iterationChunkLocation.x = this.positionMin.x & -16;
              this.iterationChunkLocation.y += 16;
              if (this.iterationChunkLocation.y > (this.positionMax.y & -16))
                return false;
            }
          }
        }
        this.cursor = IteratorHelper.ZOrderToPosition(this.iterationIndex).ToWorld(this.iterationChunkLocation);
      }
      while (!this.IsInBounds(this.cursor));
      return true;
    }
  }
}
