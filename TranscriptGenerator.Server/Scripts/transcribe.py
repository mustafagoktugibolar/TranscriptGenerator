import argparse
import whisper
import os
import sys

# --- Argümanları al
parser = argparse.ArgumentParser()
parser.add_argument('--path', required=True, help='Path to audio file (mp3, mp4, wav, etc.)')
parser.add_argument('--model', default='base', help='Whisper model (tiny, base, small, medium, large)')
args = parser.parse_args()

# --- Dosya var mı kontrolü
if not os.path.exists(args.path):
    print(f"Error: File not found → {args.path}", file=sys.stderr)
    exit(1)

try:
    # --- Model yükle
    model = whisper.load_model(args.model)

    # --- Transcribe
    result = model.transcribe(args.path)

    # --- Sadece düz metin olarak yazdır
    print(result["text"])

except Exception as e:
    print(f"Error during transcription: {e}", file=sys.stderr)
    exit(1)
