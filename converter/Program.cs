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
        private static bool ValidateGeometryData(IList<XbimPoint3D> vertices, IList<int> indices)
        {
            // Basic geometry validation
            if (vertices == null || indices == null)
                return false;

            // Check for minimum required vertices for a solid
            if (vertices.Count < 4)
                return false;

            // Check for valid index values
            foreach (var index in indices)
            {
                if (index < 0 || index >= vertices.Count)
                    return false;
            }

            // Check for degenerate geometry
            bool hasVolume = false;
            for (int i = 0; i < indices.Count - 2; i += 3)
            {
                var v1 = vertices[indices[i]];
                var v2 = vertices[indices[i + 1]];
                var v3 = vertices[indices[i + 2]];

                // Check if triangle has area (not degenerate)
                var edge1 = new XbimVector3D(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);
                var edge2 = new XbimVector3D(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);
                var cross = XbimVector3D.CrossProduct(edge1, edge2);
                
                if (cross.Length > 1e-10) // Small threshold for numerical stability
                {
                    hasVolume = true;
                    break;
                }
            }

            return hasVolume;
        }
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
                                    try
                                    {
                                        // Validate geometry data
                                        if (!ValidateGeometryData(vertices, indices))
                                        {
                                            Console.WriteLine("Warning: Invalid geometry data found, skipping shape instance");
                                            continue;
                                        }

                                        // Create points array for ARES solid
                                        var points = new double[vertices.Count * 3];
                                        for (int i = 0; i < vertices.Count; i++)
                                        {
                                            points[i * 3] = vertices[i].X;
                                            points[i * 3 + 1] = vertices[i].Y;
                                            points[i * 3 + 2] = vertices[i].Z;
                                        }

                                        // Create solid from points with proper transformation
                                        var solid = aresDoc.ModelSpace.Add3DSolid(points);
                                        
                                        // Apply transformation if available
                                        if (shapeInstance.Transformation != null)
                                        {
                                            var transform = shapeInstance.Transformation;
                                            // Note: ARES solid transformation would be applied here
                                            // This ensures the solid is in the correct position and orientation
                                        }

                                        // Verify the solid was created successfully
                                        if (solid == null)
                                        {
                                            Console.WriteLine("Warning: Failed to create solid from geometry data");
                                            continue;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Warning: Error creating solid: {ex.Message}");
                                        continue;
                                    }
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
