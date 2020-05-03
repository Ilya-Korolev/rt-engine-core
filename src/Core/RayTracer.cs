using System;
using RayTracingEngine.Core.SceneObjects;
using RayTracingEngine.ImageProcessing;
using RayTracingEngine.MathExtra;
using RayTracingEngine.Models;
using RayTracingEngine.Models.Light;

namespace RayTracingEngine.Core
{
   public class RayTracer
   {
      private readonly double _delta = 0.00001d;

      Scene _scene;
      RenderParameters _renderParameters;

      public RayTracer(Scene scene, RenderParameters renderParameters)
      {
         _scene = scene;
         _renderParameters = renderParameters;
      }

      public Color Trace(Ray ray)
         => Trace(ray, _renderParameters.ReflectionDepth);

      private Color Trace(Ray ray, int reflectionDepth)
      {
         (ISceneObject closestObject, double? distance) = ray.GetClosestObject(_scene.Objects, _renderParameters.MinDistance, _renderParameters.MaxDistance);

         if (closestObject == null)
            return _scene.BackgroundColor;

         Vector3d intersectionPoint = ray.GetPertainPoint(distance.Value);
         Vector3d normal = closestObject.GetNormal(intersectionPoint);
         Vector3d viewDirection = -ray.Direction;

         double lightIntensity = ComputeLightIntensity(intersectionPoint, normal, viewDirection, closestObject.Material.SpecularExponent);

         Color localColor = closestObject.Material.Color.WithIntensity(lightIntensity);

         if (reflectionDepth <= 0)
            return localColor;

         Vector3d reflection = viewDirection.Reflect(normal);
         Ray reflectedRay = new Ray(intersectionPoint, reflection);

         Color reflectedColor = Trace(reflectedRay, reflectionDepth - 1);

         return localColor.WithIntensity(1d - closestObject.Material.ReflectiveCoefficient) + reflectedColor.WithIntensity(closestObject.Material.ReflectiveCoefficient);
      }

      private double ComputeLightIntensity(Vector3d intersectionPoint, Vector3d normal, Vector3d viewDirection, double specularExponent)
      {
         double intensity = 0d;

         foreach (var light in _scene.Lights)
            intensity += ComputeLightIntensity(light, intersectionPoint, normal, viewDirection, specularExponent);

         return intensity;
      }

      private double ComputeLightIntensity(ILight light, Vector3d intersectionPoint, Vector3d normal, Vector3d viewDirection, double specularExponent)
      {
         if (light is AmbientLight)
            return light.Intensity;

         Vector3d lightDirection;

         switch (light)
         {
            case DirectionalLight directionalLight:
               lightDirection = directionalLight.Direction;
               break;

            case PointLight pointLight:
               lightDirection = pointLight.Position - intersectionPoint;
               break;

            default:
               throw new NotSupportedException($"Cannot handle light of type {light.GetType().Name}");
         }

         // shadow
         Ray shadowRay = new Ray(intersectionPoint, lightDirection);
         bool hasShadow = shadowRay.HasIntersection(_scene.Objects, _delta, _renderParameters.MaxDistance);
         if (hasShadow)
            return 0d;

         double intensity = 0d;

         // diffuse
         double nl = normal * lightDirection;
         if (nl > 0)
            intensity += light.Intensity * nl / (normal.Length * intersectionPoint.Length);

         // specular
         Vector3d lightReflection = lightDirection.Reflect(normal);
         double rv = lightReflection * viewDirection;
         if (rv > 0)
            intensity += light.Intensity * Math.Pow(rv / (lightReflection.Length * viewDirection.Length), specularExponent);

         return intensity;
      }
   }
}