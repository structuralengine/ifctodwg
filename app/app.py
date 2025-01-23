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
    subprocess.run([CONVERTER_EXECUTABLE, ifc_path, dwg_path])

    # 変換後の DWG を送信
    return send_file(dwg_path, as_attachment=True)

if __name__ == '__main__':
    app.run(debug=True)
