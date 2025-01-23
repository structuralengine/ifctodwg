from flask import Flask, request, send_file
import os
import subprocess

UPLOAD_FOLDER = "uploads"
OUTPUT_FOLDER = "outputs"
CONVERTER_EXECUTABLE = "../converter/bin/Debug/IFCtoDWGConverter.exe"

app = Flask(__name__)
os.makedirs(UPLOAD_FOLDER, exist_ok=True)
os.makedirs(OUTPUT_FOLDER, exist_ok=True)

@app.route('/convert', methods=['POST'])
def convert_ifc_to_dwg():
    if 'file' not in request.files:
        return "No file uploaded", 400

    file = request.files['file']
    ifc_path = os.path.join(UPLOAD_FOLDER, file.filename)
    file.save(ifc_path)

    # C# コンバーターの呼び出し
    dwg_path = os.path.join(OUTPUT_FOLDER, file.filename.replace('.ifc', '.dwg'))
    
    # Run converter and capture output
    try:
        result = subprocess.run(
            [CONVERTER_EXECUTABLE, ifc_path, dwg_path],
            capture_output=True,
            text=True,
            check=True
        )
        
        # Check if the output contains any warnings
        if "Warning:" in result.stdout or "Warning:" in result.stderr:
            print("Conversion completed with warnings:")
            print(result.stdout)
            print(result.stderr)
        
        # Verify DWG file was created
        if not os.path.exists(dwg_path):
            return "Failed to create DWG file", 500
            
        # 変換後の DWG を送信
        return send_file(dwg_path, as_attachment=True)
        
    except subprocess.CalledProcessError as e:
        error_message = f"Conversion failed: {e.stdout}\n{e.stderr}"
        print(error_message)
        return error_message, 500
    except Exception as e:
        error_message = f"Unexpected error during conversion: {str(e)}"
        print(error_message)
        return error_message, 500
    finally:
        # Cleanup temporary files
        try:
            if os.path.exists(ifc_path):
                os.remove(ifc_path)
            if os.path.exists(dwg_path) and not os.path.getsize(dwg_path):
                os.remove(dwg_path)
        except Exception as e:
            print(f"Warning: Failed to cleanup temporary files: {str(e)}")

if __name__ == '__main__':
    app.run(debug=True)
