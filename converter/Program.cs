using System;
using System.IO;
using Graebert.Ares;

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
                using (var aresDoc = new AresDocument())
                {
                    aresDoc.NewDocument();

                    // IFC ファイルの解析とジオメトリ追加（ダミーの例）
                    // 実際の IFC ジオメトリ処理は別途実装
                    aresDoc.ModelSpace.Add3DSolid(new double[] { 0, 0, 0, 1, 1, 1 });

                    aresDoc.SaveAs(dwgFilePath);
                    Console.WriteLine("Conversion successful: " + dwgFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during conversion: " + ex.Message);
            }
        }
    }
}
