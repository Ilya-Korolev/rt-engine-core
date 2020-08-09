using RayTracingEngine.MathExtra;
using RayTracingEngine.Models;

namespace RayTracingEngine.Core
{
    public abstract class SceneObject
    {
        public Material Material { get; set; } = new Material();

        internal abstract (double? firstDistance, double? secondDistance) IntersectRay(Ray ray);

        internal abstract Vector3d GetNormal(Vector3d point);
    }
}