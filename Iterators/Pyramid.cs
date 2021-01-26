using Jobs.Implementations.Construction;
using Jobs.Implementations.Construction.Iterators;
using Pipliz;

namespace Improved_Construction
{
  public class Pyramid : IIterationType
  {
    protected ConstructionArea area;
    protected Vector3Int positionMin;
    protected Vector3Int positionMax;
    protected Vector3Int cursor;
    protected Vector3Int iterationChunkLocation;
    protected int iterationIndex;

    public Pyramid(ConstructionArea area)
    {
      this.area = area;
      this.positionMin = area.Minimum;
      this.positionMax = area.Maximum;
      this.iterationChunkLocation = new Vector3Int(this.positionMin.x & -16, this.positionMin.y & -16, this.positionMin.z & -16);
      this.iterationIndex = -1;
      this.MoveNext();
            Log.Write("Called to Pyramid!");
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
            int offset = location.y - this.positionMin.y;


            return 
                (((location.x - offset == this.positionMin.x || location.x + offset == this.positionMax.x) && location.z - offset >= this.positionMin.z && location.z + offset <= this.positionMax.z) ||
                 ((location.z - offset == this.positionMin.z || location.z + offset == this.positionMax.z) && location.x - offset >= this.positionMin.x && location.x + offset <= this.positionMax.x));
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
