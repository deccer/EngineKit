namespace EngineKit.Mathematics
{
    /// <summary>
    /// Allows to determine intersections with a <see cref="Plane"/>.
    /// </summary>
    public interface IIntersectableWithPlane
    {
        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref Plane plane);
    }
}
