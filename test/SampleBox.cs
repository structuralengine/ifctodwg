using System;
using System.IO;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.RepresentationResource;

namespace IFCtoDWGConverter.Test
{
    public class SampleBox
    {
        public static void CreateSampleIfc(string outputPath)
        {
            using (var model = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
            {
                using (var txn = model.BeginTransaction("Create Sample Box"))
                {
                    // Create basic model setup
                    var project = model.Instances.New<IfcProject>();
                    project.Initialize(ProjectUnits.SIUnitsUK);
                    
                    // Create geometric context
                    var context = model.Instances.New<IfcGeometricRepresentationContext>();
                    context.ContextType = "Model";
                    context.CoordinateSpaceDimension = 3;
                    context.Precision = 0.00001;
                    context.WorldCoordinateSystem = model.Instances.New<IfcAxis2Placement3D>();
                    project.RepresentationContexts.Add(context);

                    // Create a box
                    var boxProfile = model.Instances.New<IfcRectangleProfileDef>();
                    boxProfile.ProfileType = IfcProfileTypeEnum.AREA;
                    boxProfile.XDim = 1000; // 1m width
                    boxProfile.YDim = 1000; // 1m depth

                    var extrudeArea = model.Instances.New<IfcExtrudedAreaSolid>();
                    extrudeArea.SweptArea = boxProfile;
                    extrudeArea.Position = model.Instances.New<IfcAxis2Placement3D>();
                    extrudeArea.ExtrudedDirection = model.Instances.New<IfcDirection>();
                    extrudeArea.ExtrudedDirection.SetXYZ(0, 0, 1);
                    extrudeArea.Depth = 1000; // 1m height

                    // Create shape representation
                    var shapeRep = model.Instances.New<IfcShapeRepresentation>();
                    shapeRep.ContextOfItems = context;
                    shapeRep.RepresentationType = "SweptSolid";
                    shapeRep.Items.Add(extrudeArea);

                    // Create product definition
                    var productDef = model.Instances.New<IfcProductDefinitionShape>();
                    productDef.Representations.Add(shapeRep);

                    // Create a building element (box)
                    var box = model.Instances.New<IfcBuildingElementProxy>();
                    box.Name = "Sample Box";
                    box.Description = "A 1m x 1m x 1m box for testing";
                    box.ObjectPlacement = model.Instances.New<IfcLocalPlacement>();
                    box.Representation = productDef;

                    txn.Commit();
                }

                // Save the model
                model.SaveAs(outputPath);
            }
        }
    }
}
