namespace PSDL
{
    interface IRoad
    {
        int RowCount { get; }
        int RowBreadth { get; }

        Vertex[] GetRow(int num);
        void AddRow(Vertex[] vertices);
        void SetRow(int row, Vertex[] vertices);
        Vertex GetRowCenterPoint(int row);
        void SetTexture(RoadTextureType type, string texture);
        string GetTexture(RoadTextureType type);
        Vertex[] GetSidewalkBoundary(int rowNum);
        void DeleteSidewalk(SidewalkRemovalMode mode);
    }
}
