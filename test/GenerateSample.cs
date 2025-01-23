using System;

namespace IFCtoDWGConverter.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputPath = "sample.ifc";
            if (args.Length > 0)
            {
                outputPath = args[0];
            }

            try
            {
                SampleBox.CreateSampleIfc(outputPath);
                Console.WriteLine($"Sample IFC file created successfully at: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sample IFC file: {ex.Message}");
            }
        }
    }
}
