using System;
using System.IO;
using System.Linq;
using Graebert.Ares;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace IFCtoDWGConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: IFCtoDWGConverter <input IFC file> <output DWG file>");
                return;
            }

            string ifcFilePath = args[0];
            string dwgFilePath = args[1];

            if (!File.Exists(ifcFilePath))
            {
                Console.WriteLine("Error: Input IFC file does not exist.");
                return;
            }

            try
            {
                using (var model = IfcStore.Open(ifcFilePath))
                {
                    // Create geometry context
                    var context = new Xbim3DModelContext(model);
                    context.CreateContext();

                    using (var aresDoc = new AresDocument())
                    {
                        aresDoc.NewDocument();

                        // Process each shape instance in the IFC model
                        foreach (var shapeInstance in context.ShapeInstances())
                        {
                            try
                            {
                                // Get the shape geometry
                                var geometry = shapeInstance.ShapeGeometry;
                                if (geometry == null) continue;

                                // Convert IFC geometry to ARES solid
                                var vertices = geometry.Vertices;
                                var indices = geometry.Indices;

                                if (vertices != null && indices != null && vertices.Count > 0 && indices.Count > 0)
                                {
                                    // Create a 3D solid in ARES from the mesh data
                                    // Note: This is a simplified example. You might need to handle different
                                    // geometry types and transformations differently
                                    var points = new double[vertices.Count * 3];
                                    for (int i = 0; i < vertices.Count; i++)
                                    {
                                        points[i * 3] = vertices[i].X;
                                        points[i * 3 + 1] = vertices[i].Y;
                                        points[i * 3 + 2] = vertices[i].Z;
                                    }

                                    // Create solid from points
                                    aresDoc.ModelSpace.Add3DSolid(points);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Warning: Failed to process shape instance: {ex.Message}");
                                continue;
                            }
                        }

                        aresDoc.SaveAs(dwgFilePath);
                        Console.WriteLine("Conversion successful: " + dwgFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during conversion: " + ex.Message);
            }
        }
    }
}
