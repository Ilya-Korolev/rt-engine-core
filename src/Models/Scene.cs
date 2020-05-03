using System.Collections.Generic;
using RayTracingEngine.Core.SceneObjects;
using RayTracingEngine.ImageProcessing;
using RayTracingEngine.Models.Light;

namespace RayTracingEngine.Models
{
   public class Scene
   {
      public Color BackgroundColor { get; set; }

      public List<ISceneObject> Objects { get; set; }

      public List<ILight> Lights { get; set; }

      public Scene()
      {
         BackgroundColor = new Color(0d, 0d, 0d, 1d);
         Objects = new List<ISceneObject>();
         Lights = new List<ILight>();
      }
   }
}